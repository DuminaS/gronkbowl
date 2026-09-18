using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

public enum LevelUpOutcomeKind
{
    NewSkill,
    SpeedOrArmorBoost,
    AgilityBoost,
    StrengthBoost,
}

public enum BoostableStat
{
    Speed,
    ArmorValue,
    Agility,
    Strength,
}

/// <summary>Both individual dice are kept, not just the sum - doubles matter independently of
/// what the sum-based table says (a natural 12 is always doubles; a 10 might or might not be).</summary>
public sealed record LevelUpRoll(int Die1, int Die2, int Sum, bool IsDoubles, LevelUpOutcomeKind BaseOutcome)
{
    public bool CanTakeSecondarySkillInstead => IsDoubles;
}

/// <summary>
/// The Blood Bowl half of the leveling hybrid, exactly as specified: 2d6 on level-up, 2-9 = new
/// primary skill, 10 = +1 Speed or Armor Value (coach's choice), 11 = +1 Agility, 12 = +1
/// Strength, doubles = may take a secondary-category skill instead. Deliberately does not
/// auto-pick a skill or stat - the coach chooses, the same way a draft pick or a trade is a
/// deliberate call, not something the engine decides for you.
/// </summary>
public static class LevelUpResolver
{
    public static LevelUpRoll Roll(Random rng)
    {
        var die1 = rng.Next(1, 7);
        var die2 = rng.Next(1, 7);
        var sum = die1 + die2;
        var isDoubles = die1 == die2;

        var outcome = sum switch
        {
            10 => LevelUpOutcomeKind.SpeedOrArmorBoost,
            11 => LevelUpOutcomeKind.AgilityBoost,
            12 => LevelUpOutcomeKind.StrengthBoost,
            _ => LevelUpOutcomeKind.NewSkill,
        };

        return new LevelUpRoll(die1, die2, sum, isDoubles, outcome);
    }

    /// <summary>Rerolls once, for a First Ballot Hall of Famer who hasn't used their career
    /// reroll yet. Rerolling is the coach's call, not automatic on a bad result.</summary>
    public static LevelUpRoll? Reroll(Player player, Random rng)
    {
        var profile = DevelopmentProfiles.All[player.DevelopmentPotential];
        if (!profile.HasCareerReroll || player.UsedCareerReroll)
        {
            return null;
        }

        player.UsedCareerReroll = true;
        return Roll(rng);
    }

    public static void ApplyStatBoost(Player player, BoostableStat stat)
    {
        player.Attributes = stat switch
        {
            BoostableStat.Speed => player.Attributes with { Speed = player.Attributes.Speed + 1 },
            BoostableStat.Agility => player.Attributes with { Agility = player.Attributes.Agility + 1 },
            BoostableStat.Strength => player.Attributes with { Strength = player.Attributes.Strength + 1 },
            BoostableStat.ArmorValue => player.Attributes,
            _ => throw new ArgumentOutOfRangeException(nameof(stat)),
        };

        if (stat == BoostableStat.ArmorValue)
        {
            player.ArmorValue++;
        }

        player.Level++;
    }

    public static void ApplySkill(Player player, Skill skill)
    {
        player.Skills.Add(skill);
        player.Level++;
    }
}
