using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// Resolves one snap: a line contest (Block vs Rush), then a run/pass-specific outcome, then
/// one injury check on the play's featured players. Deliberately simpler than the full
/// grid/timing-window model in the design doc - readable and explainable comes first for v1;
/// the richer per-route simulation is a post-v1 upgrade to this same interface.
/// </summary>
public static class PlayResolver
{
    public static PlayOutcome Resolve(
        Play offensePlay, Team offenseTeam,
        Play defensePlay, Team defenseTeam,
        IReadOnlyDictionary<Guid, Player> players,
        Random rng)
    {
        var offensePlayerId = offenseTeam.Roster.GetStarter(offensePlay.PrimaryPosition, players);
        var defensePlayerId = defenseTeam.Roster.GetStarter(defensePlay.PrimaryPosition, players);

        var offenseLine = RollAssignmentTotal(offensePlay, offenseTeam, AssignmentRole.Block, players, rng);
        var defenseLine = RollAssignmentTotal(defensePlay, defenseTeam, AssignmentRole.Rush, players, rng);
        var lineMargin = offenseLine - defenseLine;

        var (yards, isTurnover) = offensePlay.IsPassPlay
            ? ResolvePass(offensePlayerId, defensePlayerId, defensePlay, defenseTeam, players, lineMargin, rng)
            : ResolveRun(offensePlayerId, defensePlayerId, players, lineMargin, rng);

        var injuryEvents = ApplyHit(offensePlayerId, defensePlayerId, players, rng);

        return new PlayOutcome(yards, isTurnover, offensePlayerId, defensePlayerId, injuryEvents);
    }

    private static int RollAssignmentTotal(
        Play play, Team team, AssignmentRole role, IReadOnlyDictionary<Guid, Player> players, Random rng)
    {
        // A defense-side Agility lever was tried here (to give a low-Strength race's defense a
        // counter against a Strength-heavy offense) and reverted - see Master Design Doc /
        // README balance notes. It over-corrected: Aelari's Awareness already carries their
        // passing offense AND their pass coverage, so stacking an Agility bonus onto their run
        // defense too gave them a third compounding advantage instead of fixing anything.
        var total = RollTwoD6(rng);
        foreach (var assignment in play.Assignments.Where(a => a.Role == role))
        {
            var playerId = team.Roster.GetStarter(assignment.Slot, players);
            if (playerId is { } id && players.TryGetValue(id, out var player))
            {
                total += player.Attributes.Strength / 25;
            }
        }

        return total;
    }

    private static (int Yards, bool IsTurnover) ResolveRun(
        Guid? ballCarrierId, Guid? pursuitDefenderId, IReadOnlyDictionary<Guid, Player> players, int lineMargin, Random rng)
    {
        if (ballCarrierId is not { } id || !players.TryGetValue(id, out var ballCarrier))
        {
            return (0, false);
        }

        // A shifty, fast ball carrier can create something even behind a mediocre line - the
        // line battle is still the dominant term, but it isn't the *only* one, so a
        // high-Agility race retains some rushing value even without a Strength-heavy front.
        var elusiveness = ballCarrier.Attributes.Agility / 30;
        var evasion = RacialEvasionBonus(ballCarrier, rng);

        var jitter = rng.Next(-2, 4);
        var yards = Math.Clamp(2 + lineMargin + elusiveness + evasion + jitter, -5, 25);
        yards += BreakawayBonus(ballCarrier, GetPlayer(pursuitDefenderId, players), yards, rng);

        var fumbleRoll = rng.Next(1, 21);
        var fumbleThreshold = lineMargin < -3 ? 2 : 1;
        return (yards, fumbleRoll <= fumbleThreshold);
    }

    private static (int Yards, bool IsTurnover) ResolvePass(
        Guid? receiverId, Guid? pursuitDefenderId, Play defensePlay, Team defenseTeam,
        IReadOnlyDictionary<Guid, Player> players, int lineMargin, Random rng)
    {
        if (receiverId is not { } rId || !players.TryGetValue(rId, out var receiver))
        {
            return (0, false);
        }

        var coverageAssignment = defensePlay.Assignments
            .FirstOrDefault(a => a.Role is AssignmentRole.CoverageMan or AssignmentRole.CoverageZone);
        var coverageId = coverageAssignment is null ? null : defenseTeam.Roster.GetStarter(coverageAssignment.Slot, players);

        // Skill scaling is deliberately higher here than the line battle's Strength/25: a
        // finesse race needs a real lever to win a passing down even when it can't win the
        // trenches, or its whole racial identity collapses into "the team that loses."
        var routeRoll = RollTwoD6(rng) + receiver.Attributes.Agility / 20 + receiver.Attributes.Awareness / 20;

        var coverageRoll = RollTwoD6(rng);
        if (coverageId is { } cId && players.TryGetValue(cId, out var coverage))
        {
            coverageRoll += coverage.Attributes.Awareness / 20;
        }

        // Pressure still matters - a collapsing pocket should hurt - but it's capped rather
        // than an unbounded subtraction, so a lost line battle makes a completion harder, not
        // mathematically impossible regardless of how good the receiver is.
        const int maxPressurePenalty = 4;
        var pressurePenalty = lineMargin < 0 ? Math.Min(Math.Abs(lineMargin), maxPressurePenalty) : 0;
        routeRoll -= pressurePenalty;

        if (routeRoll < coverageRoll)
        {
            var interceptionRoll = rng.Next(1, 21);
            return (0, interceptionRoll == 1);
        }

        var yardsAfterCatch = RacialEvasionBonus(receiver, rng);
        var yards = Math.Clamp(4 + (routeRoll - coverageRoll) + yardsAfterCatch, 0, 40);
        yards += BreakawayBonus(receiver, GetPlayer(pursuitDefenderId, players), yards, rng);
        return (yards, false);
    }

    /// <summary>
    /// Speed did nothing mechanically anywhere in the engine before this - the one core
    /// attribute with no hookup at all, despite being Ironkin's other weak stat alongside
    /// Agility. A ball carrier who's meaningfully faster than the nearest defender gets a real
    /// chance to turn a good gain into a breakaway; a slow defense (Ironkin, Thornhide) has no
    /// way to run one down. Only triggers on an already-positive gain - Speed creates upside on
    /// a play that's already working, it doesn't bail out a stuffed one.
    /// </summary>
    private static int BreakawayBonus(Player ballCarrier, Player? pursuitDefender, int currentYards, Random rng)
    {
        if (currentYards < 5)
        {
            return 0;
        }

        var speedGap = ballCarrier.Attributes.Speed - (pursuitDefender?.Attributes.Speed ?? 50);
        if (speedGap <= 5)
        {
            return 0;
        }

        var breakawayChancePercent = Math.Min(speedGap, 35);
        return rng.Next(0, 100) < breakawayChancePercent ? rng.Next(5, 16) : 0;
    }

    private static Player? GetPlayer(Guid? id, IReadOnlyDictionary<Guid, Player> players) =>
        id is { } value && players.TryGetValue(value, out var player) ? player : null;

    /// <summary>
    /// Wires the two evasion-flavored Racial Abilities (Master Design Doc s0.5 / RaceProfiles)
    /// into actual yardage, rather than leaving them as unused flavor text. They're not the
    /// same size on purpose: Aelari's Featherstep is one bonus evasion roll, but Skitterkin's
    /// Scurry is literally "one extra evasion die" - a second independent roll stacked on top -
    /// which also happens to be exactly what closes their large Awareness deficit against
    /// Aelari (30 points, worth ~1.5 on the /20-scaled skill rolls) rather than leaving them
    /// strictly worse in every matchup.
    /// </summary>
    private static int RacialEvasionBonus(Player ballCarrier, Random rng) => ballCarrier.Race switch
    {
        Race.Aelari => rng.Next(0, 4),
        Race.Skitterkin => rng.Next(0, 4) + rng.Next(0, 4),
        _ => 0,
    };

    private static IReadOnlyList<InjuryEvent> ApplyHit(
        Guid? targetId, Guid? hitterId, IReadOnlyDictionary<Guid, Player> players, Random rng)
    {
        if (targetId is not { } tId || hitterId is not { } hId)
        {
            return Array.Empty<InjuryEvent>();
        }

        if (!players.TryGetValue(tId, out var target) || !players.TryGetValue(hId, out var hitter))
        {
            return Array.Empty<InjuryEvent>();
        }

        var result = CasualtyResolver.TryResolveHit(hitter, target, rng);
        if (result is null)
        {
            return Array.Empty<InjuryEvent>();
        }

        target.InjuryStatus = result.Status;
        target.InjuryWeeksRemaining = result.WeeksOut;
        target.PermanentAttributePenalty += result.PermanentAttributePenalty;

        return new[] { new InjuryEvent(target.Id, result) };
    }

    private static int RollTwoD6(Random rng) => rng.Next(1, 7) + rng.Next(1, 7);
}
