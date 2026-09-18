using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class TradeProcessorTests
{
    [Fact]
    public void TryExecute_SwapsBothPlayersIntoTheOtherTeamsDepthChart()
    {
        var players = new Dictionary<Guid, Player>();
        var teamA = TestTeamFactory.BuildTeam("A", Race.Ironkin, players);
        var teamB = TestTeamFactory.BuildTeam("B", Race.Aelari, players);
        teamA.CapSpace = 1000;
        teamB.CapSpace = 1000;

        var playerA = players.Values.First(p => p.Position == FootballPosition.RB && p.Name.StartsWith("A"));
        var playerB = players.Values.First(p => p.Position == FootballPosition.RB && p.Name.StartsWith("B"));
        playerA.Contract = new Contract(100, 3, 100, 0);
        playerB.Contract = new Contract(120, 3, 120, 0);

        var succeeded = TradeProcessor.TryExecute(teamA, playerA.Id, teamB, playerB.Id, players);

        Assert.True(succeeded);
        Assert.Contains(playerB.Id, teamA.Roster.DepthChart[FootballPosition.RB]);
        Assert.DoesNotContain(playerA.Id, teamA.Roster.DepthChart[FootballPosition.RB]);
        Assert.Contains(playerA.Id, teamB.Roster.DepthChart[FootballPosition.RB]);
        Assert.DoesNotContain(playerB.Id, teamB.Roster.DepthChart[FootballPosition.RB]);
    }

    [Fact]
    public void TryExecute_UpdatesCapSpaceOnBothSides()
    {
        var players = new Dictionary<Guid, Player>();
        var teamA = TestTeamFactory.BuildTeam("A", Race.Ironkin, players);
        var teamB = TestTeamFactory.BuildTeam("B", Race.Aelari, players);
        teamA.CapSpace = 500;
        teamB.CapSpace = 500;

        var playerA = players.Values.First(p => p.Position == FootballPosition.WR && p.Name.StartsWith("A"));
        var playerB = players.Values.First(p => p.Position == FootballPosition.WR && p.Name.StartsWith("B"));
        playerA.Contract = new Contract(100, 3, 100, 0);
        playerB.Contract = new Contract(150, 3, 150, 0);

        TradeProcessor.TryExecute(teamA, playerA.Id, teamB, playerB.Id, players);

        // Team A shed a $100 contract and took on a $150 one: net -50.
        Assert.Equal(450, teamA.CapSpace);
        // Team B shed $150, took on $100: net +50.
        Assert.Equal(550, teamB.CapSpace);
    }

    [Fact]
    public void TryExecute_RefusesTheTrade_IfEitherSideWouldGoOverCap()
    {
        var players = new Dictionary<Guid, Player>();
        var teamA = TestTeamFactory.BuildTeam("A", Race.Ironkin, players);
        var teamB = TestTeamFactory.BuildTeam("B", Race.Aelari, players);
        teamA.CapSpace = 10; // can't absorb a big contract
        teamB.CapSpace = 500;

        var playerA = players.Values.First(p => p.Position == FootballPosition.TE && p.Name.StartsWith("A"));
        var playerB = players.Values.First(p => p.Position == FootballPosition.TE && p.Name.StartsWith("B"));
        playerA.Contract = new Contract(5, 1, 5, 0);
        playerB.Contract = new Contract(300, 3, 300, 0);

        var succeeded = TradeProcessor.TryExecute(teamA, playerA.Id, teamB, playerB.Id, players);

        Assert.False(succeeded);
        Assert.Equal(10, teamA.CapSpace);
        Assert.Equal(500, teamB.CapSpace);
        Assert.Contains(playerA.Id, teamA.Roster.DepthChart[FootballPosition.TE]);
    }
}
