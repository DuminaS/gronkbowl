namespace GronkBowl.Domain;

public sealed record InjuryEvent(Guid PlayerId, InjuryResult Result);

// Self-contained on purpose: everything the Stats Aggregator and a post-game summary need is
// on the record itself, not re-derived from the Play catalog, which can change week to week.
// The situational fields (Quarter..PossessionTeamId) are the pre-snap game state for this play -
// captured so a client can replay/animate the drive without re-simulating it. HomeScoreAfter/
// AwayScoreAfter are the score once this play (and any resulting score) has been applied.
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
    IReadOnlyList<InjuryEvent> InjuryEvents,
    int Quarter,
    int Down,
    int DistanceToGo,
    int FieldPosition,
    Guid PossessionTeamId,
    int HomeScoreAfter,
    int AwayScoreAfter);
