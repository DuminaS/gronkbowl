using GronkBowl.Domain;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class DraftProspectGeneratorTests
{
    [Fact]
    public void GenerateClass_ProducesTheRequestedCount_WithAttributesBelowCeiling()
    {
        var prospects = DraftProspectGenerator.GenerateClass(50, new Random(1));

        Assert.Equal(50, prospects.Count);
        foreach (var prospect in prospects)
        {
            var ceiling = RaceProfiles.All[prospect.Race].CeilingAttributes;
            Assert.True(prospect.Attributes.Speed <= ceiling.Speed);
            Assert.True(prospect.Attributes.Strength <= ceiling.Strength);
            Assert.True(prospect.Attributes.Agility <= ceiling.Agility);
            Assert.True(prospect.Attributes.Awareness <= ceiling.Awareness);
            Assert.True(prospect.Attributes.Durability <= ceiling.Durability);
        }
    }

    [Fact]
    public void GenerateClass_SameSeed_IsDeterministic()
    {
        var first = DraftProspectGenerator.GenerateClass(20, new Random(99));
        var second = DraftProspectGenerator.GenerateClass(20, new Random(99));

        for (var i = 0; i < first.Count; i++)
        {
            Assert.Equal(first[i].Race, second[i].Race);
            Assert.Equal(first[i].Attributes, second[i].Attributes);
        }
    }
}

public class RookieWageScaleTests
{
    [Fact]
    public void EarlierPicks_EarnMoreThanLaterPicks()
    {
        var firstOverall = RookieWageScale.ForPick(1);
        var lastOverall = RookieWageScale.ForPick(100);

        Assert.True(firstOverall.AnnualGold > lastOverall.AnnualGold);
    }

    [Fact]
    public void NeverGoesBelowTheMinimum()
    {
        var veryLatePick = RookieWageScale.ForPick(10_000);
        Assert.True(veryLatePick.AnnualGold > 0);
    }
}

public class DraftOrderCalculatorTests
{
    [Fact]
    public void WorstRecord_PicksFirst()
    {
        var goodTeam = Guid.NewGuid();
        var badTeam = Guid.NewGuid();
        var standings = new Dictionary<Guid, TeamRecord>
        {
            [goodTeam] = new(Wins: 10, Losses: 0, Ties: 0),
            [badTeam] = new(Wins: 0, Losses: 10, Ties: 0),
        };

        var order = DraftOrderCalculator.FromStandings(standings, new[] { goodTeam, badTeam });

        Assert.Equal(badTeam, order[0]);
        Assert.Equal(goodTeam, order[1]);
    }

    [Fact]
    public void TeamsWithNoGamesPlayed_PickAheadOfUndefeatedTeams()
    {
        var undefeated = Guid.NewGuid();
        var newTeam = Guid.NewGuid();
        var standings = new Dictionary<Guid, TeamRecord> { [undefeated] = new(5, 0, 0) };

        var order = DraftOrderCalculator.FromStandings(standings, new[] { undefeated, newTeam });

        Assert.Equal(newTeam, order[0]);
    }
}

public class DraftBoardTests
{
    [Fact]
    public void MakePick_OnlyAllowsTheTeamOnTheClock()
    {
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var prospects = DraftProspectGenerator.GenerateClass(4, new Random(1));
        var board = new DraftBoard(prospects, new[] { teamA, teamB }, rounds: 2);

        Assert.Equal(teamA, board.OnTheClock);
        Assert.Throws<InvalidOperationException>(() => board.MakePick(teamB, prospects[0].Id));
    }

    [Fact]
    public void MakePick_AdvancesTheClockAndCannotRedraftTheSameProspect()
    {
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var prospects = DraftProspectGenerator.GenerateClass(4, new Random(1));
        var board = new DraftBoard(prospects, new[] { teamA, teamB }, rounds: 2);

        board.MakePick(teamA, prospects[0].Id);
        Assert.Equal(teamB, board.OnTheClock);

        Assert.Throws<InvalidOperationException>(() => board.MakePick(teamB, prospects[0].Id));
    }

    [Fact]
    public void IsComplete_OnceEveryPickHasBeenMade()
    {
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var prospects = DraftProspectGenerator.GenerateClass(4, new Random(1));
        var board = new DraftBoard(prospects, new[] { teamA, teamB }, rounds: 2);

        board.MakePick(teamA, prospects[0].Id);
        board.MakePick(teamB, prospects[1].Id);
        board.MakePick(teamA, prospects[2].Id);
        Assert.False(board.IsComplete);
        board.MakePick(teamB, prospects[3].Id);

        Assert.True(board.IsComplete);
        Assert.Null(board.OnTheClock);
    }

    [Fact]
    public void MakePick_EarlierPicksGetRicherRookieContracts()
    {
        var teamA = Guid.NewGuid();
        var teamB = Guid.NewGuid();
        var prospects = DraftProspectGenerator.GenerateClass(4, new Random(1));
        var board = new DraftBoard(prospects, new[] { teamA, teamB }, rounds: 2);

        var (_, firstPickContract) = board.MakePick(teamA, prospects[0].Id);
        var (_, secondPickContract) = board.MakePick(teamB, prospects[1].Id);

        Assert.True(firstPickContract.AnnualGold > secondPickContract.AnnualGold);
    }
}
