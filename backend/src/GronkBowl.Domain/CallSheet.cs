using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public class CallSheet
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid TeamId { get; init; }
    public required int Week { get; init; }

    // A team needs a plan for both sides of the ball every week - one bucket map for when it
    // has the ball, one for when it doesn't. Bucket -> ordered Play ids; list order is
    // priority within that situation.
    public Dictionary<SituationalBucket, List<Guid>> OffensiveSituationalPlays { get; init; } = new();
    public Dictionary<SituationalBucket, List<Guid>> DefensiveSituationalPlays { get; init; } = new();

    // Personnel comes from the Team's Roster.DepthChart at resolution time, not a separate
    // list here - one source of truth for "who's starting," so a mid-week injury never
    // leaves the call sheet pointing at a player who can't take the field.
    public bool Submitted { get; set; }

    // True when the League Orchestrator generated this sheet because the coach missed
    // the weekly deadline - a penalty, drawn only from the team's already-installed plays.
    public bool WasRandomlyGenerated { get; set; }

    /// <summary>
    /// The next play to call for a situation: the bucket's top-priority entry, falling back to
    /// Standard, then to any installed play at all, so a thin call sheet never leaves the
    /// Engine with nothing to run.
    /// </summary>
    public Guid? SelectPlay(PlayCategory category, SituationalBucket bucket)
    {
        var buckets = category == PlayCategory.Offense ? OffensiveSituationalPlays : DefensiveSituationalPlays;

        if (buckets.TryGetValue(bucket, out var forBucket) && forBucket.Count > 0)
        {
            return forBucket[0];
        }

        if (bucket != SituationalBucket.Standard &&
            buckets.TryGetValue(SituationalBucket.Standard, out var standard) &&
            standard.Count > 0)
        {
            return standard[0];
        }

        return buckets.Values.FirstOrDefault(list => list.Count > 0)?[0];
    }
}
