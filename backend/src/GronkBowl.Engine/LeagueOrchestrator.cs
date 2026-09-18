using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// Runs one week of a Season: for every scheduled game, uses each team's submitted Call Sheet
/// or generates the missed-deadline fallback, resolves the game through the deterministic
/// Engine, records the result, and updates standings. The seed for every game (and every
/// fallback call sheet) is derived from the season, week, and team ids - never supplied by a
/// caller - so a week's outcome is always reproducible from the Season and roster state alone.
/// </summary>
public static class LeagueOrchestrator
{
    public static IReadOnlyList<Match> ResolveWeek(
        Season season,
        int week,
        IReadOnlyDictionary<Guid, Team> teamsById,
        IReadOnlyDictionary<Guid, CallSheet> submittedCallSheets,
        IReadOnlyDictionary<Guid, Player> players)
    {
        var results = new List<Match>();

        foreach (var game in season.Schedule.Where(g => g.Week == week))
        {
            var homeTeam = teamsById[game.HomeTeamId];
            var awayTeam = teamsById[game.AwayTeamId];

            var homeSheet = ResolveCallSheet(season, week, homeTeam, submittedCallSheets);
            var awaySheet = ResolveCallSheet(season, week, awayTeam, submittedCallSheets);

            var seed = DeriveSeed(season, week, homeTeam.Id, awayTeam.Id, salt: "game");
            var match = MatchEngine.ResolveGame(homeTeam, awayTeam, homeSheet, awaySheet, players, week, seed);
            match.SeasonId = season.Id;

            season.CompletedMatches.Add(match);
            UpdateStandings(season, match);
            results.Add(match);
        }

        return results;
    }

    private static CallSheet ResolveCallSheet(
        Season season, int week, Team team, IReadOnlyDictionary<Guid, CallSheet> submitted)
    {
        if (submitted.TryGetValue(team.Id, out var sheet) && sheet.Submitted)
        {
            return sheet;
        }

        var fallbackSeed = DeriveSeed(season, week, team.Id, team.Id, salt: "fallback");
        return RandomCallSheetGenerator.Generate(team, week, new Random(unchecked((int)fallbackSeed)));
    }

    private static void UpdateStandings(Season season, Match match)
    {
        var home = GetRecord(season, match.HomeTeamId);
        var away = GetRecord(season, match.AwayTeamId);

        if (match.HomeScore > match.AwayScore)
        {
            season.Standings[match.HomeTeamId] = home with { Wins = home.Wins + 1 };
            season.Standings[match.AwayTeamId] = away with { Losses = away.Losses + 1 };
        }
        else if (match.AwayScore > match.HomeScore)
        {
            season.Standings[match.AwayTeamId] = away with { Wins = away.Wins + 1 };
            season.Standings[match.HomeTeamId] = home with { Losses = home.Losses + 1 };
        }
        else
        {
            season.Standings[match.HomeTeamId] = home with { Ties = home.Ties + 1 };
            season.Standings[match.AwayTeamId] = away with { Ties = away.Ties + 1 };
        }
    }

    private static TeamRecord GetRecord(Season season, Guid teamId) =>
        season.Standings.TryGetValue(teamId, out var record) ? record : new TeamRecord(0, 0, 0);

    private static long DeriveSeed(Season season, int week, Guid teamA, Guid teamB, string salt) =>
        HashCode.Combine(season.Id, week, teamA, teamB, salt);
}
