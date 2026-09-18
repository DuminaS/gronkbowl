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
        var playedWeeks = await _db.Matches
            .Where(m => m.SeasonId == season.Id)
            .Select(m => new { m.Week, m.HomeTeamId, m.AwayTeamId })
            .ToListAsync();
        var playedSet = playedWeeks.Select(m => (m.Week, m.HomeTeamId, m.AwayTeamId)).ToHashSet();

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
}
