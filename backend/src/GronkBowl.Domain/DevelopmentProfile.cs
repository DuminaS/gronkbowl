using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public sealed record DevelopmentProfile(
    DevelopmentPotential Tier,
    string Name,
    double SppRateMultiplier,
    int LevelCeiling,
    bool HasCareerReroll,
    string FlavorText);

/// <summary>
/// The five HC09-style development tiers, from bust to superstar. SppRateMultiplier scales how
/// far SPP goes toward the next level (higher = faster, needs less cumulative SPP per level) -
/// SPP earned on the field stays purely objective and untouched; only how quickly it converts
/// into levels differs. See Master Design Doc for the full writeup.
/// </summary>
public static class DevelopmentProfiles
{
    public static readonly IReadOnlyDictionary<DevelopmentPotential, DevelopmentProfile> All = new Dictionary<DevelopmentPotential, DevelopmentProfile>
    {
        [DevelopmentPotential.CampFodder] = new(
            DevelopmentPotential.CampFodder, "Camp Fodder", SppRateMultiplier: 0.5, LevelCeiling: 2, HasCareerReroll: false,
            "Never quite gets there. Good depth, forever depth."),

        [DevelopmentPotential.ReplacementLevel] = new(
            DevelopmentPotential.ReplacementLevel, "Replacement Level", SppRateMultiplier: 0.75, LevelCeiling: 4, HasCareerReroll: false,
            "Keeps a roster spot warm. That's about it."),

        [DevelopmentPotential.SteadyEddie] = new(
            DevelopmentPotential.SteadyEddie, "Steady Eddie", SppRateMultiplier: 1.0, LevelCeiling: 6, HasCareerReroll: false,
            "Reliable, unspectacular, the glue guy every roster needs."),

        [DevelopmentPotential.ProBowlSnub] = new(
            DevelopmentPotential.ProBowlSnub, "Pro Bowl Snub", SppRateMultiplier: 1.35, LevelCeiling: 7, HasCareerReroll: false,
            "Good enough to be a star. Somehow always overlooked."),

        [DevelopmentPotential.FirstBallotHallOfFamer] = new(
            DevelopmentPotential.FirstBallotHallOfFamer, "First Ballot Hall of Famer", SppRateMultiplier: 1.75, LevelCeiling: 7, HasCareerReroll: true,
            "Generational. No debate needed."),
    };

    /// <summary>Rough league distribution for random generation - most players are ordinary,
    /// true busts and true stars are both rare.</summary>
    public static readonly IReadOnlyList<(DevelopmentPotential Tier, int Weight)> WeightedDistribution = new List<(DevelopmentPotential, int)>
    {
        (DevelopmentPotential.CampFodder, 15),
        (DevelopmentPotential.ReplacementLevel, 25),
        (DevelopmentPotential.SteadyEddie, 35),
        (DevelopmentPotential.ProBowlSnub, 18),
        (DevelopmentPotential.FirstBallotHallOfFamer, 7),
    };

    public static DevelopmentPotential RollRandom(Random rng)
    {
        var totalWeight = WeightedDistribution.Sum(entry => entry.Weight);
        var roll = rng.Next(0, totalWeight);
        var cumulative = 0;

        foreach (var (tier, weight) in WeightedDistribution)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                return tier;
            }
        }

        return DevelopmentPotential.SteadyEddie;
    }
}
