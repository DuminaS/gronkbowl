using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine.Tests;

internal static class TestTeamFactory
{
    private static readonly FootballPosition[] AllPositions =
    {
        FootballPosition.QB, FootballPosition.RB, FootballPosition.WR, FootballPosition.TE,
        FootballPosition.OL, FootballPosition.DL, FootballPosition.LB, FootballPosition.CB, FootballPosition.S,
    };

    /// <summary>
    /// Rolls realistic attribute variance (the same 40-75%-of-ceiling distribution
    /// DraftProspectGenerator uses for anyone else) instead of giving every player identical
    /// maximum-ceiling stats. That uniform-max version was a real bug in the balance-testing
    /// methodology: it made an original roster categorically stronger than any rookie-tier
    /// emergency replacement signed mid-season, which could snowball into a death spiral that
    /// had nothing to do with the race matchup actually being measured. `rng` defaults to a
    /// fixed seed so existing call sites stay fully deterministic unless a caller wants
    /// per-trial variance (season/balance tests looping over many seeds should pass their own).
    /// </summary>
    public static Team BuildTeam(string name, Race race, Dictionary<Guid, Player> sharedPlayers, Random? rng = null)
    {
        rng ??= new Random(42);
        var roster = new Roster();

        foreach (var position in AllPositions)
        {
            var ids = new List<Guid>();
            for (var depth = 0; depth < 3; depth++)
            {
                var player = new Player
                {
                    Name = $"{name} {position} {depth + 1}",
                    Race = race,
                    Position = position,
                    Attributes = DraftProspectGenerator.RollAttributes(race, rng),
                    ArmorValue = 9 + rng.Next(0, 3),
                    DevelopmentPotential = DevelopmentProfiles.RollRandom(rng),
                    DevelopmentPotentialRevealed = true,
                };
                sharedPlayers[player.Id] = player;
                ids.Add(player.Id);
            }

            roster.SetDepthOrder(position, ids);
        }

        return new Team
        {
            Name = name,
            Roster = roster,
            Playbook = DefaultPlaybook.Offense.Concat(DefaultPlaybook.Defense).ToList(),
        };
    }

    public static CallSheet BuildCallSheetRunningEverythingThrough(Team team, Play offensePlay, Play defensePlay)
    {
        var callSheet = new CallSheet { TeamId = team.Id, Week = 1 };

        foreach (SituationalBucket bucket in Enum.GetValues<SituationalBucket>())
        {
            callSheet.OffensiveSituationalPlays[bucket] = new List<Guid> { offensePlay.Id };
            callSheet.DefensiveSituationalPlays[bucket] = new List<Guid> { defensePlay.Id };
        }

        return callSheet;
    }

    /// <summary>
    /// A call sheet that actually varies by situation, the way a real coach would build one -
    /// a fixed "always call the same play" sheet degenerates badly the moment its one featured
    /// position group gets hurt, which is a roster-building failure, not something a test
    /// meant to exercise the engine's full behavior should run into by accident.
    /// </summary>
    public static CallSheet BuildRealisticCallSheet(Team team)
    {
        var offense = DefaultPlaybook.Offense.ToDictionary(p => p.Name);
        var defense = DefaultPlaybook.Defense.ToDictionary(p => p.Name);
        var callSheet = new CallSheet { TeamId = team.Id, Week = 1 };

        var offensePriorities = new Dictionary<SituationalBucket, string>
        {
            [SituationalBucket.Standard] = "Inside Dive",
            [SituationalBucket.SecondAndShort] = "Outside Zone",
            [SituationalBucket.SecondAndLong] = "Slant-Flat",
            [SituationalBucket.ThirdAndShort] = "Inside Dive",
            [SituationalBucket.ThirdAndLong] = "Four Verticals",
            [SituationalBucket.RedZone] = "Screen Pass",
            [SituationalBucket.GoalLine] = "Inside Dive",
            [SituationalBucket.TwoMinuteDrill] = "Four Verticals",
            [SituationalBucket.BackedUp] = "Outside Zone",
        };

        var defensePriorities = new Dictionary<SituationalBucket, string>
        {
            [SituationalBucket.Standard] = "Base Front",
            [SituationalBucket.SecondAndShort] = "Base Front",
            [SituationalBucket.SecondAndLong] = "Cover 3",
            [SituationalBucket.ThirdAndShort] = "Blitz Package",
            [SituationalBucket.ThirdAndLong] = "Cover 1",
            [SituationalBucket.RedZone] = "Blitz Package",
            [SituationalBucket.GoalLine] = "Base Front",
            [SituationalBucket.TwoMinuteDrill] = "Prevent",
            [SituationalBucket.BackedUp] = "Base Front",
        };

        foreach (SituationalBucket bucket in Enum.GetValues<SituationalBucket>())
        {
            var primaryOffense = offense[offensePriorities[bucket]];
            var primaryDefense = defense[defensePriorities[bucket]];

            callSheet.OffensiveSituationalPlays[bucket] =
                new[] { primaryOffense.Id }.Concat(offense.Values.Where(p => p.Id != primaryOffense.Id).Select(p => p.Id)).ToList();
            callSheet.DefensiveSituationalPlays[bucket] =
                new[] { primaryDefense.Id }.Concat(defense.Values.Where(p => p.Id != primaryDefense.Id).Select(p => p.Id)).ToList();
        }

        return callSheet;
    }
}
