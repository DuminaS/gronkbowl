using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/trades")]
public class TradesController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public TradesController(GronkBowlDbContext db) => _db = db;

    /// <summary>A one-for-one trade via TradeProcessor - refused outright (not partially
    /// applied) if either side would end up over its cap.</summary>
    [HttpPost]
    public async Task<ActionResult<TradeResultDto>> Execute([FromBody] TradeRequest request)
    {
        var teamA = await _db.Teams.FindAsync(request.TeamAId);
        var teamB = await _db.Teams.FindAsync(request.TeamBId);
        if (teamA is null || teamB is null)
        {
            return NotFound();
        }

        var players = await _db.Players.ToDictionaryAsync(p => p.Id);
        var succeeded = TradeProcessor.TryExecute(teamA, request.PlayerAId, teamB, request.PlayerBId, players);

        if (succeeded)
        {
            await _db.SaveChangesAsync();
        }

        return new TradeResultDto(succeeded, succeeded ? "Trade completed." : "Trade refused - would exceed a team's cap, or a player id was invalid.");
    }
}
