using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Domain.Tests;

public class RosterTests
{
    private static Player MakePlayer(FootballPosition position, InjuryStatus status = InjuryStatus.Healthy) =>
        new()
        {
            Name = position.ToString(),
            Race = Race.Ironkin,
            Position = position,
            Attributes = RaceProfiles.All[Race.Ironkin].CeilingAttributes,
            ArmorValue = 9,
            InjuryStatus = status,
            DevelopmentPotential = DevelopmentPotential.SteadyEddie,
        };

    [Fact]
    public void GetStarter_ReturnsFirstHealthyPlayerInDepthOrder()
    {
        var starter = MakePlayer(FootballPosition.RB, InjuryStatus.Out);
        var backup = MakePlayer(FootballPosition.RB);
        var players = new Dictionary<Guid, Player> { [starter.Id] = starter, [backup.Id] = backup };

        var roster = new Roster();
        roster.SetDepthOrder(FootballPosition.RB, new[] { starter.Id, backup.Id });

        var result = roster.GetStarter(FootballPosition.RB, players);

        Assert.Equal(backup.Id, result);
    }

    [Fact]
    public void GetStarter_ReturnsNull_WhenEntireDepthIsUnavailable()
    {
        var onlyOption = MakePlayer(FootballPosition.QB, InjuryStatus.SeasonEnding);
        var players = new Dictionary<Guid, Player> { [onlyOption.Id] = onlyOption };

        var roster = new Roster();
        roster.SetDepthOrder(FootballPosition.QB, new[] { onlyOption.Id });

        Assert.Null(roster.GetStarter(FootballPosition.QB, players));
    }

    [Fact]
    public void GetAvailableAtPosition_SkipsInjuredPlayersButKeepsOrder()
    {
        var one = MakePlayer(FootballPosition.WR);
        var two = MakePlayer(FootballPosition.WR, InjuryStatus.Niggling);
        var three = MakePlayer(FootballPosition.WR, InjuryStatus.Out);
        var players = new Dictionary<Guid, Player> { [one.Id] = one, [two.Id] = two, [three.Id] = three };

        var roster = new Roster();
        roster.SetDepthOrder(FootballPosition.WR, new[] { one.Id, two.Id, three.Id });

        var available = roster.GetAvailableAtPosition(FootballPosition.WR, players);

        Assert.Equal(new[] { one.Id, two.Id }, available);
    }
}
