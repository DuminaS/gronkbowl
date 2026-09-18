using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// Turns a Match's EventLog into text a coach can actually read: a final score, then only the
/// plays that matter (scores, turnovers, injuries) rather than a dump of every snap.
/// </summary>
public static class MatchSummaryBuilder
{
    public static string BuildGameSummary(
        Match match, Team homeTeam, Team awayTeam, IReadOnlyDictionary<Guid, Player> players)
    {
        var lines = new List<string> { $"Final: {homeTeam.Name} {match.HomeScore} - {awayTeam.Name} {match.AwayScore}", string.Empty };

        foreach (var play in match.EventLog)
        {
            var line = DescribeNotablePlay(play, players);
            if (line is not null)
            {
                lines.Add(line);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildBoxScore(IReadOnlyDictionary<Guid, PlayerGameStats> gameStats, IReadOnlyDictionary<Guid, Player> players)
    {
        var lines = new List<string> { "Box Score:" };

        var byImpact = gameStats.Values.OrderByDescending(s => s.RushingYards + s.ReceivingYards + s.TacklesOrHits * 5);
        foreach (var stats in byImpact)
        {
            if (!players.TryGetValue(stats.PlayerId, out var player))
            {
                continue;
            }

            var parts = new List<string>();
            if (stats.RushingAttempts > 0) parts.Add($"{stats.RushingAttempts} car, {stats.RushingYards} yds, {stats.RushingTouchdowns} TD rushing");
            if (stats.Receptions > 0) parts.Add($"{stats.Receptions} rec, {stats.ReceivingYards} yds, {stats.ReceivingTouchdowns} TD receiving");
            if (stats.TacklesOrHits > 0) parts.Add($"{stats.TacklesOrHits} tackles/hits");
            if (stats.TurnoversForced > 0) parts.Add($"{stats.TurnoversForced} turnover(s) forced");
            if (stats.InjuriesCaused > 0) parts.Add($"{stats.InjuriesCaused} injury/injuries caused");
            if (stats.TimesInjured > 0) parts.Add($"injured x{stats.TimesInjured}");

            if (parts.Count > 0)
            {
                lines.Add($"{player.Name} ({player.Race}, {player.Position}): {string.Join("; ", parts)}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string? DescribeNotablePlay(PlayResult play, IReadOnlyDictionary<Guid, Player> players)
    {
        var featuredName = NameOf(play.OffensePlayerId, players);

        if (play.IsScore)
        {
            return $"Play {play.PlayIndex}: TOUCHDOWN - {featuredName} for {play.YardsGained} yards.";
        }

        if (play.IsTurnover)
        {
            var kind = play.IsPassPlay ? "intercepted" : "fumbles";
            return $"Play {play.PlayIndex}: TURNOVER - {featuredName} {kind}.";
        }

        if (play.InjuryEvents.Count > 0)
        {
            var details = play.InjuryEvents.Select(e => $"{NameOf(e.PlayerId, players)} ({e.Result.Status})");
            return $"Play {play.PlayIndex}: INJURY - {string.Join(", ", details)}.";
        }

        return null;
    }

    private static string NameOf(Guid? playerId, IReadOnlyDictionary<Guid, Player> players) =>
        playerId is { } id && players.TryGetValue(id, out var player) ? player.Name : "Unknown";
}
