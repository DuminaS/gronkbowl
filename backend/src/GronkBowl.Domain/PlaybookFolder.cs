using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

/// <summary>
/// A coach's own organization of their playbook - a named group of plays on one side of the
/// ball, e.g. "Passing Downs" or "Short Yardage." Replaces the old fixed SituationalBucket
/// taxonomy: the coach names and files plays however makes sense to them, not into a system-
/// defined list of situations. Standing team state, not something resubmitted week to week.
/// </summary>
public class PlaybookFolder
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid TeamId { get; init; }
    public required PlayCategory Category { get; init; }
    public required string Name { get; set; }

    // Order is the coach's own priority within the folder, not enforced by the engine.
    public List<Guid> PlayIds { get; init; } = new();
}
