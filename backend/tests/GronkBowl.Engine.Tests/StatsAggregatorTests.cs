using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class StatsAggregatorTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players, Match Match) PlayOneGame(long seed)
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);

        var homeSheet = TestTeamFactory.BuildRealisticCallSheet(home);
        var awaySheet = TestTeamFactory.BuildRealisticCallSheet(away);

        var match = MatchEngine.ResolveGame(home, away, homeSheet, awaySheet, players, week: 1, seed: seed);
        return (home, away, players, match);
    }

    [Fact]
    public void ComputeGameStats_CreditsYardsToTheFeaturedPlayer_NotTheWholeTeam()
    {
        var (_, _, players, match) = PlayOneGame(seed: 5);

        var gameStats = StatsAggregator.ComputeGameStats(match);

        // The aggregator credits yardage to rushing/receiving on every play, turnover or not,
        // matching real box scores (a fumble still counts the yards gained up to that point).
        var totalOffensiveYardsInLog = match.EventLog.Sum(p => p.YardsGained);
        var totalCreditedYards = gameStats.Values.Sum(s => s.RushingYards + s.ReceivingYards);

        Assert.Equal(totalOffensiveYardsInLog, totalCreditedYards);
    }

    [Fact]
    public void ApplyGameToSeason_AccumulatesAcrossMultipleGames_ForTheSameTeam()
    {
        var (home, away, players, matchWeek1) = PlayOneGame(seed: 11);
        var gameStatsWeek1 = StatsAggregator.ComputeGameStats(matchWeek1);
        var teamMap = StatsAggregator.MapPlayersToTeams(home, away);

        var seasonTotals = new Dictionary<(Guid, int, Guid), PlayerSeasonStats>();
        seasonTotals = StatsAggregator.ApplyGameToSeason(seasonTotals, gameStatsWeek1, year: 2026, teamMap);

        var matchWeek2 = MatchEngine.ResolveGame(
            home, away,
            TestTeamFactory.BuildRealisticCallSheet(home),
            TestTeamFactory.BuildRealisticCallSheet(away),
            players, week: 2, seed: 12);
        var gameStatsWeek2 = StatsAggregator.ComputeGameStats(matchWeek2);
        seasonTotals = StatsAggregator.ApplyGameToSeason(seasonTotals, gameStatsWeek2, year: 2026, teamMap);

        var anyPlayerWithTwoGames = seasonTotals.Values.FirstOrDefault(s => s.GamesPlayed == 2);
        Assert.NotNull(anyPlayerWithTwoGames);
    }

    [Fact]
    public void CareerHistory_ShowsEachTeamAPlayerWasOn_InYearOrder()
    {
        var playerId = Guid.NewGuid();
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var stats = PlayerGameStats.Empty(playerId) with { RushingYards = 50 };

        var allSeasons = new List<PlayerSeasonStats>
        {
            PlayerSeasonStats.StartingWith(playerId, 2024, teamA, stats),
            PlayerSeasonStats.StartingWith(playerId, 2025, teamA, stats),
            PlayerSeasonStats.StartingWith(playerId, 2026, teamB, stats),
        };

        var career = StatsAggregator.CareerHistory(allSeasons, playerId);

        Assert.Equal(new[] { 2024, 2025, 2026 }, career.Select(s => s.Year));
        Assert.Equal(new[] { teamA, teamA, teamB }, career.Select(s => s.TeamId));
    }
}
