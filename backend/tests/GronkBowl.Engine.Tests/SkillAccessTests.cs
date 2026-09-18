using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class SkillAccessTests
{
    private static Player MakePlayer(FootballPosition position, Race race = Race.Ironkin) => new()
    {
        Name = "Test",
        Race = race,
        Position = position,
        Attributes = RaceProfiles.All[race].CeilingAttributes,
        ArmorValue = 9,
        DevelopmentPotential = DevelopmentPotential.SteadyEddie,
    };

    [Fact]
    public void EveryPosition_CanAlwaysTakeGeneralSkills()
    {
        foreach (FootballPosition position in Enum.GetValues<FootballPosition>())
        {
            var player = MakePlayer(position);
            var available = SkillAccess.AvailablePrimarySkills(player);

            Assert.Contains(Skill.SureHands, available); // General
        }
    }

    [Fact]
    public void QuarterbackPrimary_IncludesPassing_ButNotStrength()
    {
        var qb = MakePlayer(FootballPosition.QB);
        var primary = SkillAccess.AvailablePrimarySkills(qb);

        Assert.Contains(Skill.Accurate, primary); // Passing
        Assert.DoesNotContain(Skill.MightyBlow, primary); // Strength
    }

    [Fact]
    public void OffensiveLinePrimary_IncludesStrength_ButNotPassing()
    {
        var ol = MakePlayer(FootballPosition.OL);
        var primary = SkillAccess.AvailablePrimarySkills(ol);

        Assert.Contains(Skill.MightyBlow, primary); // Strength
        Assert.DoesNotContain(Skill.Accurate, primary); // Passing
    }

    [Fact]
    public void OnlySkitterkin_CanReachMutationSkills_AndOnlyAsASecondary()
    {
        var skitterkinWr = MakePlayer(FootballPosition.WR, Race.Skitterkin);
        var ironkinWr = MakePlayer(FootballPosition.WR, Race.Ironkin);

        Assert.Contains(Skill.ExtraArms, SkillAccess.AvailableSecondarySkills(skitterkinWr));
        Assert.DoesNotContain(Skill.ExtraArms, SkillAccess.AvailableSecondarySkills(ironkinWr));
        Assert.DoesNotContain(Skill.ExtraArms, SkillAccess.AvailablePrimarySkills(skitterkinWr));
    }

    [Fact]
    public void AvailableSkills_ExcludesSkillsThePlayerAlreadyHas()
    {
        var player = MakePlayer(FootballPosition.WR);
        player.Skills.Add(Skill.SureHands);

        Assert.DoesNotContain(Skill.SureHands, SkillAccess.AvailablePrimarySkills(player));
    }
}
