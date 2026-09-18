using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class PlayResolverTests
{
    [Fact]
    public void SameSeed_ProducesIdenticalOutcome()
    {
        var players = new Dictionary<Guid, Player>();
        var offense = TestTeamFactory.BuildTeam("Offense", Race.Aelari, players);
        var defense = TestTeamFactory.BuildTeam("Defense", Race.Thornhide, players);

        var offensePlay = DefaultPlaybook.Offense.First(p => p.Name == "Inside Dive");
        var defensePlay = DefaultPlaybook.Defense.First(p => p.Name == "Base Front");

        var first = PlayResolver.Resolve(offensePlay, offense, defensePlay, defense, players, new Random(7));
        var second = PlayResolver.Resolve(offensePlay, offense, defensePlay, defense, players, new Random(7));

        Assert.Equal(first.Yards, second.Yards);
        Assert.Equal(first.IsTurnover, second.IsTurnover);
    }

    [Fact]
    public void RunPlay_YardageStaysWithinDesignedBounds()
    {
        var players = new Dictionary<Guid, Player>();
        var offense = TestTeamFactory.BuildTeam("Offense", Race.Thornhide, players);
        var defense = TestTeamFactory.BuildTeam("Defense", Race.Ironkin, players);

        var offensePlay = DefaultPlaybook.Offense.First(p => p.Name == "Inside Dive");
        var defensePlay = DefaultPlaybook.Defense.First(p => p.Name == "Base Front");

        for (var seed = 0; seed < 100; seed++)
        {
            var outcome = PlayResolver.Resolve(offensePlay, offense, defensePlay, defense, players, new Random(seed));
            Assert.InRange(outcome.Yards, -5, 25);
        }
    }

    [Fact]
    public void PassPlay_WithNoInstalledReceiverAvailable_FallsBackToIncompleteRatherThanCrashing()
    {
        var players = new Dictionary<Guid, Player>();
        var offense = TestTeamFactory.BuildTeam("Offense", Race.Aelari, players);
        var defense = TestTeamFactory.BuildTeam("Defense", Race.Skitterkin, players);

        // Injure every WR so the Slant-Flat's primary receiver has nobody to hand the assignment to.
        foreach (var wr in players.Values.Where(p => p.Position == FootballPosition.WR && p.Name.StartsWith("Offense")))
        {
            wr.InjuryStatus = InjuryStatus.SeasonEnding;
        }

        var offensePlay = DefaultPlaybook.Offense.First(p => p.Name == "Slant-Flat");
        var defensePlay = DefaultPlaybook.Defense.First(p => p.Name == "Cover 1");

        var outcome = PlayResolver.Resolve(offensePlay, offense, defensePlay, defense, players, new Random(1));

        Assert.Equal(0, outcome.Yards);
        Assert.False(outcome.IsTurnover);
    }
}
