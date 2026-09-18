using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// Derives everything stat-related from a Match's EventLog - the log is the one source of
/// truth, box scores and career totals are always rebuildable from it. See Master Design Doc s4.
/// </summary>
public static class StatsAggregator
{
    public static IReadOnlyDictionary<Guid, PlayerGameStats> ComputeGameStats(Match match)
    {
        var stats = new Dictionary<Guid, PlayerGameStats>();

        PlayerGameStats Get(Guid playerId) =>
            stats.TryGetValue(playerId, out var existing) ? existing : PlayerGameStats.Empty(playerId);

        foreach (var play in match.EventLog)
        {
            if (play.OffensePlayerId is { } offenseId)
            {
                var current = Get(offenseId);
                var touchdown = play.IsScore ? 1 : 0;
                stats[offenseId] = play.IsPassPlay
                    ? current with
                    {
                        Receptions = current.Receptions + (play.IsTurnover ? 0 : 1),
                        ReceivingYards = current.ReceivingYards + play.YardsGained,
                        ReceivingTouchdowns = current.ReceivingTouchdowns + touchdown,
                    }
                    : current with
                    {
                        RushingAttempts = current.RushingAttempts + 1,
                        RushingYards = current.RushingYards + play.YardsGained,
                        RushingTouchdowns = current.RushingTouchdowns + touchdown,
                    };
            }

            if (play.DefensePlayerId is { } defenseId)
            {
                var current = Get(defenseId);
                stats[defenseId] = current with
                {
                    TacklesOrHits = current.TacklesOrHits + 1,
                    TurnoversForced = current.TurnoversForced + (play.IsTurnover ? 1 : 0),
                };
            }

            foreach (var injury in play.InjuryEvents)
            {
                if (play.DefensePlayerId is { } hitterId)
                {
                    var hitterStats = Get(hitterId);
                    stats[hitterId] = hitterStats with { InjuriesCaused = hitterStats.InjuriesCaused + 1 };
                }

                var targetStats = Get(injury.PlayerId);
                stats[injury.PlayerId] = targetStats with { TimesInjured = targetStats.TimesInjured + 1 };
            }
        }

        return stats;
    }

    public static IReadOnlyDictionary<Guid, Guid> MapPlayersToTeams(Team homeTeam, Team awayTeam)
    {
        var map = new Dictionary<Guid, Guid>();
        foreach (var playerId in homeTeam.Roster.DepthChart.Values.SelectMany(ids => ids)) map[playerId] = homeTeam.Id;
        foreach (var playerId in awayTeam.Roster.DepthChart.Values.SelectMany(ids => ids)) map[playerId] = awayTeam.Id;
        return map;
    }

    /// <summary>Folds one game's stats into a running season table, mutating and returning it.</summary>
    public static Dictionary<(Guid PlayerId, int Year, Guid TeamId), PlayerSeasonStats> ApplyGameToSeason(
        Dictionary<(Guid PlayerId, int Year, Guid TeamId), PlayerSeasonStats> seasonTotals,
        IReadOnlyDictionary<Guid, PlayerGameStats> gameStats,
        int year,
        IReadOnlyDictionary<Guid, Guid> playerTeamForThisGame)
    {
        foreach (var (playerId, gameLine) in gameStats)
        {
            if (!playerTeamForThisGame.TryGetValue(playerId, out var teamId))
            {
                continue;
            }

            var key = (playerId, year, teamId);
            seasonTotals[key] = seasonTotals.TryGetValue(key, out var existing)
                ? existing.AddGame(gameLine)
                : PlayerSeasonStats.StartingWith(playerId, year, teamId, gameLine);
        }

        return seasonTotals;
    }

    /// <summary>A player's full career, year by year, team by team, in chronological order.</summary>
    public static IReadOnlyList<PlayerSeasonStats> CareerHistory(
        IEnumerable<PlayerSeasonStats> allSeasonStats, Guid playerId) =>
        allSeasonStats
            .Where(s => s.PlayerId == playerId)
            .OrderBy(s => s.Year)
            .ToList();
}
