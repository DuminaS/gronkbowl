using GronkBowl.Domain;
using GronkBowl.Engine;

namespace GronkBowl.Api;

public static class DtoMapping
{
    public static MatchDetailDto ToDetailDto(
        Match match, Team homeTeam, Team awayTeam, IReadOnlyDictionary<Guid, Player> players)
    {
        var playNames = homeTeam.Playbook.Concat(awayTeam.Playbook)
            .GroupBy(p => p.Id)
            .ToDictionary(g => g.Key, g => g.First().Name);

        string NameOf(Guid playId) => playNames.TryGetValue(playId, out var name) ? name : "Unknown Play";
        string PlayerNameOf(Guid playerId) => players.TryGetValue(playerId, out var player) ? player.Name : "Unknown";

        var plays = match.EventLog.Select(play => new PlayResultDto(
            play.PlayIndex,
            NameOf(play.OffensePlayId),
            NameOf(play.DefensePlayId),
            play.IsPassPlay,
            play.YardsGained,
            play.IsTurnover,
            play.IsScore,
            play.InjuryEvents.Select(e => $"{PlayerNameOf(e.PlayerId)} ({e.Result.Status})").ToList(),
            play.Quarter,
            play.Down,
            play.DistanceToGo,
            play.FieldPosition,
            play.PossessionTeamId,
            play.HomeScoreAfter,
            play.AwayScoreAfter)).ToList();

        var gameStats = StatsAggregator.ComputeGameStats(match);
        var boxScore = gameStats.Values
            .OrderByDescending(s => s.RushingYards + s.ReceivingYards + s.TacklesOrHits * 5)
            .Where(s => players.ContainsKey(s.PlayerId))
            .Select(s =>
            {
                var player = players[s.PlayerId];
                return new BoxScoreRowDto(
                    s.PlayerId, player.Name, player.Race.ToString(), player.Position.ToString(),
                    s.RushingAttempts, s.RushingYards, s.RushingTouchdowns,
                    s.Receptions, s.ReceivingYards, s.ReceivingTouchdowns,
                    s.TacklesOrHits, s.TurnoversForced, s.InjuriesCaused, s.TimesInjured);
            })
            .ToList();

        return new MatchDetailDto(
            match.Id, match.Week,
            homeTeam.Id, homeTeam.Name, awayTeam.Id, awayTeam.Name,
            match.HomeScore, match.AwayScore, plays, boxScore);
    }

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
