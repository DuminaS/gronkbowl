using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;

namespace GronkBowl.Api;

/// <summary>Seeds a fresh database with a 4-team demo league on first run only - once any
/// team exists, this is a no-op, so it's safe to call on every startup.</summary>
public static class DbSeeder
{
    public static void SeedIfEmpty(GronkBowlDbContext db)
    {
        if (db.Teams.Any())
        {
            return;
        }

        var players = new Dictionary<Guid, Player>();
        var rng = new Random(2026);

        var teams = new[]
        {
            DemoRosterBuilder.BuildTeam("Ironclad Reapers", Race.Ironkin, players, rng),
            DemoRosterBuilder.BuildTeam("Skysprint Talons", Race.Aelari, players, rng),
            DemoRosterBuilder.BuildTeam("Warhide Crushers", Race.Thornhide, players, rng),
            DemoRosterBuilder.BuildTeam("Shiverfang Swarm", Race.Skitterkin, players, rng),
        };

        // Default plays are shared objects (same ids) across every team's Playbook - add each
        // distinct Play once so EF doesn't try to insert the same primary key twice.
        var distinctPlays = teams.SelectMany(t => t.Playbook).DistinctBy(p => p.Id).ToList();
        db.Plays.AddRange(distinctPlays);
        db.Players.AddRange(players.Values);
        db.Teams.AddRange(teams);

        var season = new Season { Year = 2026, RulesVersion = "v1" };
        season.Schedule.AddRange(ScheduleGenerator.GenerateRoundRobin(teams.Select(t => t.Id).ToList()));
        db.Seasons.Add(season);

        // A starting free-agent market - unrostered players nobody has to draft to sign.
        var freeAgents = DraftProspectGenerator.GenerateClass(12, rng);
        db.Players.AddRange(freeAgents);

        db.SaveChanges();
    }
}
