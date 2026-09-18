using GronkBowl.Domain.Enums;

namespace GronkBowl.Domain;

public class Player
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required Race Race { get; init; }
    public required FootballPosition Position { get; set; }
    public required Attributes Attributes { get; set; }
    public required int ArmorValue { get; set; }
    public int Grit { get; set; }
    public List<PersonalTrait> Traits { get; init; } = new();

    public InjuryStatus InjuryStatus { get; set; } = InjuryStatus.Healthy;
    public int InjuryWeeksRemaining { get; set; }
    public int PermanentAttributePenalty { get; set; }

    public Contract? Contract { get; set; }
    public int ChemistryModifier { get; set; }

    // BangedUp always clears to Healthy when the Engine finalizes a match, so it never
    // persists into the next week's lineup decision.
    public bool IsAvailableForLineup =>
        InjuryStatus is InjuryStatus.Healthy or InjuryStatus.Niggling;

    // Blood Bowl-style leveling (Master Design Doc: progression system).
    public int Level { get; set; } = 1;
    public int SkillPoints { get; set; }
    public List<Skill> Skills { get; init; } = new();
    public bool UsedCareerReroll { get; set; }

    // HC09-style hidden potential. A rookie's true tier exists from day one but isn't shown to
    // the coach until DevelopmentPotentialRevealed flips - see PlayerProgression.
    public required DevelopmentPotential DevelopmentPotential { get; init; }
    public bool DevelopmentPotentialRevealed { get; set; }
    public int GamesPlayed { get; set; }
}
