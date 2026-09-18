using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>The raw result of one snap, before the Engine applies it to field position and scoring.</summary>
public sealed record PlayOutcome(
    int Yards,
    bool IsTurnover,
    Guid? OffensePlayerId,
    Guid? DefensePlayerId,
    IReadOnlyList<InjuryEvent> InjuryEvents);
