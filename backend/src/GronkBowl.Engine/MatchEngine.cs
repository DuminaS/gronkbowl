using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// The pure resolve() function from the Master Design Doc s4: given two teams, their submitted
/// call sheets, the shared player pool, and a server-generated seed, produces a fully resolved
/// Match with no side effects and no mid-resolution reads of live state. Same inputs, same
/// seed, same result - every time.
/// </summary>
public static class MatchEngine
{
    public static Match ResolveGame(
        Team homeTeam, Team awayTeam,
        CallSheet homeCallSheet, CallSheet awayCallSheet,
        IReadOnlyDictionary<Guid, Player> players,
        int week, long seed)
    {
        var rng = new Random(unchecked((int)seed));
        var homePlaybook = homeTeam.Playbook.ToDictionary(p => p.Id);
        var awayPlaybook = awayTeam.Playbook.ToDictionary(p => p.Id);

        var state = new GameState { PossessionTeamId = homeTeam.Id, DefendingTeamId = awayTeam.Id };
        var match = new Match { HomeTeamId = homeTeam.Id, AwayTeamId = awayTeam.Id, Week = week, Seed = seed };
        var playIndex = 0;

        while (!state.IsGameOver)
        {
            var offenseIsHome = state.PossessionTeamId == homeTeam.Id;
            var (offenseTeam, offenseCallSheet, offensePlaybook) =
                offenseIsHome ? (homeTeam, homeCallSheet, homePlaybook) : (awayTeam, awayCallSheet, awayPlaybook);
            var (defenseTeam, defenseCallSheet, defensePlaybook) =
                offenseIsHome ? (awayTeam, awayCallSheet, awayPlaybook) : (homeTeam, homeCallSheet, homePlaybook);

            var bucket = SituationClassifier.Classify(state);
            var offensePlayId = offenseCallSheet.SelectPlay(PlayCategory.Offense, bucket);
            var defensePlayId = defenseCallSheet.SelectPlay(PlayCategory.Defense, bucket);

            if (offensePlayId is not { } opId || !offensePlaybook.TryGetValue(opId, out var offensePlay) ||
                defensePlayId is not { } dpId || !defensePlaybook.TryGetValue(dpId, out var defensePlay))
            {
                // Nothing legal installed for this situation - punt the possession away rather
                // than stall the game (an empty call sheet is a coaching failure, not a crash).
                state.FlipPossession(100 - state.FieldPosition);
                state.AdvanceClock();
                continue;
            }

            var outcome = PlayResolver.Resolve(offensePlay, offenseTeam, defensePlay, defenseTeam, players, rng);
            playIndex = RecordPlay(match, state, offensePlay, defensePlay, outcome, homeTeam.Id, playIndex);
            state.AdvanceClock();
        }

        match.HomeScore = state.HomeScore;
        match.AwayScore = state.AwayScore;
        match.IsResolved = true;
        return match;
    }

    private static int RecordPlay(
        Match match, GameState state, Play offensePlay, Play defensePlay, PlayOutcome outcome,
        Guid homeTeamId, int playIndex)
    {
        // Captured before state mutates below, so the log reflects the situation the coaches
        // were actually calling plays into - the pre-snap read, not the post-play result.
        var quarter = state.Quarter;
        var down = state.Down;
        var distanceToGo = state.DistanceToGo;
        var fieldPosition = state.FieldPosition;
        var possessionTeamId = state.PossessionTeamId;

        PlayResult BuildResult(bool isTurnover, bool isScore) => new(
            playIndex,
            offensePlay.Id,
            defensePlay.Id,
            offensePlay.IsPassPlay,
            outcome.OffensePlayerId,
            outcome.DefensePlayerId,
            outcome.Yards,
            isTurnover,
            isScore,
            outcome.InjuryEvents,
            quarter,
            down,
            distanceToGo,
            fieldPosition,
            possessionTeamId,
            state.HomeScore,
            state.AwayScore);

        if (outcome.IsTurnover)
        {
            var turnoverSpot = Math.Clamp(state.FieldPosition + outcome.Yards, 1, 99);
            match.EventLog.Add(BuildResult(isTurnover: true, isScore: false));
            state.FlipPossession(100 - turnoverSpot);
            return playIndex + 1;
        }

        var newFieldPosition = state.FieldPosition + outcome.Yards;
        if (newFieldPosition >= 100)
        {
            if (state.PossessionTeamId == homeTeamId) state.HomeScore += 7; else state.AwayScore += 7;
            match.EventLog.Add(BuildResult(isTurnover: false, isScore: true));
            state.FlipPossession(25);
            return playIndex + 1;
        }

        match.EventLog.Add(BuildResult(isTurnover: false, isScore: false));

        // A stuffed run/sack can push newFieldPosition below the offense's own goal line - no
        // safety mechanic exists yet, so the drive is simply stopped at the 1 rather than let
        // FieldPosition go negative and corrupt every play recorded after it.
        newFieldPosition = Math.Clamp(newFieldPosition, 1, 99);

        if (outcome.Yards >= state.DistanceToGo)
        {
            state.Down = 1;
            state.DistanceToGo = 10;
            state.FieldPosition = newFieldPosition;
            return playIndex + 1;
        }

        state.FieldPosition = newFieldPosition;
        state.DistanceToGo -= outcome.Yards;
        state.Down++;

        if (state.Down > 4)
        {
            state.FlipPossession(100 - state.FieldPosition);
        }

        return playIndex + 1;
    }
}
