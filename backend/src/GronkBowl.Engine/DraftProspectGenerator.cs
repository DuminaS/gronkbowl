using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// v1 draft pool generation: with only four launch races, every race is equally likely - the
/// full rarity-tiered scarcity model is a post-v1 upgrade once the race roster grows (Master
/// Design Doc s0.5). Attributes roll to a fraction of the race's career ceiling, since a
/// rookie shouldn't enter the league already maxed out.
/// </summary>
public static class DraftProspectGenerator
{
    private const int MinCeilingPercent = 40;
    private const int MaxCeilingPercent = 75;
    private const int TraitChancePercent = 40;

    private static readonly Race[] LaunchRaces = Enum.GetValues<Race>();
    private static readonly PersonalTrait[] AllTraits = Enum.GetValues<PersonalTrait>();
    private static readonly FootballPosition[] AllPositions = Enum.GetValues<FootballPosition>();

    public static List<Player> GenerateClass(int count, Random rng)
    {
        var prospects = new List<Player>(count);

        for (var i = 0; i < count; i++)
        {
            var race = LaunchRaces[rng.Next(LaunchRaces.Length)];
            var position = AllPositions[rng.Next(AllPositions.Length)];

            var prospect = new Player
            {
                Name = $"Prospect {i + 1}",
                Race = race,
                Position = position,
                Attributes = RollAttributes(race, rng),
                ArmorValue = 9 + rng.Next(0, 3),
                DevelopmentPotential = DevelopmentProfiles.RollRandom(rng),
            };

            if (rng.Next(0, 100) < TraitChancePercent)
            {
                prospect.Traits.Add(AllTraits[rng.Next(AllTraits.Length)]);
            }

            prospects.Add(prospect);
        }

        return prospects;
    }

    /// <summary>Rookie attributes for a given race, rolled to 40-75% of its career ceiling -
    /// reusable anywhere a fresh player needs to be generated (draft classes, demo rosters).</summary>
    public static Attributes RollAttributes(Race race, Random rng)
    {
        var ceiling = RaceProfiles.All[race].CeilingAttributes;
        return new Attributes(
            Speed: RollTowardCeiling(ceiling.Speed, rng),
            Strength: RollTowardCeiling(ceiling.Strength, rng),
            Agility: RollTowardCeiling(ceiling.Agility, rng),
            Awareness: RollTowardCeiling(ceiling.Awareness, rng),
            Durability: RollTowardCeiling(ceiling.Durability, rng));
    }

    private static int RollTowardCeiling(int ceiling, Random rng)
    {
        var percent = rng.Next(MinCeilingPercent, MaxCeilingPercent + 1);
        return ceiling * percent / 100;
    }
}
