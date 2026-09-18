using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Domain.Tests;

public class RaceProfileTests
{
    [Theory]
    [InlineData(Race.Ironkin)]
    [InlineData(Race.Aelari)]
    [InlineData(Race.Thornhide)]
    [InlineData(Race.Skitterkin)]
    public void EveryLaunchRace_HasACeilingProfile(Race race)
    {
        Assert.True(RaceProfiles.All.ContainsKey(race));
    }

    [Fact]
    public void Ironkin_IsTankierThanAelari()
    {
        var ironkin = RaceProfiles.All[Race.Ironkin].CeilingAttributes;
        var aelari = RaceProfiles.All[Race.Aelari].CeilingAttributes;

        Assert.True(ironkin.Durability > aelari.Durability);
        Assert.True(aelari.Speed > ironkin.Speed);
    }

    [Fact]
    public void NewPlayer_StartsHealthyAndAvailable()
    {
        var player = new Player
        {
            Name = "Test Player",
            Race = Race.Thornhide,
            Position = FootballPosition.LB,
            Attributes = RaceProfiles.All[Race.Thornhide].CeilingAttributes,
            ArmorValue = 9
        };

        Assert.Equal(InjuryStatus.Healthy, player.InjuryStatus);
        Assert.True(player.IsAvailableForLineup);
    }
}
