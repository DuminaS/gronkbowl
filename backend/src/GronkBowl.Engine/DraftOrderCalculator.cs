using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>Worst record picks first - ties competitive balance directly to on-field results.</summary>
public static class DraftOrderCalculator
{
    public static List<Guid> FromStandings(IReadOnlyDictionary<Guid, TeamRecord> standings, IReadOnlyList<Guid> allTeamIds) =>
        allTeamIds.OrderBy(id => WinPercentage(standings, id)).ToList();

    private static double WinPercentage(IReadOnlyDictionary<Guid, TeamRecord> standings, Guid teamId)
    {
        if (!standings.TryGetValue(teamId, out var record))
        {
            return 0;
        }

        var games = record.Wins + record.Losses + record.Ties;
        return games == 0 ? 0 : (record.Wins + record.Ties * 0.5) / games;
    }
}
