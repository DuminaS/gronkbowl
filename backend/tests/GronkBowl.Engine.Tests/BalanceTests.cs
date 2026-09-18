using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

/// <summary>
/// Guards against the exact regression this project already shipped once: a race matchup so
/// lopsided one side never wins. Not a precision balance suite - just a tripwire that fails
/// loudly if a future engine change makes one race's identity collapse into "the team that
/// loses," the way the line-battle-dominates-everything bug did before these fixes.
/// </summary>
public class BalanceTests
{
    [Fact]
    public void IronkinVsAelari_NeitherRaceDominatesTheMatchup()
    {
        var ironkinWins = 0;
        var aelariWins = 0;
        const int trials = 40;

        for (var seed = 0; seed < trials; seed++)
        {
            var players = new Dictionary<Guid, Player>();
            var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
            var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);
            var homeSheet = TestTeamFactory.BuildRealisticCallSheet(home);
            var awaySheet = TestTeamFactory.BuildRealisticCallSheet(away);

            var match = MatchEngine.ResolveGame(home, away, homeSheet, awaySheet, players, week: 1, seed: seed);

            if (match.HomeScore > match.AwayScore) ironkinWins++;
            else if (match.AwayScore > match.HomeScore) aelariWins++;
        }

        // Aelari (finesse/speed) should be able to win a meaningful share of these against
        // Ironkin (power) even though the matchups favor different archetypes - a 0-for-40
        // outcome (which is what this test would have caught before the balance pass) means a
        // race's whole identity has been mathematically nullified, not just disadvantaged.
        Assert.True(aelariWins >= trials / 10, $"Aelari won only {aelariWins}/{trials} - a finesse race should never be this close to unable to win.");
        Assert.True(ironkinWins >= trials / 10, $"Ironkin won only {ironkinWins}/{trials} - a power race should never be this close to unable to win.");
    }
}
