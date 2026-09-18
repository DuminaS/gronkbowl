using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public sealed record InjuryResult(
    InjuryStatus Status,
    int SeverityRoll,
    int WeeksOut,
    int PermanentAttributePenalty,
    bool CausesDeadCap,
    bool EndsCareer);
