using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class LevelUpResolverTests
{
    private static Player MakePlayer(DevelopmentPotential potential = DevelopmentPotential.SteadyEddie, FootballPosition position = FootballPosition.WR, Race race = Race.Ironkin) => new()
    {
        Name = "Test",
        Race = race,
        Position = position,
        Attributes = RaceProfiles.All[race].CeilingAttributes,
        ArmorValue = 9,
        DevelopmentPotential = potential,
    };

    [Fact]
    public void Roll_OutcomeAlwaysMatchesTheDesignBriefsSumTable()
    {
        for (var seed = 0; seed < 500; seed++)
        {
            var roll = LevelUpResolver.Roll(new Random(seed));
            Assert.Equal(roll.Die1 + roll.Die2, roll.Sum);

            var expected = roll.Sum switch
            {
                10 => LevelUpOutcomeKind.SpeedOrArmorBoost,
                11 => LevelUpOutcomeKind.AgilityBoost,
                12 => LevelUpOutcomeKind.StrengthBoost,
                _ => LevelUpOutcomeKind.NewSkill,
            };

            Assert.Equal(expected, roll.BaseOutcome);
        }
    }

    [Fact]
    public void Roll_IsDoubles_OnlyWhenBothDiceMatch()
    {
        var doublesSeen = false;
        var nonDoublesSeen = false;

        for (var seed = 0; seed < 100 && !(doublesSeen && nonDoublesSeen); seed++)
        {
            var roll = LevelUpResolver.Roll(new Random(seed));
            Assert.Equal(roll.Die1 == roll.Die2, roll.IsDoubles);
            if (roll.IsDoubles) doublesSeen = true; else nonDoublesSeen = true;
        }

        Assert.True(doublesSeen);
        Assert.True(nonDoublesSeen);
    }

    [Fact]
    public void ApplyStatBoost_Strength_IncrementsAttributeAndLevel()
    {
        var player = MakePlayer();
        var before = player.Attributes.Strength;

        LevelUpResolver.ApplyStatBoost(player, BoostableStat.Strength);

        Assert.Equal(before + 1, player.Attributes.Strength);
        Assert.Equal(2, player.Level);
    }

    [Fact]
    public void ApplyStatBoost_ArmorValue_IncrementsArmorNotAnAttribute()
    {
        var player = MakePlayer();
        var attributesBefore = player.Attributes;
        var armorBefore = player.ArmorValue;

        LevelUpResolver.ApplyStatBoost(player, BoostableStat.ArmorValue);

        Assert.Equal(armorBefore + 1, player.ArmorValue);
        Assert.Equal(attributesBefore, player.Attributes);
    }

    [Fact]
    public void ApplySkill_AddsSkillAndIncrementsLevel()
    {
        var player = MakePlayer();
        LevelUpResolver.ApplySkill(player, Skill.SureHands);

        Assert.Contains(Skill.SureHands, player.Skills);
        Assert.Equal(2, player.Level);
    }

    [Fact]
    public void Reroll_OnlyWorksForAHallOfFamer_AndOnlyOncePerCareer()
    {
        var star = MakePlayer(DevelopmentPotential.FirstBallotHallOfFamer);
        var average = MakePlayer(DevelopmentPotential.SteadyEddie);

        Assert.Null(LevelUpResolver.Reroll(average, new Random(1)));

        Assert.NotNull(LevelUpResolver.Reroll(star, new Random(1)));
        Assert.True(star.UsedCareerReroll);
        Assert.Null(LevelUpResolver.Reroll(star, new Random(1)));
    }
}
