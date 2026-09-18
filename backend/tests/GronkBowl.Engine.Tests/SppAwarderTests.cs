using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class SppAwarderTests
{
    [Fact]
    public void AwardSppForGame_CreditsTouchdownsReceptionsAndTurnoversForced()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);

        var scorer = players.Values.First(p => p.Position == FootballPosition.WR && p.Name.StartsWith("Home"));
        var defender = players.Values.First(p => p.Position == FootballPosition.CB && p.Name.StartsWith("Away"));

        var gameStats = new Dictionary<Guid, PlayerGameStats>
        {
            [scorer.Id] = PlayerGameStats.Empty(scorer.Id) with { Receptions = 3, ReceivingTouchdowns = 1 },
            [defender.Id] = PlayerGameStats.Empty(defender.Id) with { TurnoversForced = 1 },
        };

        SppAwarder.AwardSppForGame(gameStats, home, away, players, new Random(1));

        // 3 (touchdown) + 3 (three receptions x 1) = 6, plus whatever the random MVP bonus adds.
        Assert.True(scorer.SkillPoints >= 6);
        Assert.True(defender.SkillPoints >= 2);
        Assert.Equal(1, scorer.GamesPlayed);
        Assert.Equal(1, defender.GamesPlayed);
    }

    [Fact]
    public void AwardSppForGame_AwardsExactlyOneMvpBonusPerTeam()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);

        var homePlayer = players.Values.First(p => p.Name.StartsWith("Home"));
        var awayPlayer = players.Values.First(p => p.Name.StartsWith("Away"));

        var gameStats = new Dictionary<Guid, PlayerGameStats>
        {
            [homePlayer.Id] = PlayerGameStats.Empty(homePlayer.Id),
            [awayPlayer.Id] = PlayerGameStats.Empty(awayPlayer.Id),
        };

        SppAwarder.AwardSppForGame(gameStats, home, away, players, new Random(1));

        // With zero stat-based SPP and only one candidate per team, the MVP bonus (4) is the
        // only possible source - both should get exactly it.
        Assert.Equal(4, homePlayer.SkillPoints);
        Assert.Equal(4, awayPlayer.SkillPoints);
    }

    [Fact]
    public void AwardSppForGame_NeverAwardsSppToAPlayerWhoDidNotAppearInTheGame()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);

        var participant = players.Values.First(p => p.Name.StartsWith("Home"));
        var bystander = players.Values.First(p => p.Id != participant.Id);

        var gameStats = new Dictionary<Guid, PlayerGameStats>
        {
            [participant.Id] = PlayerGameStats.Empty(participant.Id) with { Receptions = 1 },
        };

        SppAwarder.AwardSppForGame(gameStats, home, away, players, new Random(1));

        Assert.Equal(0, bystander.GamesPlayed);
        Assert.Equal(0, bystander.SkillPoints);
    }
}
