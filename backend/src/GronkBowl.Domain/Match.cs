namespace GronkBowl.Domain;

public class Match
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // Set by whoever resolves the match within a season (the League Orchestrator), not by the
    // Engine itself - MatchEngine.ResolveGame stays decoupled from the Season concept entirely.
    public Guid SeasonId { get; set; }

    public required Guid HomeTeamId { get; init; }
    public required Guid AwayTeamId { get; init; }
    public required int Week { get; init; }

    // Server-generated; never client-supplied, so results are always reproducible and disputable.
    public long Seed { get; init; }

    public List<PlayResult> EventLog { get; init; } = new();
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public bool IsResolved { get; set; }

    // Live per-down state - meaningful only while !IsResolved. Each down waits on both sides
    // submitting a play (PendingOffensePlayId/PendingDefensePlayId) before it resolves and these
    // advance to the next down; once IsResolved they're left at whatever they were on the final
    // play and should not be read.
    public int Quarter { get; set; } = 1;
    public int PlaysRemainingInQuarter { get; set; } = 12;
    public int Down { get; set; } = 1;
    public int DistanceToGo { get; set; } = 10;
    public int FieldPosition { get; set; } = 25;
    public Guid PossessionTeamId { get; set; }
    public Guid? PendingOffensePlayId { get; set; }
    public Guid? PendingDefensePlayId { get; set; }
}
