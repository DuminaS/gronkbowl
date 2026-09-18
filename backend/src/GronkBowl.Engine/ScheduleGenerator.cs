using GronkBowl.Domain;

namespace GronkBowl.Engine;

/// <summary>
/// Standard circle-method round robin: every team plays every other team exactly once, one or
/// more games per week. Handles an odd team count with a bye (one team sits out that week)
/// rather than refusing to schedule - the v1 MVP target is 2-4 teams, and 3 is a real case.
/// </summary>
public static class ScheduleGenerator
{
    public static List<ScheduledGame> GenerateRoundRobin(IReadOnlyList<Guid> teamIds)
    {
        if (teamIds.Count < 2)
        {
            return new List<ScheduledGame>();
        }

        var rotation = teamIds.Select(id => (Guid?)id).ToList();
        if (rotation.Count % 2 != 0)
        {
            rotation.Add(null); // bye slot
        }

        var teamCount = rotation.Count;
        var rounds = teamCount - 1;
        var games = new List<ScheduledGame>();

        for (var week = 1; week <= rounds; week++)
        {
            for (var i = 0; i < teamCount / 2; i++)
            {
                var home = rotation[i];
                var away = rotation[teamCount - 1 - i];
                if (home is { } h && away is { } a)
                {
                    games.Add(new ScheduledGame(week, h, a));
                }
            }

            rotation = RotateKeepingFirstFixed(rotation);
        }

        return games;
    }

    private static List<Guid?> RotateKeepingFirstFixed(List<Guid?> rotation)
    {
        var fixedTeam = rotation[0];
        var rest = rotation.Skip(1).ToList();
        rest.Insert(0, rest[^1]);
        rest.RemoveAt(rest.Count - 1);
        return new List<Guid?> { fixedTeam }.Concat(rest).ToList();
    }
}
