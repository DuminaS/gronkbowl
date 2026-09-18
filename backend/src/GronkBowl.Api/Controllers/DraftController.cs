using GronkBowl.Domain;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/draft")]
public class DraftController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public DraftController(GronkBowlDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<DraftStateDto?>> GetCurrent()
    {
        // PickOrder.Count and IsComplete can't be translated into SQL (the column is an opaque
        // jsonb blob to EF) - load the (small) set of draft states and pick in memory.
        var draft = (await _db.DraftStates.ToListAsync()).OrderByDescending(d => d.CurrentPickIndex).FirstOrDefault();
        if (draft is null)
        {
            return Ok(null);
        }

        return await ToDto(draft);
    }

    /// <summary>
    /// Starts a new draft: generates a class, orders picks worst-record-first (see
    /// DraftOrderCalculator), and persists it. Refuses to start a second draft while one is
    /// still in progress, since only one draft board makes sense at a time.
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<DraftStateDto>> Start([FromBody] StartDraftRequest request)
    {
        if ((await _db.DraftStates.ToListAsync()).Any(d => !d.IsComplete))
        {
            return Conflict("A draft is already in progress.");
        }

        var season = await _db.Seasons.FirstOrDefaultAsync();
        if (season is null)
        {
            return NotFound("No season exists yet.");
        }

        var completedGames = await _db.Matches.CountAsync(m => m.SeasonId == season.Id);
        if (!SeasonPhaseCalculator.IsRegularSeasonComplete(season, completedGames))
        {
            return Conflict("The rookie draft opens once the regular season is complete.");
        }

        var teamIds = await _db.Teams.Select(t => t.Id).ToListAsync();
        var draftOrder = DraftOrderCalculator.FromStandings(season.Standings, teamIds);

        var rng = new Random();
        var prospects = DraftProspectGenerator.GenerateClass(request.ProspectCount, rng);
        _db.Players.AddRange(prospects);

        var draft = new DraftState
        {
            SeasonId = season.Id,
            PickOrder = Enumerable.Range(0, request.Rounds).SelectMany(_ => draftOrder).ToList(),
            ProspectPlayerIds = prospects.Select(p => p.Id).ToList(),
        };
        _db.DraftStates.Add(draft);

        await _db.SaveChangesAsync();
        return await ToDto(draft);
    }

    /// <summary>Only the team on the clock can pick, and only an undrafted prospect from this
    /// class - both are refused rather than silently corrected.</summary>
    [HttpPost("pick")]
    public async Task<ActionResult<DraftPickResultDto>> Pick([FromBody] DraftPickRequest request)
    {
        var draft = (await _db.DraftStates.ToListAsync()).OrderByDescending(d => d.CurrentPickIndex).FirstOrDefault();
        if (draft is null || draft.IsComplete)
        {
            return BadRequest("No active draft.");
        }

        if (draft.OnTheClock != request.TeamId)
        {
            return BadRequest("That team is not on the clock.");
        }

        if (!draft.ProspectPlayerIds.Contains(request.ProspectId) || draft.DraftedProspectPlayerIds.Contains(request.ProspectId))
        {
            return BadRequest("That prospect is not available in this draft.");
        }

        var team = await _db.Teams.FindAsync(request.TeamId);
        var prospect = await _db.Players.FindAsync(request.ProspectId);
        if (team is null || prospect is null)
        {
            return NotFound();
        }

        var contract = RookieWageScale.ForPick(draft.OverallPickNumber);
        prospect.Contract = contract;
        team.Roster.AddToDepthChart(prospect.Position, prospect.Id);

        draft.DraftedProspectPlayerIds.Add(prospect.Id);
        draft.CurrentPickIndex++;

        await _db.SaveChangesAsync();

        return new DraftPickResultDto(DtoMapping.ToDto(prospect), draft.OverallPickNumber - 1, contract.AnnualGold, contract.Years);
    }

    private async Task<DraftStateDto> ToDto(DraftState draft)
    {
        var availableIds = draft.ProspectPlayerIds.Except(draft.DraftedProspectPlayerIds).ToHashSet();
        var available = await _db.Players.Where(p => availableIds.Contains(p.Id)).ToListAsync();

        return new DraftStateDto(
            draft.Id,
            draft.OnTheClock,
            draft.OverallPickNumber,
            draft.PickOrder.Count,
            draft.IsComplete,
            available.Select(DtoMapping.ToDto).ToList());
    }
}
