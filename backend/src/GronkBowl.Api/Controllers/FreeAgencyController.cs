using GronkBowl.Domain;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/freeagents")]
public class FreeAgencyController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public FreeAgencyController(GronkBowlDbContext db) => _db = db;

    /// <summary>A free agent is any player not on a team's roster and not part of an
    /// in-progress draft class - derived, not tracked separately.</summary>
    [HttpGet]
    public async Task<ActionResult<List<PlayerDto>>> GetAvailable()
    {
        var rosteredIds = (await _db.Teams.ToListAsync())
            .SelectMany(t => t.Roster.DepthChart.Values.SelectMany(ids => ids).Concat(t.Roster.PracticeSquad))
            .ToHashSet();

        var activeDraftProspectIds = (await _db.DraftStates.ToListAsync())
            .Where(d => !d.IsComplete)
            .SelectMany(d => d.ProspectPlayerIds.Except(d.DraftedProspectPlayerIds))
            .ToHashSet();

        var players = await _db.Players.ToListAsync();
        var freeAgents = players.Where(p => !rosteredIds.Contains(p.Id) && !activeDraftProspectIds.Contains(p.Id));

        return freeAgents.Select(DtoMapping.ToDto).ToList();
    }

    /// <summary>
    /// Signs a free agent using the real FreeAgencyResolver (Master Design Doc s7) - with a
    /// single offer on the table it always wins, same as it would among several competing
    /// offers, so this reuses the tested resolution logic rather than a shortcut.
    /// </summary>
    [HttpPost("{playerId:guid}/sign")]
    public async Task<ActionResult<PlayerDto>> Sign(Guid playerId, [FromBody] SignFreeAgentRequest request)
    {
        var player = await _db.Players.FindAsync(playerId);
        var team = await _db.Teams.FindAsync(request.TeamId);
        if (player is null || team is null)
        {
            return NotFound();
        }

        var contract = new Contract(request.AnnualGold, request.Years, request.GuaranteedGold, request.SigningBonus);
        var offer = new FreeAgentOffer(request.TeamId, contract);
        var winner = FreeAgencyResolver.ResolveSigning(new[] { offer }, new Random());

        if (winner is null)
        {
            return BadRequest("No valid offer.");
        }

        player.Contract = winner.Contract;
        team.Roster.AddToDepthChart(player.Position, player.Id);

        await _db.SaveChangesAsync();
        return DtoMapping.ToDto(player);
    }
}
