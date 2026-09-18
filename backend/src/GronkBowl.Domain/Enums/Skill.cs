namespace GronkBowl.Domain.Enums;

public enum Skill
{
    // General - every position can take these as a primary skill.
    SureHands,
    Block,
    Tackle,
    DirtyPlayer,

    // Agility
    Dodge,
    Catch,
    DivingTackle,

    // Strength
    MightyBlow,
    Guard,
    StandFirm,

    // Passing
    Accurate,
    StrongArm,
    SafePass,

    // Mutation - Skitterkin only, doubles-only. See SkillAccess.
    ExtraArms,
    BigHand,
}
