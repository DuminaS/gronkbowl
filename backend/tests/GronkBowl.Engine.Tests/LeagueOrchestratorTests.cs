using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class LeagueOrchestratorTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players) BuildMatchup()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Thornhide, players);
        return (home, away, players);
    }

    private static Season BuildOneWeekSeason(Team home, Team away, Guid? seasonId = null) => new()
    {
        Id = seasonId ?? Guid.NewGuid(),
        Year = 2026,
        RulesVersion = "v1",
        Schedule = { new ScheduledGame(Week: 1, home.Id, away.Id) },
    };

    [Fact]
    public void ResolveWeek_OnlyResolvesGamesScheduledForThatWeek()
    {
        var (home, away, players) = BuildMatchup();
        var season = BuildOneWeekSeason(home, away);
        season.Schedule.Add(new ScheduledGame(Week: 2, home.Id, away.Id));

        var teams = new Dictionary<Guid, Team> { [home.Id] = home, [away.Id] = away };
        var callSheets = new Dictionary<Guid, CallSheet>
        {
            [home.Id] = TestTeamFactory.BuildRealisticCallSheet(home).Also(cs => cs.Submitted = true),
            [away.Id] = TestTeamFactory.BuildRealisticCallSheet(away).Also(cs => cs.Submitted = true),
        };

        var results = LeagueOrchestrator.ResolveWeek(season, week: 1, teams, callSheets, players);

        Assert.Single(results);
        Assert.Single(season.CompletedMatches);
        Assert.Equal(1, season.CompletedMatches[0].Week);
    }

    [Fact]
    public void ResolveWeek_UpdatesStandings_ForTheWinnerAndLoser()
    {
        var (home, away, players) = BuildMatchup();
        var season = BuildOneWeekSeason(home, away);
        var teams = new Dictionary<Guid, Team> { [home.Id] = home, [away.Id] = away };
        var callSheets = new Dictionary<Guid, CallSheet>
        {
            [home.Id] = TestTeamFactory.BuildRealisticCallSheet(home).Also(cs => cs.Submitted = true),
            [away.Id] = TestTeamFactory.BuildRealisticCallSheet(away).Also(cs => cs.Submitted = true),
        };

        var results = LeagueOrchestrator.ResolveWeek(season, week: 1, teams, callSheets, players);
        var match = results.Single();

        var homeRecord = season.Standings[home.Id];
        var awayRecord = season.Standings[away.Id];

        if (match.HomeScore > match.AwayScore)
        {
            Assert.Equal(1, homeRecord.Wins);
            Assert.Equal(1, awayRecord.Losses);
        }
        else if (match.AwayScore > match.HomeScore)
        {
            Assert.Equal(1, awayRecord.Wins);
            Assert.Equal(1, homeRecord.Losses);
        }
        else
        {
            Assert.Equal(1, homeRecord.Ties);
            Assert.Equal(1, awayRecord.Ties);
        }
    }

    [Fact]
    public void ResolveWeek_MissingSubmission_FallsBackToARandomCallSheet_AndStillResolves()
    {
        var (home, away, players) = BuildMatchup();
        var season = BuildOneWeekSeason(home, away);
        var teams = new Dictionary<Guid, Team> { [home.Id] = home, [away.Id] = away };

        // Away never submits anything.
        var callSheets = new Dictionary<Guid, CallSheet>
        {
            [home.Id] = TestTeamFactory.BuildRealisticCallSheet(home).Also(cs => cs.Submitted = true),
        };

        var results = LeagueOrchestrator.ResolveWeek(season, week: 1, teams, callSheets, players);

        Assert.Single(results);
        Assert.True(results[0].IsResolved);
    }

    [Fact]
    public void ResolveWeek_SameSeasonAndWeek_ProducesTheSameResultEveryTime()
    {
        var sharedSeasonId = new Guid("11111111-1111-1111-1111-111111111111");
        var (home, away, players) = BuildMatchup();
        var teams = new Dictionary<Guid, Team> { [home.Id] = home, [away.Id] = away };
        var sheets = new Dictionary<Guid, CallSheet>
        {
            [home.Id] = TestTeamFactory.BuildRealisticCallSheet(home).Also(cs => cs.Submitted = true),
            [away.Id] = TestTeamFactory.BuildRealisticCallSheet(away).Also(cs => cs.Submitted = true),
        };

        var season1 = BuildOneWeekSeason(home, away, sharedSeasonId);
        var result1 = LeagueOrchestrator.ResolveWeek(season1, week: 1, teams, sheets, players).Single();

        // Same team/player objects, same ids and call sheets - just reset the mutable injury
        // state a real game would have changed, so the second run starts from the identical
        // pristine roster instead of carrying over the first run's casualties.
        foreach (var player in players.Values)
        {
            player.InjuryStatus = InjuryStatus.Healthy;
            player.InjuryWeeksRemaining = 0;
            player.PermanentAttributePenalty = 0;
        }

        var season2 = BuildOneWeekSeason(home, away, sharedSeasonId);
        var result2 = LeagueOrchestrator.ResolveWeek(season2, week: 1, teams, sheets, players).Single();

        Assert.Equal(result1.HomeScore, result2.HomeScore);
        Assert.Equal(result1.AwayScore, result2.AwayScore);
        Assert.Equal(result1.EventLog.Count, result2.EventLog.Count);
    }
}

internal static class TestExtensions
{
    public static T Also<T>(this T value, Action<T> configure)
    {
        configure(value);
        return value;
    }
}
