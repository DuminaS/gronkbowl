using GronkBowl.Domain;
using GronkBowl.Domain.Enums;
using Xunit;

namespace GronkBowl.Engine.Tests;

/// <summary>
/// Guards the season-level Durability fix specifically: a higher-Durability race should need
/// fewer emergency roster replacements over a season than a lower-Durability opponent, on
/// average. Not a full balance suite - see README/Master Design Doc for what's still open
/// (Ironkin's power+toughness combination currently compounds rather than trading off, which
/// this test deliberately does not assert against, since fixing that is unresolved work).
/// </summary>
public class SeasonBalanceTests
{
    [Theory]
    [InlineData(Race.Ironkin, Race.Aelari)]
    [InlineData(Race.Ironkin, Race.Skitterkin)]
    [InlineData(Race.Ironkin, Race.Thornhide)]
    public void HigherDurabilityRace_NeedsFewerReplacementsOverASeason_OnAverage(
        Race tougherRace, Race fragileRace)
    {
        // A single 16-week trial is noisy, especially for a small Durability gap (Ironkin vs.
        // Thornhide is only 15 points, versus 45+ against the other two) - average several
        // independent seasons rather than asserting off one roll of the dice.
        var tougherTotal = 0;
        var fragileTotal = 0;
        const int trialSeasons = 5;

        for (var trial = 0; trial < trialSeasons; trial++)
        {
            var players = new Dictionary<Guid, Player>();
            var tougher = TestTeamFactory.BuildTeam("Tougher", tougherRace, players);
            var fragile = TestTeamFactory.BuildTeam("Fragile", fragileRace, players);
            var tougherSheet = TestTeamFactory.BuildRealisticCallSheet(tougher);
            var fragileSheet = TestTeamFactory.BuildRealisticCallSheet(fragile);

            var weeks = SeasonSimulator.SimulateRepeatedMatchup(
                tougher, tougherSheet, fragile, fragileSheet, players, 16, seedBase: 2000 + trial * 100);

            tougherTotal += weeks.Sum(w => w.HomeReplacementsSignedThisWeek);
            fragileTotal += weeks.Sum(w => w.AwayReplacementsSignedThisWeek);
        }

        Assert.True(
            tougherTotal <= fragileTotal,
            $"{tougherRace} (higher Durability) signed {tougherTotal} total replacements across {trialSeasons} seasons vs {fragileRace}'s {fragileTotal} - Durability should protect against needing more, not fewer, on average.");
    }
}
