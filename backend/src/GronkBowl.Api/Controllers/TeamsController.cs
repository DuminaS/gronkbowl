using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/teams")]
public class TeamsController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public TeamsController(GronkBowlDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<TeamSummaryDto>>> GetAll() =>
        await _db.Teams.Select(t => new TeamSummaryDto(t.Id, t.Name, t.Gold, t.CapSpace)).ToListAsync();

    [HttpGet("{teamId:guid}")]
    public async Task<ActionResult<TeamSummaryDto>> GetOne(Guid teamId)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team is null)
        {
            return NotFound();
        }

        return new TeamSummaryDto(team.Id, team.Name, team.Gold, team.CapSpace);
    }

    [HttpGet("{teamId:guid}/roster")]
    public async Task<ActionResult<List<RosterEntryDto>>> GetRoster(Guid teamId)
    {
        var team = await _db.Teams.FindAsync(teamId);
        if (team is null)
        {
            return NotFound();
        }

        var playerIds = team.Roster.DepthChart.Values.SelectMany(ids => ids).ToHashSet();
        var players = await _db.Players.Where(p => playerIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var entries = team.Roster.DepthChart
            .Select(kvp => new RosterEntryDto(
                kvp.Key.ToString(),
                kvp.Value.Where(players.ContainsKey).Select(id => DtoMapping.ToDto(players[id])).ToList()))
            .OrderBy(entry => entry.Position)
            .ToList();

        return entries;
    }

    [HttpGet("{teamId:guid}/playbook")]
    public async Task<ActionResult<List<PlayDto>>> GetPlaybook(Guid teamId)
    {
        var team = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == teamId);
        if (team is null)
        {
            return NotFound();
        }

        return team.Playbook.Select(DtoMapping.ToDto).ToList();
    }
}
