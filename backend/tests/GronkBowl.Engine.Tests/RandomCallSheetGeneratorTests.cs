using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

public class RandomCallSheetGeneratorTests
{
    [Fact]
    public void Generate_OnlyDrawsFromTheTeamsOwnInstalledPlays()
    {
        var players = new Dictionary<Guid, Player>();
        var team = TestTeamFactory.BuildTeam("Slackers", Race.Ironkin, players);

        var sheet = RandomCallSheetGenerator.Generate(team, week: 3, new Random(1));

        var installedOffenseIds = team.Playbook.Where(p => p.Category == PlayCategory.Offense).Select(p => p.Id).ToHashSet();
        var installedDefenseIds = team.Playbook.Where(p => p.Category == PlayCategory.Defense).Select(p => p.Id).ToHashSet();

        foreach (var playId in sheet.OffensiveSituationalPlays.Values.SelectMany(list => list))
        {
            Assert.Contains(playId, installedOffenseIds);
        }

        foreach (var playId in sheet.DefensiveSituationalPlays.Values.SelectMany(list => list))
        {
            Assert.Contains(playId, installedDefenseIds);
        }
    }

    [Fact]
    public void Generate_MarksTheSheetAsRandomlyGeneratedAndSubmitted()
    {
        var players = new Dictionary<Guid, Player>();
        var team = TestTeamFactory.BuildTeam("Slackers", Race.Ironkin, players);

        var sheet = RandomCallSheetGenerator.Generate(team, week: 3, new Random(1));

        Assert.True(sheet.WasRandomlyGenerated);
        Assert.True(sheet.Submitted);
    }

    [Fact]
    public void Generate_SameRngState_ProducesTheSameSheet()
    {
        var players = new Dictionary<Guid, Player>();
        var team = TestTeamFactory.BuildTeam("Slackers", Race.Ironkin, players);

        var first = RandomCallSheetGenerator.Generate(team, week: 3, new Random(42));
        var second = RandomCallSheetGenerator.Generate(team, week: 3, new Random(42));

        Assert.Equal(
            first.OffensiveSituationalPlays[SituationalBucket.Standard],
            second.OffensiveSituationalPlays[SituationalBucket.Standard]);
    }
}
