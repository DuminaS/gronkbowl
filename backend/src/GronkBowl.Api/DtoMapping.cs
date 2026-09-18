using GronkBowl.Domain;

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
        player.Traits.Select(t => t.ToString()).ToList());

    public static PlayDto ToDto(Play play) => new(
        play.Id,
        play.Name,
        play.Category.ToString(),
        play.IsDefaultPlay,
        play.IsPassPlay,
        play.PrimaryPosition.ToString(),
        play.Assignments.Select(a => new PlayAssignmentDto(a.Slot.ToString(), a.Role.ToString())).ToList());
}
