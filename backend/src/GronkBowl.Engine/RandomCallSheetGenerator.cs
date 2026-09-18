using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// The missed-deadline penalty from the Master Design Doc: an unweighted random slotting of
/// the team's own already-installed plays into every situational bucket. Never draws from a
/// league-wide default pool - only what this team installed, which always includes the free
/// starter playbook, so there's always something to fall back on, just not a competent plan.
/// </summary>
public static class RandomCallSheetGenerator
{
    public static CallSheet Generate(Team team, int week, Random rng)
    {
        var offensePlays = team.Playbook.Where(p => p.Category == PlayCategory.Offense).Select(p => p.Id).ToList();
        var defensePlays = team.Playbook.Where(p => p.Category == PlayCategory.Defense).Select(p => p.Id).ToList();

        var callSheet = new CallSheet
        {
            TeamId = team.Id,
            Week = week,
            Submitted = true,
            WasRandomlyGenerated = true,
        };

        foreach (SituationalBucket bucket in Enum.GetValues<SituationalBucket>())
        {
            callSheet.OffensiveSituationalPlays[bucket] = Shuffle(offensePlays, rng);
            callSheet.DefensiveSituationalPlays[bucket] = Shuffle(defensePlays, rng);
        }

        return callSheet;
    }

    private static List<Guid> Shuffle(IReadOnlyList<Guid> source, Random rng)
    {
        var shuffled = source.ToList();
        for (var i = shuffled.Count - 1; i > 0; i--)
        {
            var swapIndex = rng.Next(i + 1);
            (shuffled[i], shuffled[swapIndex]) = (shuffled[swapIndex], shuffled[i]);
        }

        return shuffled;
    }
}
