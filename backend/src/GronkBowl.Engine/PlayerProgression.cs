using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// The SPP/level bookkeeping half of the leveling hybrid: cumulative thresholds from the
/// design brief (6/16/31/51/76/121 for levels 2-7), scaled per player by their (usually
/// hidden) Development Potential - a faster developer needs less cumulative SPP per level, a
/// bust needs more. SPP earned on the field (see StatsAggregator) is never touched by this -
/// only how quickly it converts into levels differs.
/// </summary>
public static class PlayerProgression
{
    private static readonly int[] BaseThresholds = { 0, 6, 16, 31, 51, 76, 121 };
    public const int MaxLevel = 7;
    public const int PotentialRevealGamesPlayed = 8;

    public static int SppNeededForLevel(int level, Player player)
    {
        if (level < 1 || level > MaxLevel)
        {
            throw new ArgumentOutOfRangeException(nameof(level), $"Level must be between 1 and {MaxLevel}.");
        }

        var multiplier = DevelopmentProfiles.All[player.DevelopmentPotential].SppRateMultiplier;
        return (int)Math.Ceiling(BaseThresholds[level - 1] / multiplier);
    }

    public static bool IsEligibleToLevelUp(Player player)
    {
        var ceiling = DevelopmentProfiles.All[player.DevelopmentPotential].LevelCeiling;
        if (player.Level >= ceiling || player.Level >= MaxLevel)
        {
            return false;
        }

        return player.SkillPoints >= SppNeededForLevel(player.Level + 1, player);
    }

    /// <summary>Objective, position/potential-blind - the same play earns the same SPP no
    /// matter who's carrying the ball. A player can earn SPP more than once in a game (box
    /// score stats, then an MVP bonus), so this never touches GamesPlayed - call
    /// RecordGamePlayed exactly once per participant instead.</summary>
    public static void AwardSpp(Player player, int spp) => player.SkillPoints += spp;

    /// <summary>Call once per player who actually took part in a game (see
    /// StatsAggregator.ComputeGameStats). Reveals a rookie's true potential once they've
    /// played enough games to have shown it.</summary>
    public static void RecordGamePlayed(Player player)
    {
        player.GamesPlayed++;

        if (!player.DevelopmentPotentialRevealed && player.GamesPlayed >= PotentialRevealGamesPlayed)
        {
            player.DevelopmentPotentialRevealed = true;
        }
    }
}
