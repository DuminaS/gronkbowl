using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// The Casualty &amp; Injury Table from the Master Design Doc (s6): a 2d6 armor check, then a
/// 2d6 severity roll on a hit that breaks armor. Deterministic given the same Random seed
/// and call order - no hidden state, so a disputed result can always be re-derived.
/// </summary>
public static class CasualtyResolver
{
    /// <returns>null if the armor check fails (no injury); otherwise the resolved InjuryResult.</returns>
    public static InjuryResult? TryResolveHit(Player hitter, Player target, Random rng)
    {
        // Strength contributes a small bonus, not a dominant one - a raw 2d6 vs AV is already
        // the whole check in Blood Bowl; Strength is what separates a scary hitter from an
        // average one, not what makes armor break the norm rather than the exception.
        //
        // Durability works the other way, on the target's side: before this, a race's actual
        // Durability stat did nothing but shave a week off Missed-Time duration, so Ironkin's
        // toughness was entirely their flat "Unbreakable" racial bonus, not their 95 Durability
        // - Aelari (50) and Skitterkin (45) got zero attribute-driven protection, and Thornhide
        // (80) was just as armor-resistant as Ironkin (95) since neither value mattered here.
        var armorRoll = RollTwoD6(rng)
            + (hitter.Attributes.Strength / 50) + HitterArmorBonus(hitter)
            - (target.Attributes.Durability / 30);
        if (hitter.Skills.Contains(Skill.MightyBlow))
        {
            armorRoll += 1;
        }
        if (armorRoll < target.ArmorValue)
        {
            return null;
        }

        var severityRoll = RollTwoD6(rng) + SeverityModifier(hitter, target);
        severityRoll = Math.Clamp(severityRoll, 2, 12);

        return severityRoll switch
        {
            2 => ResolveCatastrophic(rng),
            3 => new InjuryResult(InjuryStatus.SeasonEnding, severityRoll, WeeksOut: 0, PermanentAttributePenalty: 0, CausesDeadCap: false, EndsCareer: false),
            4 or 5 => new InjuryResult(InjuryStatus.Niggling, severityRoll, WeeksOut: 0, PermanentAttributePenalty: 5, CausesDeadCap: false, EndsCareer: false),
            >= 6 and <= 8 => new InjuryResult(InjuryStatus.Out, severityRoll, WeeksOut: RollMissedTimeWeeks(rng, target), PermanentAttributePenalty: 0, CausesDeadCap: false, EndsCareer: false),
            9 or 10 => new InjuryResult(InjuryStatus.BangedUp, severityRoll, WeeksOut: 0, PermanentAttributePenalty: 0, CausesDeadCap: false, EndsCareer: false),
            _ => new InjuryResult(InjuryStatus.Healthy, severityRoll, WeeksOut: 0, PermanentAttributePenalty: 0, CausesDeadCap: false, EndsCareer: false),
        };
    }

    private static InjuryResult ResolveCatastrophic(Random rng)
    {
        var subRoll = rng.Next(1, 7);
        return subRoll <= 4
            ? new InjuryResult(InjuryStatus.CareerEnding, 2, WeeksOut: 0, PermanentAttributePenalty: 0, CausesDeadCap: true, EndsCareer: true)
            : new InjuryResult(InjuryStatus.Deceased, 2, WeeksOut: 0, PermanentAttributePenalty: 0, CausesDeadCap: true, EndsCareer: true);
    }

    private static int RollMissedTimeWeeks(Random rng, Player target)
    {
        var baseWeeks = rng.Next(1, 5);
        var toughened = target.Attributes.Durability >= 80 ? baseWeeks - 1 : baseWeeks;
        return Math.Max(1, toughened);
    }

    private static int HitterArmorBonus(Player hitter) =>
        hitter.Traits.Contains(PersonalTrait.HotHead) ? 1 : 0;

    /// <summary>Positive = a worse outcome for the target (lower table roll); negative = milder.</summary>
    private static int SeverityModifier(Player hitter, Player target)
    {
        var modifier = 0;

        if (hitter.Race == Race.Thornhide) modifier -= 1; // Savage Blow
        if (target.Race == Race.Ironkin) modifier += 1; // Unbreakable
        if (target.Traits.Contains(PersonalTrait.IronWill)) modifier += 1;
        if (target.Traits.Contains(PersonalTrait.InjuryProne)) modifier -= 1;
        if (hitter.Skills.Contains(Skill.DirtyPlayer)) modifier -= 1;

        return modifier;
    }

    private static int RollTwoD6(Random rng) => rng.Next(1, 7) + rng.Next(1, 7);
}
