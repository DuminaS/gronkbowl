using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class PlayerProgressionTests
{
    private static Player MakePlayer(DevelopmentPotential potential, FootballPosition position = FootballPosition.WR, Race race = Race.Ironkin) => new()
    {
        Name = "Test",
        Race = race,
        Position = position,
        Attributes = RaceProfiles.All[race].CeilingAttributes,
        ArmorValue = 9,
        DevelopmentPotential = potential,
    };

    [Theory]
    [InlineData(2, 6)]
    [InlineData(3, 16)]
    [InlineData(4, 31)]
    [InlineData(5, 51)]
    [InlineData(6, 76)]
    [InlineData(7, 121)]
    public void SppNeededForLevel_MatchesTheDesignBrief_ForASteadyEddie(int level, int expectedSpp)
    {
        var player = MakePlayer(DevelopmentPotential.SteadyEddie);
        Assert.Equal(expectedSpp, PlayerProgression.SppNeededForLevel(level, player));
    }

    [Fact]
    public void FasterDeveloper_NeedsLessSppThanASteadyEddie_ForTheSameLevel()
    {
        var star = MakePlayer(DevelopmentPotential.FirstBallotHallOfFamer);
        var average = MakePlayer(DevelopmentPotential.SteadyEddie);

        Assert.True(PlayerProgression.SppNeededForLevel(4, star) < PlayerProgression.SppNeededForLevel(4, average));
    }

    [Fact]
    public void Bust_NeedsMoreSppThanASteadyEddie_ForTheSameLevel()
    {
        var bust = MakePlayer(DevelopmentPotential.CampFodder);
        var average = MakePlayer(DevelopmentPotential.SteadyEddie);

        Assert.True(PlayerProgression.SppNeededForLevel(2, bust) > PlayerProgression.SppNeededForLevel(2, average));
    }

    [Fact]
    public void CampFodder_CannotLevelUpPastItsCeiling_EvenWithUnlimitedSpp()
    {
        var bust = MakePlayer(DevelopmentPotential.CampFodder);
        bust.Level = 2;
        bust.SkillPoints = 100_000;

        Assert.False(PlayerProgression.IsEligibleToLevelUp(bust));
    }

    [Fact]
    public void SteadyEddie_IsEligible_OnceItCrossesItsThreshold()
    {
        var player = MakePlayer(DevelopmentPotential.SteadyEddie);
        player.SkillPoints = 5;
        Assert.False(PlayerProgression.IsEligibleToLevelUp(player));

        player.SkillPoints = 6;
        Assert.True(PlayerProgression.IsEligibleToLevelUp(player));
    }

    [Fact]
    public void RecordGamePlayed_RevealsPotential_OnceTheThresholdIsHit()
    {
        var rookie = MakePlayer(DevelopmentPotential.ProBowlSnub);
        Assert.False(rookie.DevelopmentPotentialRevealed);

        for (var i = 0; i < PlayerProgression.PotentialRevealGamesPlayed - 1; i++)
        {
            PlayerProgression.RecordGamePlayed(rookie);
        }

        Assert.False(rookie.DevelopmentPotentialRevealed);

        PlayerProgression.RecordGamePlayed(rookie);
        Assert.True(rookie.DevelopmentPotentialRevealed);
        Assert.Equal(PlayerProgression.PotentialRevealGamesPlayed, rookie.GamesPlayed);
    }

    [Fact]
    public void AwardSpp_NeverTouchesGamesPlayed_SoMultipleAwardsInOneGameDontDoubleCount()
    {
        var player = MakePlayer(DevelopmentPotential.SteadyEddie);

        PlayerProgression.RecordGamePlayed(player);
        PlayerProgression.AwardSpp(player, 3);
        PlayerProgression.AwardSpp(player, 4); // e.g. stats, then an MVP bonus

        Assert.Equal(1, player.GamesPlayed);
        Assert.Equal(7, player.SkillPoints);
    }
}
