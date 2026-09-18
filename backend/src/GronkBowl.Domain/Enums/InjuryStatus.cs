namespace GronkBowl.Domain.Enums;

/// <summary>Persistent injury state. BangedUp clears automatically after the game; Out carries a week count.</summary>
public enum InjuryStatus
{
    Healthy,
    BangedUp,
    Out,
    Niggling,
    SeasonEnding,
    CareerEnding,
    Deceased
}
