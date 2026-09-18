using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// The Draft and Free Agency are off-season events, not something a coach does mid-year - so
/// "is the regular season done" needs to be a real, checkable fact, not just implied by the UI.
/// Season.CompletedMatches is EF-ignored (Matches lives in its own table, see
/// GronkBowlDbContext.ConfigureSeason), so callers must pass the real count from a query against
/// that table rather than trusting Season.CompletedMatches.Count, which is empty on anything
/// loaded fresh from the database.
/// </summary>
public static class SeasonPhaseCalculator
{
    public static bool IsRegularSeasonComplete(Season season, int completedMatchCount) =>
        season.Schedule.Count > 0 && completedMatchCount >= season.Schedule.Count;

    /// <summary>The lowest scheduled week that doesn't have every one of its games completed
    /// yet, or null once the whole schedule is done.</summary>
    public static int? NextUnplayedWeek(Season season, IReadOnlyCollection<(int Week, Guid HomeTeamId, Guid AwayTeamId)> completedGames)
    {
        var played = completedGames.ToHashSet();
        return season.Schedule
            .Where(g => !played.Contains((g.Week, g.HomeTeamId, g.AwayTeamId)))
            .Select(g => g.Week)
            .OrderBy(w => w)
            .Cast<int?>()
            .FirstOrDefault();
    }
}
