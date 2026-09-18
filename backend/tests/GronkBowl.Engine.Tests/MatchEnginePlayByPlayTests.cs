using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

/// <summary>
/// Guards the situational-state fields PlayResult now carries (Quarter/Down/DistanceToGo/
/// FieldPosition/PossessionTeamId/*ScoreAfter) - these exist so a client can replay a drive
/// without re-simulating it, so an off-by-one in when RecordPlay reads state (before vs. after
/// it mutates) would silently corrupt every replay without breaking score/event-log tests.
/// </summary>
public class MatchEnginePlayByPlayTests
{
    private static (Team Home, Team Away, Dictionary<Guid, Player> Players) BuildMatchup()
    {
        var players = new Dictionary<Guid, Player>();
        var home = TestTeamFactory.BuildTeam("Home", Race.Ironkin, players);
        var away = TestTeamFactory.BuildTeam("Away", Race.Aelari, players);
        return (home, away, players);
    }

    [Fact]
    public void EventLog_FirstPlay_ReflectsKickoffState()
    {
        var (home, away, players) = BuildMatchup();
        var match = MatchEngine.ResolveGame(
            home, away, TestTeamFactory.BuildRealisticCallSheet(home), TestTeamFactory.BuildRealisticCallSheet(away),
            players, week: 1, seed: 7);

        var first = match.EventLog[0];
        Assert.Equal(1, first.Quarter);
        Assert.Equal(1, first.Down);
        Assert.Equal(10, first.DistanceToGo);
        Assert.Equal(25, first.FieldPosition);
        Assert.Equal(home.Id, first.PossessionTeamId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(11)]
    [InlineData(23)]
    [InlineData(37)]
    [InlineData(101)]
    public void EventLog_EveryConsecutivePair_ObeysTheDownAndDistanceStateMachine(int seed)
    {
        var (home, away, players) = BuildMatchup();
        var match = MatchEngine.ResolveGame(
            home, away, TestTeamFactory.BuildRealisticCallSheet(home), TestTeamFactory.BuildRealisticCallSheet(away),
            players, week: 1, seed: seed);

        foreach (var play in match.EventLog)
        {
            Assert.InRange(play.Quarter, 1, 4);
            Assert.InRange(play.Down, 1, 4);
            Assert.InRange(play.FieldPosition, 1, 99);
            Assert.True(play.DistanceToGo >= 1);
        }

        for (var i = 0; i < match.EventLog.Count - 1; i++)
        {
            var prev = match.EventLog[i];
            var next = match.EventLog[i + 1];

            if (prev.IsScore || prev.IsTurnover)
            {
                // Possession always changes hands on a score or a turnover, and the new
                // possessing team starts a fresh first-and-ten.
                Assert.NotEqual(prev.PossessionTeamId, next.PossessionTeamId);
                Assert.Equal(1, next.Down);
                Assert.Equal(10, next.DistanceToGo);
                continue;
            }

            if (prev.YardsGained >= prev.DistanceToGo)
            {
                // Converted - same team, fresh first-and-ten from the spot they reached.
                Assert.Equal(prev.PossessionTeamId, next.PossessionTeamId);
                Assert.Equal(1, next.Down);
                Assert.Equal(10, next.DistanceToGo);
                Assert.Equal(Math.Clamp(prev.FieldPosition + prev.YardsGained, 1, 99), next.FieldPosition);
            }
            else if (prev.Down == 4)
            {
                // Turnover on downs - possession flips, fresh first-and-ten.
                Assert.NotEqual(prev.PossessionTeamId, next.PossessionTeamId);
                Assert.Equal(1, next.Down);
                Assert.Equal(10, next.DistanceToGo);
            }
            else
            {
                // Drive continues - same team, down ticks up, distance/field position track yardage exactly.
                Assert.Equal(prev.PossessionTeamId, next.PossessionTeamId);
                Assert.Equal(prev.Down + 1, next.Down);
                Assert.Equal(prev.DistanceToGo - prev.YardsGained, next.DistanceToGo);
                Assert.Equal(Math.Clamp(prev.FieldPosition + prev.YardsGained, 1, 99), next.FieldPosition);
            }
        }
    }

    [Fact]
    public void EventLog_ScoreAfterFields_OnlyChangeOnTheScoringPlayItself()
    {
        var anyScoreSeenAcrossSeeds = false;

        foreach (var seed in Enumerable.Range(0, 15))
        {
            var (home, away, players) = BuildMatchup();
            var match = MatchEngine.ResolveGame(
                home, away, TestTeamFactory.BuildRealisticCallSheet(home), TestTeamFactory.BuildRealisticCallSheet(away),
                players, week: 1, seed: seed);

            var runningHome = 0;
            var runningAway = 0;

            foreach (var play in match.EventLog)
            {
                if (play.IsScore)
                {
                    anyScoreSeenAcrossSeeds = true;
                    if (play.PossessionTeamId == home.Id) runningHome += 7; else runningAway += 7;
                }

                Assert.Equal(runningHome, play.HomeScoreAfter);
                Assert.Equal(runningAway, play.AwayScoreAfter);
            }

            Assert.Equal(match.HomeScore, runningHome);
            Assert.Equal(match.AwayScore, runningAway);
        }

        Assert.True(anyScoreSeenAcrossSeeds);
    }
}
