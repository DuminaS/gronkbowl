using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class MatchSummaryBuilderTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players, Match Match) PlayOneGame(long seed)
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home Squad", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away Squad", Race.Aelari, players);

        var homeSheet = TestTeamFactory.BuildRealisticCallSheet(home);
        var awaySheet = TestTeamFactory.BuildRealisticCallSheet(away);

        var match = MatchEngine.ResolveGame(home, away, homeSheet, awaySheet, players, week: 1, seed: seed);
        return (home, away, players, match);
    }

    [Fact]
    public void BuildGameSummary_LeadsWithTheFinalScoreAndBothTeamNames()
    {
        var (home, away, players, match) = PlayOneGame(seed: 3);

        var summary = MatchSummaryBuilder.BuildGameSummary(match, home, away, players);

        Assert.StartsWith("Final:", summary);
        Assert.Contains(home.Name, summary);
        Assert.Contains(away.Name, summary);
    }

    [Fact]
    public void BuildGameSummary_OmitsRoutinePlaysWithNoScoreTurnoverOrInjury()
    {
        var (home, away, players, match) = PlayOneGame(seed: 3);
        var summary = MatchSummaryBuilder.BuildGameSummary(match, home, away, players);

        var routinePlayCount = match.EventLog.Count(p => !p.IsScore && !p.IsTurnover && p.InjuryEvents.Count == 0);
        var summaryLineCount = summary.Split(Environment.NewLine).Length;

        Assert.True(summaryLineCount < match.EventLog.Count || routinePlayCount == 0);
    }

    [Fact]
    public void BuildBoxScore_NamesOnlyPlayersWhoActuallyRecordedAStat()
    {
        var (home, away, players, match) = PlayOneGame(seed: 3);
        var gameStats = StatsAggregator.ComputeGameStats(match);

        var boxScore = MatchSummaryBuilder.BuildBoxScore(gameStats, players);

        Assert.StartsWith("Box Score:", boxScore);
        foreach (var player in players.Values.Where(p => !gameStats.ContainsKey(p.Id)))
        {
            Assert.DoesNotContain(player.Name, boxScore);
        }
    }
}
