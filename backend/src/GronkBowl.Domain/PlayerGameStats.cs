namespace GronkBowl.Domain;

/// <summary>One player's stat line for one game - the atomic unit that season and career
/// totals are just sums of.</summary>
public sealed record PlayerGameStats(
    Guid PlayerId,
    int RushingAttempts,
    int RushingYards,
    int RushingTouchdowns,
    int Receptions,
    int ReceivingYards,
    int ReceivingTouchdowns,
    int TacklesOrHits,
    int TurnoversForced,
    int InjuriesCaused,
    int TimesInjured)
{
    public static PlayerGameStats Empty(Guid playerId) => new(playerId, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    public static PlayerGameStats operator +(PlayerGameStats a, PlayerGameStats b) => new(
        a.PlayerId,
        a.RushingAttempts + b.RushingAttempts,
        a.RushingYards + b.RushingYards,
        a.RushingTouchdowns + b.RushingTouchdowns,
        a.Receptions + b.Receptions,
        a.ReceivingYards + b.ReceivingYards,
        a.ReceivingTouchdowns + b.ReceivingTouchdowns,
        a.TacklesOrHits + b.TacklesOrHits,
        a.TurnoversForced + b.TurnoversForced,
        a.InjuriesCaused + b.InjuriesCaused,
        a.TimesInjured + b.TimesInjured);
}
