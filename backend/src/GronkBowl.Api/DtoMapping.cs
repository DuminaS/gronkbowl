using GronkBowl.Domain;
using GronkBowl.Engine;

namespace GronkBowl.Api;

public static class DtoMapping
{
    public static PlayerDto ToDto(Player player) => new(
        player.Id,
        player.Name,
        player.Race.ToString(),
        player.Position.ToString(),
        player.Attributes.Speed,
        player.Attributes.Strength,
        player.Attributes.Agility,
        player.Attributes.Awareness,
        player.Attributes.Durability,
        player.ArmorValue,
        player.InjuryStatus.ToString(),
        player.InjuryWeeksRemaining,
        player.Traits.Select(t => t.ToString()).ToList(),
        player.Level,
        player.SkillPoints,
        player.Level < PlayerProgression.MaxLevel ? PlayerProgression.SppNeededForLevel(player.Level + 1, player) : null,
        player.Skills.Select(s => s.ToString()).ToList(),
        // Hidden until proven: a rookie's true tier never leaves the server as a string until revealed.
        player.DevelopmentPotentialRevealed ? DevelopmentProfiles.All[player.DevelopmentPotential].Name : null,
        player.DevelopmentPotentialRevealed,
        player.GamesPlayed);

    public static PlayDto ToDto(Play play) => new(
        play.Id,
        play.Name,
        play.Category.ToString(),
        play.IsDefaultPlay,
        play.IsPassPlay,
        play.PrimaryPosition.ToString(),
        play.Assignments.Select(a => new PlayAssignmentDto(a.Slot.ToString(), a.Role.ToString())).ToList());
}
