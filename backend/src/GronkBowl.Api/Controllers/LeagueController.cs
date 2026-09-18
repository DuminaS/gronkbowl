using GronkBowl.Domain;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

[ApiController]
[Route("api/league")]
public class LeagueController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public LeagueController(GronkBowlDbContext db) => _db = db;

    [HttpGet("standings")]
    public async Task<ActionResult<List<StandingsRowDto>>> Standings()
    {
        var season = await _db.Seasons.FirstOrDefaultAsync();
        var teams = await _db.Teams.ToListAsync();

        return teams
            .Select(team =>
            {
                var record = season is not null && season.Standings.TryGetValue(team.Id, out var r) ? r : new TeamRecord(0, 0, 0);
                return new StandingsRowDto(team.Id, team.Name, record.Wins, record.Losses, record.Ties);
            })
            .OrderByDescending(row => row.Wins)
            .ThenBy(row => row.Losses)
            .ToList();
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<List<ScheduledGameDto>>> Schedule()
    {
        var season = await _db.Seasons.FirstOrDefaultAsync();
        if (season is null)
        {
            return new List<ScheduledGameDto>();
        }

        var teams = await _db.Teams.ToDictionaryAsync(t => t.Id);
        var playedSet = await PlayedGamesSet(season.Id);

        return season.Schedule
            .Select(game => new ScheduledGameDto(
                game.Week,
                game.HomeTeamId,
                teams[game.HomeTeamId].Name,
                game.AwayTeamId,
                teams[game.AwayTeamId].Name,
                playedSet.Contains((game.Week, game.HomeTeamId, game.AwayTeamId))))
            .ToList();
    }

    /// <summary>
    /// Whether the regular season is done and the Draft/Free Agency are open - the Draft and
    /// Free Agency controllers re-check this themselves before acting (this endpoint just lets
    /// the frontend show the right thing instead of a coach hitting a 409 blind).
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<SeasonStatusDto>> Status()
    {
        var season = await _db.Seasons.FirstOrDefaultAsync();
        if (season is null)
        {
            return new SeasonStatusDto(0, 0, null, false);
        }

        var playedSet = await PlayedGamesSet(season.Id);
        var nextUnplayedWeek = SeasonPhaseCalculator.NextUnplayedWeek(season, playedSet);
        var isComplete = SeasonPhaseCalculator.IsRegularSeasonComplete(season, playedSet.Count);

        return new SeasonStatusDto(season.Schedule.Count, playedSet.Count, nextUnplayedWeek, isComplete);
    }

    private async Task<HashSet<(int Week, Guid HomeTeamId, Guid AwayTeamId)>> PlayedGamesSet(Guid seasonId)
    {
        var playedGames = await _db.Matches
            .Where(m => m.SeasonId == seasonId)
            .Select(m => new { m.Week, m.HomeTeamId, m.AwayTeamId })
            .ToListAsync();
        return playedGames.Select(m => (m.Week, m.HomeTeamId, m.AwayTeamId)).ToHashSet();
    }

    /// <summary>
    /// Runs the weekly submission flow for real: any team that hasn't submitted a call sheet
    /// for this week gets the random missed-deadline fallback, every scheduled game resolves
    /// deterministically, and standings + player injury state persist to the database.
    /// </summary>
    [HttpPost("weeks/{week:int}/resolve")]
    public async Task<ActionResult<List<MatchResultDto>>> ResolveWeek(int week)
    {
        var season = await _db.Seasons.FirstOrDefaultAsync();
        if (season is null)
        {
            return NotFound("No season exists yet.");
        }

        var teams = await _db.Teams.Include(t => t.Playbook).ToDictionaryAsync(t => t.Id);
        var players = await _db.Players.ToDictionaryAsync(p => p.Id);
        var callSheets = await _db.CallSheets.Where(c => c.Week == week).ToDictionaryAsync(c => c.TeamId);

        var results = LeagueOrchestrator.ResolveWeek(season, week, teams, callSheets, players);

        _db.Matches.AddRange(results);
        await _db.SaveChangesAsync();

        return results.Select(match =>
        {
            var homeTeam = teams[match.HomeTeamId];
            var awayTeam = teams[match.AwayTeamId];
            var summary = MatchSummaryBuilder.BuildGameSummary(match, homeTeam, awayTeam, players);
            var gameStats = StatsAggregator.ComputeGameStats(match);
            var boxScore = MatchSummaryBuilder.BuildBoxScore(gameStats, players);

            return new MatchResultDto(match.Id, match.Week, match.HomeTeamId, match.AwayTeamId, match.HomeScore, match.AwayScore, summary, boxScore);
        }).ToList();
    }

    /// <summary>
    /// The structured, play-by-play view of an already-resolved match - situational state
    /// (down/distance/field position/quarter/score) per play, for a client to step through or
    /// animate, rather than the flattened text summary/box score from ResolveWeek.
    /// </summary>
    [HttpGet("matches/{matchId:guid}")]
    public async Task<ActionResult<MatchDetailDto>> GetMatch(Guid matchId)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        if (match is null)
        {
            return NotFound();
        }

        var teams = await _db.Teams.Include(t => t.Playbook)
            .Where(t => t.Id == match.HomeTeamId || t.Id == match.AwayTeamId)
            .ToDictionaryAsync(t => t.Id);
        var players = await _db.Players.ToDictionaryAsync(p => p.Id);

        return DtoMapping.ToDetailDto(match, teams[match.HomeTeamId], teams[match.AwayTeamId], players);
    }
}
