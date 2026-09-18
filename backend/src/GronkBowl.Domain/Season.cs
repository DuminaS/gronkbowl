namespace GronkBowl.Domain;

public sealed record TeamRecord(int Wins, int Losses, int Ties);

/// <summary>An unplayed fixture. Becomes a Match once the Engine resolves it.</summary>
public sealed record ScheduledGame(int Week, Guid HomeTeamId, Guid AwayTeamId);

public class Season
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required int Year { get; init; }
    public required string RulesVersion { get; init; }
    public List<ScheduledGame> Schedule { get; init; } = new();
    public List<Match> CompletedMatches { get; init; } = new();
    public Dictionary<Guid, TeamRecord> Standings { get; init; } = new();
    public List<Guid> DraftOrder { get; init; } = new();
}
