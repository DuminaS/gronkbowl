using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// Converts one game's box score into Blood Bowl-style Star Player Points, adapted from BB's
/// own award table (Touchdown 3, Causing Casualty 2, Interception 2, Completion 1, MVP 4 - one
/// per team, chosen at random among that team's participants) onto our stat line. Defensive
/// SPP deliberately comes only from impact plays (turnovers forced, injuries caused), not
/// routine tackles - matching Blood Bowl's own design that tackling is the job, not stardom.
/// </summary>
public static class SppAwarder
{
    private const int TouchdownSpp = 3;
    private const int TurnoverForcedSpp = 2;
    private const int InjuryCausedSpp = 2;
    private const int ReceptionSpp = 1;
    private const int MvpSpp = 4;

    public static void AwardSppForGame(
        IReadOnlyDictionary<Guid, PlayerGameStats> gameStats,
        Team homeTeam, Team awayTeam,
        IReadOnlyDictionary<Guid, Player> players,
        Random rng)
    {
        foreach (var (playerId, stats) in gameStats)
        {
            if (!players.TryGetValue(playerId, out var player))
            {
                continue;
            }

            PlayerProgression.RecordGamePlayed(player);

            var spp = TouchdownSpp * (stats.RushingTouchdowns + stats.ReceivingTouchdowns)
                + TurnoverForcedSpp * stats.TurnoversForced
                + InjuryCausedSpp * stats.InjuriesCaused
                + ReceptionSpp * stats.Receptions;

            if (spp > 0)
            {
                PlayerProgression.AwardSpp(player, spp);
            }
        }

        AwardMvp(homeTeam, gameStats, players, rng);
        AwardMvp(awayTeam, gameStats, players, rng);
    }

    private static void AwardMvp(
        Team team, IReadOnlyDictionary<Guid, PlayerGameStats> gameStats,
        IReadOnlyDictionary<Guid, Player> players, Random rng)
    {
        var participants = team.Roster.DepthChart.Values
            .SelectMany(ids => ids)
            .Where(gameStats.ContainsKey)
            .ToList();

        if (participants.Count == 0)
        {
            return;
        }

        var mvpId = participants[rng.Next(participants.Count)];
        if (players.TryGetValue(mvpId, out var mvp))
        {
            PlayerProgression.AwardSpp(mvp, MvpSpp);
        }
    }
}
