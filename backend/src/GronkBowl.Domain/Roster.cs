using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

/// <summary>
/// A depth chart per position, ordered starter-first. "Starting lineup" and "bench" are
/// derived views over this, not separate lists - when a starter goes down, the next
/// available player in that position's depth list is already the answer, with no extra
/// bookkeeping required when an injury lands.
/// </summary>
public class Roster
{
    public Dictionary<FootballPosition, List<Guid>> DepthChart { get; init; } = new();
    public List<Guid> PracticeSquad { get; init; } = new();
    public List<Guid> InjuredReserve { get; init; } = new();

    public void SetDepthOrder(FootballPosition position, IEnumerable<Guid> orderedPlayerIds)
    {
        DepthChart[position] = orderedPlayerIds.ToList();
    }

    /// <summary>Adds a newly acquired player (a draft pick, a free-agent signing) to the
    /// bottom of their position's depth chart - they have to earn their way up, not start
    /// ahead of players already on the roster.</summary>
    public void AddToDepthChart(FootballPosition position, Guid playerId)
    {
        if (!DepthChart.TryGetValue(position, out var depth))
        {
            depth = new List<Guid>();
            DepthChart[position] = depth;
        }

        depth.Add(playerId);
    }

    public Guid? GetStarter(FootballPosition position, IReadOnlyDictionary<Guid, Player> players)
    {
        foreach (var id in GetDepthAt(position))
        {
            if (players.TryGetValue(id, out var player) && player.IsAvailableForLineup)
            {
                return id;
            }
        }

        return null;
    }

    public IReadOnlyList<Guid> GetAvailableAtPosition(FootballPosition position, IReadOnlyDictionary<Guid, Player> players)
    {
        return GetDepthAt(position)
            .Where(id => players.TryGetValue(id, out var player) && player.IsAvailableForLineup)
            .ToList();
    }

    private IReadOnlyList<Guid> GetDepthAt(FootballPosition position) =>
        DepthChart.TryGetValue(position, out var depth) ? depth : Array.Empty<Guid>();
}
