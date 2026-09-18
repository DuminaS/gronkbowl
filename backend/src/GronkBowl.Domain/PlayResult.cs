namespace GronkBowl.Domain;

public sealed record InjuryEvent(Guid PlayerId, InjuryResult Result);

// Self-contained on purpose: everything the Stats Aggregator and a post-game summary need is
// on the record itself, not re-derived from the Play catalog, which can change week to week.
public sealed record PlayResult(
    int PlayIndex,
    Guid OffensePlayId,
    Guid DefensePlayId,
    bool IsPassPlay,
    Guid? OffensePlayerId,
    Guid? DefensePlayerId,
    int YardsGained,
    bool IsTurnover,
    bool IsScore,
    IReadOnlyList<InjuryEvent> InjuryEvents);
