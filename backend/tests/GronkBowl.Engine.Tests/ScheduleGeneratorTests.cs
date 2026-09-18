using Xunit;

namespace GronkBowl.Engine.Tests;

public class ScheduleGeneratorTests
{
    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void EveryTeam_PlaysEveryOtherTeam_ExactlyOnce(int teamCount)
    {
        var teamIds = Enumerable.Range(0, teamCount).Select(_ => Guid.NewGuid()).ToList();

        var schedule = ScheduleGenerator.GenerateRoundRobin(teamIds);

        var expectedMatchups = teamCount * (teamCount - 1) / 2;
        Assert.Equal(expectedMatchups, schedule.Count);

        foreach (var teamId in teamIds)
        {
            var opponents = schedule
                .Where(g => g.HomeTeamId == teamId || g.AwayTeamId == teamId)
                .Select(g => g.HomeTeamId == teamId ? g.AwayTeamId : g.HomeTeamId)
                .ToList();

            Assert.Equal(teamCount - 1, opponents.Count);
            Assert.Equal(opponents.Distinct().Count(), opponents.Count);
        }
    }

    [Fact]
    public void OddTeamCount_NeverSchedulesATeamTwiceInTheSameWeek()
    {
        var teamIds = Enumerable.Range(0, 5).Select(_ => Guid.NewGuid()).ToList();
        var schedule = ScheduleGenerator.GenerateRoundRobin(teamIds);

        foreach (var weekGroup in schedule.GroupBy(g => g.Week))
        {
            var teamsThisWeek = weekGroup.SelectMany(g => new[] { g.HomeTeamId, g.AwayTeamId }).ToList();
            Assert.Equal(teamsThisWeek.Distinct().Count(), teamsThisWeek.Count);
        }
    }

    [Fact]
    public void FewerThanTwoTeams_ProducesNoGames()
    {
        Assert.Empty(ScheduleGenerator.GenerateRoundRobin(new[] { Guid.NewGuid() }));
        Assert.Empty(ScheduleGenerator.GenerateRoundRobin(Array.Empty<Guid>()));
    }
}
