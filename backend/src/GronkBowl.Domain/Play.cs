using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public class Play
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required PlayCategory Category { get; init; }

    // Default plays are permanently public league-wide (no scouting fog-of-war) and install
    // at zero cost against the weekly practice-rep cap. See Master Design Doc s5.
    public bool IsDefaultPlay { get; init; }

    public List<PlayAssignment> Assignments { get; init; } = new();
    public List<string> Tags { get; init; } = new();
    public int RiskProfile { get; set; }

    public required bool IsPassPlay { get; init; }

    // The featured player this play is built around: the ball carrier on a run, the primary
    // receiver on a pass, or the primary run-fit/coverage defender on defense. Used by the
    // Engine to pick who touches the ball and who's in on the hit, so post-game summaries can
    // name a player instead of reporting an anonymous yardage total.
    public required FootballPosition PrimaryPosition { get; init; }
}
