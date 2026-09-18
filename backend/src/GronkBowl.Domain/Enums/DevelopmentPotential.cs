namespace GronkBowl.Domain.Enums;

/// <summary>
/// The HC09 half of the leveling hybrid: how fast a player's SPP converts into levels, and how
/// high they can ultimately go. Hidden for rookies until they've proven it on the field - see
/// Player.DevelopmentPotentialRevealed.
/// </summary>
public enum DevelopmentPotential
{
    CampFodder,
    ReplacementLevel,
    SteadyEddie,
    ProBowlSnub,
    FirstBallotHallOfFamer,
}
