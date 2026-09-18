using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public sealed record SkillDefinition(Skill Skill, SkillCategory Category, string Description, bool IsWiredIntoEngine);

/// <summary>
/// The full v1 skill list. Not every skill has a mechanical hook yet - `IsWiredIntoEngine`
/// tells you which ones `PlayResolver`/`CasualtyResolver` actually read. The rest are real,
/// selectable skills with defined categories, just not yet mechanically hooked up - the same
/// kind of intentional, documented gap as the unwired parts of the Faction Bible.
/// </summary>
public static class SkillDefinitions
{
    public static readonly IReadOnlyDictionary<Skill, SkillDefinition> All = new Dictionary<Skill, SkillDefinition>
    {
        [Skill.SureHands] = new(Skill.SureHands, SkillCategory.General, "Reduces fumble chance on runs.", true),
        [Skill.Block] = new(Skill.Block, SkillCategory.General, "Bonus to this player's line-battle roll.", true),
        [Skill.Tackle] = new(Skill.Tackle, SkillCategory.General, "Reduces the ball carrier's breakaway chance against this defender.", true),
        [Skill.DirtyPlayer] = new(Skill.DirtyPlayer, SkillCategory.General, "Bonus to the injury severity roll when this player lands a hit.", true),

        [Skill.Dodge] = new(Skill.Dodge, SkillCategory.Agility, "Bonus evasion on runs and catches.", true),
        [Skill.Catch] = new(Skill.Catch, SkillCategory.Agility, "Bonus to the receiver's route roll.", true),
        [Skill.DivingTackle] = new(Skill.DivingTackle, SkillCategory.Agility, "Shrinks the ball carrier's Speed edge on a breakaway attempt.", false),

        [Skill.MightyBlow] = new(Skill.MightyBlow, SkillCategory.Strength, "Bonus to the hitter's armor-break roll.", true),
        [Skill.Guard] = new(Skill.Guard, SkillCategory.Strength, "Bonus to this player's line-battle roll.", true),
        [Skill.StandFirm] = new(Skill.StandFirm, SkillCategory.Strength, "Reduces the chance of a negative-yardage run.", false),

        [Skill.Accurate] = new(Skill.Accurate, SkillCategory.Passing, "Reduces pressure's effect on pass accuracy.", false),
        [Skill.StrongArm] = new(Skill.StrongArm, SkillCategory.Passing, "Extends the maximum yardage on a completed pass.", false),
        [Skill.SafePass] = new(Skill.SafePass, SkillCategory.Passing, "Reduces interception chance.", false),

        [Skill.ExtraArms] = new(Skill.ExtraArms, SkillCategory.Mutation, "Bonus to the receiver's route roll, stacking with Catch.", true),
        [Skill.BigHand] = new(Skill.BigHand, SkillCategory.Mutation, "Further reduces fumble chance, stacking with Sure Hands.", true),
    };
}
