namespace GronkBowl.Domain;

/// <summary>
/// The persisted state of one draft: the full pick order (one entry per pick, across every
/// round), which prospects are in this class, and which have already been taken. A DraftBoard
/// (GronkBowl.Engine) is just a live, in-memory view rebuilt from this plus the Players table -
/// this is the row that survives between requests and process restarts.
/// </summary>
public class DraftState
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid SeasonId { get; init; }
    public List<Guid> PickOrder { get; init; } = new();
    public int CurrentPickIndex { get; set; }
    public List<Guid> ProspectPlayerIds { get; init; } = new();
    public List<Guid> DraftedProspectPlayerIds { get; init; } = new();

    public bool IsComplete => CurrentPickIndex >= PickOrder.Count;
    public Guid? OnTheClock => IsComplete ? null : PickOrder[CurrentPickIndex];
    public int OverallPickNumber => CurrentPickIndex + 1;
}
