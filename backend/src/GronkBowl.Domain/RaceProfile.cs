using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public sealed record RaceProfile(
    Race Race,
    Attributes CeilingAttributes,
    string RacialAbilityName,
    string RacialAbilityDescription);

/// <summary>
/// Career-long attribute ceilings and the single core racial ability per launch race.
/// Racial Exclusive Trait pools and the Relationship Ledger are post-v1 (see Master Design Doc s0.5).
/// </summary>
public static class RaceProfiles
{
    public static readonly IReadOnlyDictionary<Race, RaceProfile> All = new Dictionary<Race, RaceProfile>
    {
        [Race.Ironkin] = new(
            Race.Ironkin,
            new Attributes(Speed: 55, Strength: 85, Agility: 50, Awareness: 65, Durability: 95),
            "Unbreakable",
            "Reduced injury severity on the Casualty Table."),

        [Race.Aelari] = new(
            Race.Aelari,
            new Attributes(Speed: 95, Strength: 45, Agility: 95, Awareness: 85, Durability: 50),
            "Featherstep",
            "Bonus evasion against the first tackle attempt."),

        [Race.Thornhide] = new(
            Race.Thornhide,
            new Attributes(Speed: 60, Strength: 95, Agility: 45, Awareness: 50, Durability: 80),
            "Savage Blow",
            "Bonus on the injury roll when this player lands a hit."),

        [Race.Skitterkin] = new(
            Race.Skitterkin,
            new Attributes(Speed: 90, Strength: 40, Agility: 90, Awareness: 55, Durability: 45),
            "Scurry",
            "One extra evasion die when attempting to dodge a tackle."),
    };
}
