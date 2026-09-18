using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// A basic one-for-one player trade between two rosters. Both sides must clear a cap check
/// after the swap - a trade that would push either team over its cap is refused outright,
/// not partially applied. Contracts travel with the player; only roster membership changes.
/// </summary>
public static class TradeProcessor
{
    public static bool TryExecute(
        Team teamA, Guid playerAId,
        Team teamB, Guid playerBId,
        IReadOnlyDictionary<Guid, Player> players)
    {
        if (!players.TryGetValue(playerAId, out var playerA) || !players.TryGetValue(playerBId, out var playerB))
        {
            return false;
        }

        var playerAGold = playerA.Contract?.AnnualGold ?? 0;
        var playerBGold = playerB.Contract?.AnnualGold ?? 0;

        // Each team sheds the salary of the player it trades away and takes on the salary of
        // the player it receives.
        var teamACapAfter = teamA.CapSpace + playerAGold - playerBGold;
        var teamBCapAfter = teamB.CapSpace + playerBGold - playerAGold;

        if (teamACapAfter < 0 || teamBCapAfter < 0)
        {
            return false;
        }

        SwapInDepthChart(teamA.Roster, playerA.Position, outgoingId: playerAId, incomingId: playerBId);
        SwapInDepthChart(teamB.Roster, playerB.Position, outgoingId: playerBId, incomingId: playerAId);

        teamA.CapSpace = teamACapAfter;
        teamB.CapSpace = teamBCapAfter;

        return true;
    }

    private static void SwapInDepthChart(Roster roster, FootballPosition position, Guid outgoingId, Guid incomingId)
    {
        if (!roster.DepthChart.TryGetValue(position, out var depth))
        {
            depth = new List<Guid>();
            roster.DepthChart[position] = depth;
        }

        var index = depth.IndexOf(outgoingId);
        if (index >= 0)
        {
            depth[index] = incomingId;
        }
        else
        {
            depth.Add(incomingId);
        }
    }
}
