using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// A live draft's state machine: teams pick in order, one prospect at a time. Straight order
/// (not snake) for v1 - the competitive-balance lever already lives in the inverse-standings
/// order itself, not in how rounds alternate.
/// </summary>
public sealed class DraftBoard
{
    private readonly Dictionary<Guid, Player> _availableProspects;
    private readonly HashSet<Guid> _draftedProspectIds = new();

    public IReadOnlyList<Guid> PickOrder { get; }
    public int CurrentPickIndex { get; private set; }

    public DraftBoard(IEnumerable<Player> prospects, IReadOnlyList<Guid> draftOrder, int rounds)
    {
        _availableProspects = prospects.ToDictionary(p => p.Id);
        PickOrder = Enumerable.Range(0, rounds).SelectMany(_ => draftOrder).ToList();
    }

    public Guid? OnTheClock => CurrentPickIndex < PickOrder.Count ? PickOrder[CurrentPickIndex] : null;
    public bool IsComplete => CurrentPickIndex >= PickOrder.Count;
    public int OverallPickNumber => CurrentPickIndex + 1;

    public (Player Prospect, Contract Contract) MakePick(Guid teamId, Guid prospectId)
    {
        if (OnTheClock != teamId)
        {
            throw new InvalidOperationException($"Team {teamId} is not on the clock.");
        }

        if (!_availableProspects.TryGetValue(prospectId, out var prospect) || _draftedProspectIds.Contains(prospectId))
        {
            throw new InvalidOperationException($"Prospect {prospectId} is not available to draft.");
        }

        var contract = RookieWageScale.ForPick(OverallPickNumber);
        _draftedProspectIds.Add(prospectId);
        CurrentPickIndex++;

        return (prospect, contract);
    }
}
