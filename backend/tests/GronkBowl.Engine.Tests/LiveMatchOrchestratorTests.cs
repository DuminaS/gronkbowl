using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class LiveMatchOrchestratorTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players) BuildMatchup()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);
        return (home, away, players);
    }

    private static Guid OffensePlayId(Team team) => team.Playbook.First(p => p.Category == PlayCategory.Offense).Id;
    private static Guid DefensePlayId(Team team) => team.Playbook.First(p => p.Category == PlayCategory.Defense).Id;

    [Fact]
    public void StartMatch_BeginsAtKickoff()
    {
        var (home, away, _) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        Assert.Equal(1, match.Quarter);
        Assert.Equal(1, match.Down);
        Assert.Equal(10, match.DistanceToGo);
        Assert.Equal(25, match.FieldPosition);
        Assert.Equal(home.Id, match.PossessionTeamId);
        Assert.Null(match.PendingOffensePlayId);
        Assert.Null(match.PendingDefensePlayId);
        Assert.False(match.IsResolved);
        Assert.Empty(match.EventLog);
    }

    [Fact]
    public void SubmitPlay_OneSideOnly_DoesNotResolveTheDown()
    {
        var (home, away, players) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        var outcome = LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, OffensePlayId(home));

        Assert.Equal(SubmitPlayOutcome.Accepted, outcome);
        Assert.NotNull(match.PendingOffensePlayId);
        Assert.Null(LiveMatchOrchestrator.TryResolveCurrentDown(match, home, away, players, new Random(1)));
        Assert.Empty(match.EventLog);
    }

    [Fact]
    public void SubmitPlay_SameSideTwice_IsRefused()
    {
        var (home, away, _) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, OffensePlayId(home));
        var second = LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, OffensePlayId(home));

        Assert.Equal(SubmitPlayOutcome.AlreadySubmitted, second);
    }

    [Fact]
    public void SubmitPlay_WrongCategoryForYourSide_IsRefused()
    {
        var (home, away, _) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        // Home has the ball (offense) - offering a defensive play for that side is illegal.
        var outcome = LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, DefensePlayId(home));

        Assert.Equal(SubmitPlayOutcome.IllegalPlay, outcome);
    }

    [Fact]
    public void SubmitPlay_TeamNotInThisMatch_IsRefused()
    {
        var (home, away, _) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        var outcome = LiveMatchOrchestrator.SubmitPlay(match, home, away, Guid.NewGuid(), OffensePlayId(home));

        Assert.Equal(SubmitPlayOutcome.TeamNotInMatch, outcome);
    }

    [Fact]
    public void BothSidesSubmit_ResolvesExactlyOneDownAndClearsPending()
    {
        var (home, away, players) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, OffensePlayId(home));
        LiveMatchOrchestrator.SubmitPlay(match, home, away, away.Id, DefensePlayId(away));
        var result = LiveMatchOrchestrator.TryResolveCurrentDown(match, home, away, players, new Random(1));

        Assert.NotNull(result);
        Assert.Single(match.EventLog);
        Assert.Equal(0, result!.PlayIndex);
        Assert.Equal(1, result.Down);
        Assert.Equal(10, result.DistanceToGo);
        Assert.Equal(25, result.FieldPosition);
        Assert.Equal(home.Id, result.PossessionTeamId);
        Assert.Null(match.PendingOffensePlayId);
        Assert.Null(match.PendingDefensePlayId);
    }

    [Fact]
    public void ForceResolveCurrentDown_FillsInFromFolders_ForWhicheverSideHasNotSubmitted()
    {
        var (home, away, players) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        // Only the offense (home) has submitted - the defense (away) is missing.
        LiveMatchOrchestrator.SubmitPlay(match, home, away, home.Id, OffensePlayId(home));

        var folders = new List<PlaybookFolder>
        {
            new() { TeamId = away.Id, Category = PlayCategory.Defense, Name = "Base Defense", PlayIds = away.Playbook.Where(p => p.Category == PlayCategory.Defense).Select(p => p.Id).ToList() },
        };

        var result = LiveMatchOrchestrator.ForceResolveCurrentDown(match, home, away, folders, players, new Random(1));

        Assert.NotNull(result);
        Assert.Single(match.EventLog);
    }

    [Fact]
    public void ForceResolveCurrentDown_WithNoFolders_StillFallsBackToTheTeamsPlaybook()
    {
        var (home, away, players) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);

        var result = LiveMatchOrchestrator.ForceResolveCurrentDown(match, home, away, new List<PlaybookFolder>(), players, new Random(1));

        Assert.NotNull(result);
    }

    [Fact]
    public void APlayedOutGame_EventuallyReachesResolved_WithASensibleFinalScore()
    {
        var (home, away, players) = BuildMatchup();
        var match = LiveMatchOrchestrator.StartMatch(home, away, week: 1, seed: 1);
        var rng = new Random(7);

        var safety = 0;
        while (!match.IsResolved && safety++ < 500)
        {
            var offenseTeam = match.PossessionTeamId == home.Id ? home : away;
            var defenseTeam = match.PossessionTeamId == home.Id ? away : home;

            LiveMatchOrchestrator.SubmitPlay(match, home, away, offenseTeam.Id, OffensePlayId(offenseTeam));
            LiveMatchOrchestrator.SubmitPlay(match, home, away, defenseTeam.Id, DefensePlayId(defenseTeam));
            LiveMatchOrchestrator.TryResolveCurrentDown(match, home, away, players, rng);
        }

        Assert.True(match.IsResolved);
        Assert.True(match.HomeScore % 7 == 0);
        Assert.True(match.AwayScore % 7 == 0);
        Assert.NotEmpty(match.EventLog);
    }
}
