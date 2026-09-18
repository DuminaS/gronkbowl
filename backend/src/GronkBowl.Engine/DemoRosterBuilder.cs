using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// Builds a full, playable roster for one team - used to seed a league before any real draft
/// or free-agency history exists (a first-run demo league, or a fresh test fixture).
/// </summary>
public static class DemoRosterBuilder
{
    private static readonly FootballPosition[] Positions = Enum.GetValues<FootballPosition>();
    private const int DepthPerPosition = 3;
    private const int StartingGold = 2000;
    private const int StartingCapSpace = 5000;

    public static Team BuildTeam(string name, Race race, Dictionary<Guid, Player> playerPool, Random rng)
    {
        var roster = new Roster();

        foreach (var position in Positions)
        {
            var ids = new List<Guid>();
            for (var depth = 0; depth < DepthPerPosition; depth++)
            {
                var player = new Player
                {
                    Name = $"{race} {position} {depth + 1}",
                    Race = race,
                    Position = position,
                    Attributes = DraftProspectGenerator.RollAttributes(race, rng),
                    ArmorValue = 9 + rng.Next(0, 3),
                };

                playerPool[player.Id] = player;
                ids.Add(player.Id);
            }

            roster.SetDepthOrder(position, ids);
        }

        return new Team
        {
            Name = name,
            Roster = roster,
            Playbook = DefaultPlaybook.Offense.Concat(DefaultPlaybook.Defense).ToList(),
            Gold = StartingGold,
            CapSpace = StartingCapSpace,
        };
    }
}
