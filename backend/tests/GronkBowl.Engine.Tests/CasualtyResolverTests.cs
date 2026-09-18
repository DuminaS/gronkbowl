using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class CasualtyResolverTests
{
    private static Player MakePlayer(Race race, int armorValue, int strength = 60, int durability = 60) =>
        new()
        {
            Name = race.ToString(),
            Race = race,
            Position = FootballPosition.LB,
            Attributes = new Attributes(Speed: 60, Strength: strength, Agility: 60, Awareness: 60, Durability: durability),
            ArmorValue = armorValue,
            DevelopmentPotential = DevelopmentPotential.SteadyEddie,
        };

    [Fact]
    public void ArmorValueOfTwo_AlwaysBreaks_RegardlessOfRoll()
    {
        var hitter = MakePlayer(Race.Thornhide, armorValue: 9, strength: 0);
        var target = MakePlayer(Race.Aelari, armorValue: 2, durability: 0);

        for (var seed = 0; seed < 50; seed++)
        {
            var result = CasualtyResolver.TryResolveHit(hitter, target, new Random(seed));
            Assert.NotNull(result);
        }
    }

    [Fact]
    public void ExtremeArmorValue_NeverBreaks()
    {
        var hitter = MakePlayer(Race.Thornhide, armorValue: 9, strength: 100);
        var target = MakePlayer(Race.Aelari, armorValue: 999);

        for (var seed = 0; seed < 50; seed++)
        {
            var result = CasualtyResolver.TryResolveHit(hitter, target, new Random(seed));
            Assert.Null(result);
        }
    }

    [Fact]
    public void SameSeed_ProducesTheSameResult()
    {
        var hitter = MakePlayer(Race.Thornhide, armorValue: 9);
        var target = MakePlayer(Race.Skitterkin, armorValue: 6);

        var first = CasualtyResolver.TryResolveHit(hitter, target, new Random(12345));
        var second = CasualtyResolver.TryResolveHit(hitter, target, new Random(12345));

        Assert.Equal(first, second);
    }

    [Fact]
    public void Unbreakable_NeverMakesTheOutcomeWorse_ThanAnEquivalentNonIronkinTarget()
    {
        var hitter = MakePlayer(Race.Aelari, armorValue: 9);

        for (var seed = 0; seed < 200; seed++)
        {
            var baseline = CasualtyResolver.TryResolveHit(hitter, MakePlayer(Race.Aelari, armorValue: 6), new Random(seed));
            var ironkinTarget = CasualtyResolver.TryResolveHit(hitter, MakePlayer(Race.Ironkin, armorValue: 6), new Random(seed));

            if (baseline is null || ironkinTarget is null)
            {
                continue;
            }

            Assert.True(ironkinTarget.SeverityRoll >= baseline.SeverityRoll);
        }
    }

    [Fact]
    public void HigherDurability_NeverBreaksArmorMoreOftenThanLowerDurability()
    {
        var hitter = MakePlayer(Race.Thornhide, armorValue: 9);

        var toughBreaks = 0;
        var fragileBreaks = 0;
        const int trials = 300;

        for (var seed = 0; seed < trials; seed++)
        {
            var tough = MakePlayer(Race.Ironkin, armorValue: 9, durability: 95);
            var fragile = MakePlayer(Race.Aelari, armorValue: 9, durability: 45);

            if (CasualtyResolver.TryResolveHit(hitter, tough, new Random(seed)) is not null) toughBreaks++;
            if (CasualtyResolver.TryResolveHit(hitter, fragile, new Random(seed)) is not null) fragileBreaks++;
        }

        Assert.True(toughBreaks < fragileBreaks, $"Expected higher Durability to break armor less often: tough broke {toughBreaks}/{trials}, fragile broke {fragileBreaks}/{trials}.");
    }
}
