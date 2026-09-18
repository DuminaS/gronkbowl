using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

public enum SubmitPlayOutcome { Accepted, AlreadySubmitted, TeamNotInMatch, IllegalPlay, MatchAlreadyOver }

/// <summary>
/// Runs a match one down at a time, waiting on both coaches to submit a play for the current
/// down instead of resolving a whole game from pre-submitted call sheets in one batch (see
/// MatchEngine.ResolveGame, which still exists and is reused internally here for the actual
/// down-resolution math via MatchEngine.ResolveOneDown - only the orchestration differs).
/// </summary>
public static class LiveMatchOrchestrator
{
    public static Match StartMatch(Team homeTeam, Team awayTeam, int week, long seed) => new()
    {
        HomeTeamId = homeTeam.Id,
        AwayTeamId = awayTeam.Id,
        Week = week,
        Seed = seed,
        PossessionTeamId = homeTeam.Id,
        Quarter = 1,
        PlaysRemainingInQuarter = 12,
        Down = 1,
        DistanceToGo = 10,
        FieldPosition = 25,
    };

    /// <summary>Records a team's play choice for the current down. Only clears once both sides
    /// have submitted (see TryResolveCurrentDown) - a second submission from the same team
    /// before that is refused rather than silently overwriting their first choice.</summary>
    public static SubmitPlayOutcome SubmitPlay(Match match, Team homeTeam, Team awayTeam, Guid teamId, Guid playId)
    {
        if (match.IsResolved)
        {
            return SubmitPlayOutcome.MatchAlreadyOver;
        }

        if (teamId != match.HomeTeamId && teamId != match.AwayTeamId)
        {
            return SubmitPlayOutcome.TeamNotInMatch;
        }

        var isOffense = teamId == match.PossessionTeamId;
        var team = teamId == homeTeam.Id ? homeTeam : awayTeam;
        var expectedCategory = isOffense ? PlayCategory.Offense : PlayCategory.Defense;

        if (!team.Playbook.Any(p => p.Id == playId && p.Category == expectedCategory))
        {
            return SubmitPlayOutcome.IllegalPlay;
        }

        if (isOffense)
        {
            if (match.PendingOffensePlayId is not null)
            {
                return SubmitPlayOutcome.AlreadySubmitted;
            }

            match.PendingOffensePlayId = playId;
        }
        else
        {
            if (match.PendingDefensePlayId is not null)
            {
                return SubmitPlayOutcome.AlreadySubmitted;
            }

            match.PendingDefensePlayId = playId;
        }

        return SubmitPlayOutcome.Accepted;
    }

    /// <summary>Resolves the current down if both sides have submitted, returning the play that
    /// was just recorded - or null if the match is still waiting on someone.</summary>
    public static PlayResult? TryResolveCurrentDown(
        Match match, Team homeTeam, Team awayTeam, IReadOnlyDictionary<Guid, Player> players, Random rng)
    {
        if (match.IsResolved || match.PendingOffensePlayId is not { } offensePlayId || match.PendingDefensePlayId is not { } defensePlayId)
        {
            return null;
        }

        var offenseIsHome = match.PossessionTeamId == homeTeam.Id;
        var offenseTeam = offenseIsHome ? homeTeam : awayTeam;
        var defenseTeam = offenseIsHome ? awayTeam : homeTeam;

        var offensePlay = offenseTeam.Playbook.FirstOrDefault(p => p.Id == offensePlayId);
        var defensePlay = defenseTeam.Playbook.FirstOrDefault(p => p.Id == defensePlayId);
        if (offensePlay is null || defensePlay is null)
        {
            // Shouldn't happen - SubmitPlay already validated against the Playbook - but a live
            // match should never crash on a data inconsistency, only clear the bad pending state.
            match.PendingOffensePlayId = null;
            match.PendingDefensePlayId = null;
            return null;
        }

        var state = MatchEngine.ToGameState(match);
        MatchEngine.ResolveOneDown(match, state, offensePlay, offenseTeam, defensePlay, defenseTeam, players, rng);
        MatchEngine.SyncFromGameState(match, state);

        match.PendingOffensePlayId = null;
        match.PendingDefensePlayId = null;

        return match.EventLog[^1];
    }

    /// <summary>
    /// The timeout-fallback entry point (invoked explicitly for now rather than by a scheduled
    /// job - see the plan's "explicitly deferred" section): auto-picks a play for whichever side
    /// hasn't submitted yet, preferring a random play from one of that team's own folders for
    /// this side of the ball, falling back to any installed play of the right category.
    /// </summary>
    public static PlayResult? ForceResolveCurrentDown(
        Match match, Team homeTeam, Team awayTeam, IReadOnlyList<PlaybookFolder> folders,
        IReadOnlyDictionary<Guid, Player> players, Random rng)
    {
        if (match.IsResolved)
        {
            return null;
        }

        var offenseTeam = match.PossessionTeamId == homeTeam.Id ? homeTeam : awayTeam;
        var defenseTeam = match.PossessionTeamId == homeTeam.Id ? awayTeam : homeTeam;

        match.PendingOffensePlayId ??= AutoPickPlay(offenseTeam, folders, PlayCategory.Offense, rng);
        match.PendingDefensePlayId ??= AutoPickPlay(defenseTeam, folders, PlayCategory.Defense, rng);

        return TryResolveCurrentDown(match, homeTeam, awayTeam, players, rng);
    }

    private static Guid? AutoPickPlay(Team team, IReadOnlyList<PlaybookFolder> folders, PlayCategory category, Random rng)
    {
        var candidateFolders = folders.Where(f => f.TeamId == team.Id && f.Category == category && f.PlayIds.Count > 0).ToList();
        if (candidateFolders.Count > 0)
        {
            var folder = candidateFolders[rng.Next(candidateFolders.Count)];
            return folder.PlayIds[rng.Next(folder.PlayIds.Count)];
        }

        var installed = team.Playbook.Where(p => p.Category == category).ToList();
        return installed.Count > 0 ? installed[rng.Next(installed.Count)].Id : null;
    }
}
