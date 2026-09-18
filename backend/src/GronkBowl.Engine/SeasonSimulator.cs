using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

public sealed record WeeklySnapshot(
    int Week, int HomeScore, int AwayScore, int HomeUnavailable, int AwayUnavailable,
    int HomeReplacementsSignedThisWeek, int AwayReplacementsSignedThisWeek);

/// <summary>
/// A diagnostic/analysis tool, not a gameplay feature: plays the same two teams against each
/// other week after week, with injuries carrying over between games. Built specifically to
/// answer a question a single isolated game cannot: does a fragile-but-talented race's early
/// edge get eaten by attrition over a season, the way its Durability stat is supposed to cost
/// it? A "do nothing" season isn't a fair test of that - a real coach signs replacements for
/// an empty position - so between weeks this auto-signs a same-race emergency replacement for
/// any position left with zero available players. The number of replacements a race needs
/// over a season *is* the visible cost of fragility; that's the metric this is actually for.
/// </summary>
public static class SeasonSimulator
{
    private static readonly FootballPosition[] AllPositions = Enum.GetValues<FootballPosition>();

    public static List<WeeklySnapshot> SimulateRepeatedMatchup(
        Team home, CallSheet homeSheet,
        Team away, CallSheet awaySheet,
        Dictionary<Guid, Player> players,
        int weeks, long seedBase)
    {
        var snapshots = new List<WeeklySnapshot>(weeks);
        var rng = new Random(unchecked((int)seedBase));

        for (var week = 1; week <= weeks; week++)
        {
            TickInjuryRecovery(players);
            var teamASigned = ReplenishEmptyPositions(home, players, rng);
            var teamBSigned = ReplenishEmptyPositions(away, players, rng);

            // Alternate who actually has first possession each week - always giving the same
            // team "home" for all 16 games would let a first-possession edge compound over the
            // season, confounding it with whatever race difference is being measured.
            var match = week % 2 == 1
                ? MatchEngine.ResolveGame(home, away, homeSheet, awaySheet, players, week, seedBase + week)
                : MatchEngine.ResolveGame(away, home, awaySheet, homeSheet, players, week, seedBase + week);
            var (teamAScore, teamBScore) = week % 2 == 1 ? (match.HomeScore, match.AwayScore) : (match.AwayScore, match.HomeScore);

            snapshots.Add(new WeeklySnapshot(
                week,
                teamAScore,
                teamBScore,
                CountUnavailable(home, players),
                CountUnavailable(away, players),
                teamASigned,
                teamBSigned));
        }

        return snapshots;
    }

    private static int ReplenishEmptyPositions(Team team, Dictionary<Guid, Player> players, Random rng)
    {
        var signed = 0;
        foreach (var position in AllPositions)
        {
            if (team.Roster.GetAvailableAtPosition(position, players).Count > 0)
            {
                continue;
            }

            var race = team.Roster.DepthChart.Values.SelectMany(ids => ids)
                .Select(id => players.TryGetValue(id, out var p) ? p.Race : (Race?)null)
                .FirstOrDefault(r => r is not null) ?? Race.Ironkin;

            var replacement = new Player
            {
                Name = $"{race} {position} (emergency signing)",
                Race = race,
                Position = position,
                Attributes = DraftProspectGenerator.RollAttributes(race, rng),
                ArmorValue = 9 + rng.Next(0, 3),
                DevelopmentPotential = DevelopmentProfiles.RollRandom(rng),
            };

            players[replacement.Id] = replacement;
            team.Roster.AddToDepthChart(position, replacement.Id);
            signed++;
        }

        return signed;
    }

    /// <summary>Missed-time injuries expire week to week; everything else (Niggling and
    /// worse) is meant to persist for the test's purposes.</summary>
    private static void TickInjuryRecovery(IReadOnlyDictionary<Guid, Player> players)
    {
        foreach (var player in players.Values)
        {
            if (player.InjuryStatus == Domain.Enums.InjuryStatus.Out && player.InjuryWeeksRemaining > 0)
            {
                player.InjuryWeeksRemaining--;
                if (player.InjuryWeeksRemaining == 0)
                {
                    player.InjuryStatus = Domain.Enums.InjuryStatus.Healthy;
                }
            }
        }
    }

    private static int CountUnavailable(Team team, IReadOnlyDictionary<Guid, Player> players) =>
        team.Roster.DepthChart.Values
            .SelectMany(ids => ids)
            .Count(id => players.TryGetValue(id, out var player) && !player.IsAvailableForLineup);
}
