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
}
