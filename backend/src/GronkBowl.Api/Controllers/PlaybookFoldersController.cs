using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

/// <summary>
/// A coach's own playbook organization - named folders of plays, replacing the old fixed
/// SituationalBucket call sheet. Standing team state (create/rename/file plays whenever), not a
/// weekly submission - what a coach actually calls happens live, one down at a time, through
/// LiveMatchController.
/// </summary>
[ApiController]
[Route("api/teams/{teamId:guid}/folders")]
public class PlaybookFoldersController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public PlaybookFoldersController(GronkBowlDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<PlaybookFolderDto>>> GetAll(Guid teamId)
    {
        if (!await _db.Teams.AnyAsync(t => t.Id == teamId))
        {
            return NotFound();
        }

        var folders = await _db.PlaybookFolders.Where(f => f.TeamId == teamId).ToListAsync();
        return folders.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<PlaybookFolderDto>> Create(Guid teamId, [FromBody] CreateFolderRequest request)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team is null)
        {
            return NotFound();
        }

        if (!Enum.TryParse<PlayCategory>(request.Category, ignoreCase: true, out var category))
        {
            return BadRequest("Category must be Offense or Defense.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("A folder needs a name.");
        }

        var folder = new PlaybookFolder { TeamId = teamId, Category = category, Name = request.Name.Trim() };
        _db.PlaybookFolders.Add(folder);
        await _db.SaveChangesAsync();
        return ToDto(folder);
    }

    /// <summary>Renames a folder and/or replaces which plays it contains (and their order).
    /// Play ids that aren't actually installed in this team's Playbook are silently dropped
    /// rather than trusted, same policy the old call sheet endpoint used.</summary>
    [HttpPut("{folderId:guid}")]
    public async Task<ActionResult<PlaybookFolderDto>> Update(Guid teamId, Guid folderId, [FromBody] UpdateFolderRequest request)
    {
        var folder = await _db.PlaybookFolders.FirstOrDefaultAsync(f => f.Id == folderId && f.TeamId == teamId);
        if (folder is null)
        {
            return NotFound();
        }

        var team = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == teamId);
        if (team is null)
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            folder.Name = request.Name.Trim();
        }

        if (request.PlayIds is not null)
        {
            var installedIds = team.Playbook.Where(p => p.Category == folder.Category).Select(p => p.Id).ToHashSet();
            folder.PlayIds.Clear();
            folder.PlayIds.AddRange(request.PlayIds.Where(installedIds.Contains));
        }

        await _db.SaveChangesAsync();
        return ToDto(folder);
    }

    [HttpDelete("{folderId:guid}")]
    public async Task<IActionResult> Delete(Guid teamId, Guid folderId)
    {
        var folder = await _db.PlaybookFolders.FirstOrDefaultAsync(f => f.Id == folderId && f.TeamId == teamId);
        if (folder is null)
        {
            return NotFound();
        }

        _db.PlaybookFolders.Remove(folder);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    internal static PlaybookFolderDto ToDto(PlaybookFolder folder) =>
        new(folder.Id, folder.TeamId, folder.Category.ToString(), folder.Name, folder.PlayIds);
}
