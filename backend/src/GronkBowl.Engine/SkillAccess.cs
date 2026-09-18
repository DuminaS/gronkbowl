using GronkBowl.Domain;
using GronkBowl.Domain.Enums;

namespace GronkBowl.Engine;

/// <summary>
/// Which skill categories a player can draw a new skill from - General is universal; one
/// football-appropriate category is primary per position; a second is accessible only when the
/// level-up roll comes up doubles. Mutation is a race-gated exception (Skitterkin only, doubles
/// only), a direct analogue of Blood Bowl's own Skaven/Chaos Dwarf mutation-on-doubles rule.
/// </summary>
public static class SkillAccess
{
    private static readonly Dictionary<FootballPosition, SkillCategory> PrimaryByPosition = new()
    {
        [FootballPosition.QB] = SkillCategory.Passing,
        [FootballPosition.RB] = SkillCategory.Agility,
        [FootballPosition.WR] = SkillCategory.Agility,
        [FootballPosition.TE] = SkillCategory.Strength,
        [FootballPosition.OL] = SkillCategory.Strength,
        [FootballPosition.DL] = SkillCategory.Strength,
        [FootballPosition.LB] = SkillCategory.Strength,
        [FootballPosition.CB] = SkillCategory.Agility,
        [FootballPosition.S] = SkillCategory.Agility,
        [FootballPosition.K] = SkillCategory.Passing,
        [FootballPosition.P] = SkillCategory.Passing,
    };

    private static readonly Dictionary<FootballPosition, SkillCategory> SecondaryByPosition = new()
    {
        [FootballPosition.QB] = SkillCategory.Agility,
        [FootballPosition.RB] = SkillCategory.Strength,
        [FootballPosition.WR] = SkillCategory.Passing,
        [FootballPosition.TE] = SkillCategory.Agility,
        [FootballPosition.OL] = SkillCategory.Agility,
        [FootballPosition.DL] = SkillCategory.Agility,
        [FootballPosition.LB] = SkillCategory.Agility,
        [FootballPosition.CB] = SkillCategory.Strength,
        [FootballPosition.S] = SkillCategory.Strength,
        [FootballPosition.K] = SkillCategory.Agility,
        [FootballPosition.P] = SkillCategory.Agility,
    };

    public static IReadOnlySet<SkillCategory> PrimaryCategories(Player player)
    {
        var categories = new HashSet<SkillCategory> { SkillCategory.General, PrimaryByPosition[player.Position] };
        return categories;
    }

    public static IReadOnlySet<SkillCategory> SecondaryCategories(Player player)
    {
        var categories = new HashSet<SkillCategory> { SecondaryByPosition[player.Position] };

        // Skitterkin's Scurry ability already grants extra evasion (see PlayResolver); on the
        // level-up table specifically, they're also the one race that can reach into Mutation -
        // and only on doubles, exactly like the Skaven/Chaos Dwarf precedent this is modeled on.
        if (player.Race == Race.Skitterkin)
        {
            categories.Add(SkillCategory.Mutation);
        }

        return categories;
    }

    public static IReadOnlyList<Skill> AvailablePrimarySkills(Player player)
    {
        var categories = PrimaryCategories(player);
        return SkillDefinitions.All.Values
            .Where(def => categories.Contains(def.Category) && !player.Skills.Contains(def.Skill))
            .Select(def => def.Skill)
            .ToList();
    }

    public static IReadOnlyList<Skill> AvailableSecondarySkills(Player player)
    {
        var categories = SecondaryCategories(player);
        return SkillDefinitions.All.Values
            .Where(def => categories.Contains(def.Category) && !player.Skills.Contains(def.Skill))
            .Select(def => def.Skill)
            .ToList();
    }
}
