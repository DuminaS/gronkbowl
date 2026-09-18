using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/teams/{teamId:guid}/callsheet")]
public class CallSheetsController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public CallSheetsController(GronkBowlDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<CallSheetDto?>> Get(Guid teamId)
    {
        if (!await _db.Teams.AnyAsync(t => t.Id == teamId))
        {
            return NotFound();
        }

        var sheet = await _db.CallSheets
            .Where(c => c.TeamId == teamId)
            .OrderByDescending(c => c.Week)
            .FirstOrDefaultAsync();

        return sheet is null ? Ok(null) : ToDto(sheet);
    }

    /// <summary>
    /// Replaces the team's call sheet for the given week. Manual only, matching the design
    /// doc's "no AI auto-fill" rule - every entry here is exactly what the coach chose. Any
    /// play id that isn't actually in the team's installed Playbook is silently dropped rather
    /// than trusted, since the client can't be relied on to only send legal plays.
    /// </summary>
    [HttpPut]
    public async Task<ActionResult<CallSheetDto>> Submit(Guid teamId, [FromBody] SubmitCallSheetRequest request)
    {
        var team = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == teamId);
        if (team is null)
        {
            return NotFound();
        }

        var installedPlayIds = team.Playbook.Select(p => p.Id).ToHashSet();

        var callSheet = await _db.CallSheets.FirstOrDefaultAsync(c => c.TeamId == teamId && c.Week == request.Week);
        var isNew = callSheet is null;
        callSheet ??= new CallSheet { TeamId = teamId, Week = request.Week };
        callSheet.Submitted = true;
        callSheet.WasRandomlyGenerated = false;

        PopulateBuckets(callSheet.OffensiveSituationalPlays, request.OffensiveSituationalPlays, installedPlayIds);
        PopulateBuckets(callSheet.DefensiveSituationalPlays, request.DefensiveSituationalPlays, installedPlayIds);

        if (isNew)
        {
            _db.CallSheets.Add(callSheet);
        }

        await _db.SaveChangesAsync();
        return ToDto(callSheet);
    }

    private static void PopulateBuckets(
        Dictionary<SituationalBucket, List<Guid>> target,
        Dictionary<string, List<Guid>> source,
        HashSet<Guid> installedPlayIds)
    {
        foreach (var (bucketName, playIds) in source)
        {
            if (Enum.TryParse<SituationalBucket>(bucketName, ignoreCase: true, out var bucket))
            {
                target[bucket] = playIds.Where(installedPlayIds.Contains).ToList();
            }
        }
    }

    private static CallSheetDto ToDto(CallSheet callSheet)
    {
        var buckets = Enum.GetValues<SituationalBucket>()
            .Select(bucket => new CallSheetBucketDto(
                bucket.ToString(),
                callSheet.OffensiveSituationalPlays.TryGetValue(bucket, out var offense) ? offense : new List<Guid>(),
                callSheet.DefensiveSituationalPlays.TryGetValue(bucket, out var defense) ? defense : new List<Guid>()))
            .ToList();

        return new CallSheetDto(callSheet.TeamId, callSheet.Week, callSheet.Submitted, callSheet.WasRandomlyGenerated, buckets);
    }
}
