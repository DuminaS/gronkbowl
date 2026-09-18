using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class MatchEngineTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players) BuildMatchup()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);
        return (home, away, players);
    }

    private static CallSheet BuildFullCallSheet(Team team) => TestTeamFactory.BuildRealisticCallSheet(team);

    [Fact]
    public void ResolveGame_TerminatesAndProducesANonEmptyEventLog()
    {
        var (home, away, players) = BuildMatchup();
        var homeSheet = BuildFullCallSheet(home);
        var awaySheet = BuildFullCallSheet(away);

        var match = MatchEngine.ResolveGame(home, away, homeSheet, awaySheet, players, week: 1, seed: 42);

        Assert.True(match.IsResolved);
        Assert.NotEmpty(match.EventLog);
        Assert.True(match.HomeScore >= 0);
        Assert.True(match.AwayScore >= 0);
        Assert.True(match.HomeScore % 7 == 0);
        Assert.True(match.AwayScore % 7 == 0);
    }

    [Fact]
    public void ResolveGame_SameSeed_ProducesTheIdenticalFinalScore()
    {
        var (home1, away1, players1) = BuildMatchup();
        var match1 = MatchEngine.ResolveGame(home1, away1, BuildFullCallSheet(home1), BuildFullCallSheet(away1), players1, week: 1, seed: 999);

        var (home2, away2, players2) = BuildMatchup();
        var match2 = MatchEngine.ResolveGame(home2, away2, BuildFullCallSheet(home2), BuildFullCallSheet(away2), players2, week: 1, seed: 999);

        Assert.Equal(match1.HomeScore, match2.HomeScore);
        Assert.Equal(match1.AwayScore, match2.AwayScore);
        Assert.Equal(match1.EventLog.Count, match2.EventLog.Count);
    }

    [Fact]
    public void ResolveGame_DifferentSeeds_CanProduceDifferentOutcomes()
    {
        var results = Enumerable.Range(0, 10).Select(seed =>
        {
            var (home, away, players) = BuildMatchup();
            var match = MatchEngine.ResolveGame(home, away, BuildFullCallSheet(home), BuildFullCallSheet(away), players, week: 1, seed: seed);
            return (match.HomeScore, match.AwayScore);
        }).ToList();

        Assert.True(results.Distinct().Count() > 1);
    }

    [Fact]
    public void ResolveGame_CanProduceInjuriesOverTheCourseOfAGame()
    {
        var anyInjuryAcrossManySeeds = Enumerable.Range(0, 25).Any(seed =>
        {
            var (home, away, players) = BuildMatchup();
            var match = MatchEngine.ResolveGame(home, away, BuildFullCallSheet(home), BuildFullCallSheet(away), players, week: 1, seed: seed);
            return match.EventLog.Any(play => play.InjuryEvents.Count > 0);
        });

        Assert.True(anyInjuryAcrossManySeeds);
    }
}
