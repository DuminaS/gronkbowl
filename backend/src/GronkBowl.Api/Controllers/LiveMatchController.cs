using GronkBowl.Domain;
using GronkBowl.Engine;
using GronkBowl.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GronkBowl.Api.Controllers;

/// <summary>
/// The live, one-down-at-a-time match loop: start a scheduled game, ask what the current down
/// is waiting on, submit a play for it, and (as the manual stand-in for a timeout - see the
/// plan's "explicitly deferred" section) force a down through using each team's own installed
/// plays if a coach hasn't answered. This is the primary way a game gets played now; the older
/// whole-week batch resolve (LeagueController.ResolveWeek) still exists underneath as an
/// instant-simulate utility, not the main loop.
/// </summary>
[ApiController]
[Route("api/league/matches")]
public class LiveMatchController : ControllerBase
{
    private readonly GronkBowlDbContext _db;

    public LiveMatchController(GronkBowlDbContext db) => _db = db;

    [HttpPost("live/start")]
    public async Task<ActionResult<LiveMatchStartedDto>> Start([FromBody] StartLiveMatchRequest request)
    {
        var season = await _db.Seasons.FirstOrDefaultAsync();
        if (season is null)
        {
            return NotFound("No season exists yet.");
        }

        var alreadyPlayed = await _db.Matches.AnyAsync(m =>
            m.SeasonId == season.Id && m.Week == request.Week &&
            m.HomeTeamId == request.HomeTeamId && m.AwayTeamId == request.AwayTeamId);
        if (alreadyPlayed)
        {
            return Conflict("This matchup has already been started or played for this week.");
        }

        var homeTeam = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == request.HomeTeamId);
        var awayTeam = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == request.AwayTeamId);
        if (homeTeam is null || awayTeam is null)
        {
            return NotFound("Team not found.");
        }

        var seed = HashCode.Combine(season.Id, request.Week, homeTeam.Id, awayTeam.Id, "live");
        var match = LiveMatchOrchestrator.StartMatch(homeTeam, awayTeam, request.Week, seed);
        match.SeasonId = season.Id;

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        return new LiveMatchStartedDto(match.Id);
    }

    [HttpGet("{matchId:guid}/current-down")]
    public async Task<ActionResult<CurrentDownDto>> CurrentDown(Guid matchId, [FromQuery] Guid teamId)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        if (match is null)
        {
            return NotFound();
        }

        var teams = await LoadTeams(match);
        if (teams is null)
        {
            return NotFound("Team not found.");
        }

        var (homeTeam, awayTeam) = teams.Value;
        if (teamId != homeTeam.Id && teamId != awayTeam.Id)
        {
            return BadRequest("That team isn't part of this match.");
        }

        var folders = await _db.PlaybookFolders.Where(f => f.TeamId == teamId).ToListAsync();
        return await BuildCurrentDownDto(match, homeTeam, awayTeam, teamId, folders);
    }

    [HttpPost("{matchId:guid}/submit-play")]
    public async Task<ActionResult<SubmitLivePlayResultDto>> SubmitPlay(Guid matchId, [FromBody] SubmitLivePlayRequest request)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        if (match is null)
        {
            return NotFound();
        }

        var teams = await LoadTeams(match);
        if (teams is null)
        {
            return NotFound("Team not found.");
        }

        var (homeTeam, awayTeam) = teams.Value;
        var outcome = LiveMatchOrchestrator.SubmitPlay(match, homeTeam, awayTeam, request.TeamId, request.PlayId);
        if (outcome != SubmitPlayOutcome.Accepted)
        {
            return BadRequest(outcome.ToString());
        }

        var players = await LoadPlayers(homeTeam, awayTeam);
        var resolved = LiveMatchOrchestrator.TryResolveCurrentDown(match, homeTeam, awayTeam, players, new Random());

        await _db.SaveChangesAsync();

        var folders = await _db.PlaybookFolders.Where(f => f.TeamId == request.TeamId).ToListAsync();
        var currentDown = await BuildCurrentDownDto(match, homeTeam, awayTeam, request.TeamId, folders);
        return new SubmitLivePlayResultDto(resolved is not null, resolved is null ? null : ToPlayResultDto(resolved, homeTeam, awayTeam, players), currentDown);
    }

    /// <summary>Manual stand-in for a per-down timeout - see the plan's "explicitly deferred"
    /// section on automating this via a background sweep instead.</summary>
    [HttpPost("{matchId:guid}/force-resolve")]
    public async Task<ActionResult<SubmitLivePlayResultDto>> ForceResolve(Guid matchId)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        if (match is null)
        {
            return NotFound();
        }

        var teams = await LoadTeams(match);
        if (teams is null)
        {
            return NotFound("Team not found.");
        }

        var (homeTeam, awayTeam) = teams.Value;
        var folders = await _db.PlaybookFolders.Where(f => f.TeamId == homeTeam.Id || f.TeamId == awayTeam.Id).ToListAsync();
        var players = await LoadPlayers(homeTeam, awayTeam);
        var resolved = LiveMatchOrchestrator.ForceResolveCurrentDown(match, homeTeam, awayTeam, folders, players, new Random());

        await _db.SaveChangesAsync();

        var currentDown = await BuildCurrentDownDto(match, homeTeam, awayTeam, homeTeam.Id, folders.Where(f => f.TeamId == homeTeam.Id).ToList());
        return new SubmitLivePlayResultDto(resolved is not null, resolved is null ? null : ToPlayResultDto(resolved, homeTeam, awayTeam, players), currentDown);
    }

    private async Task<(Team Home, Team Away)?> LoadTeams(Match match)
    {
        var homeTeam = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == match.HomeTeamId);
        var awayTeam = await _db.Teams.Include(t => t.Playbook).FirstOrDefaultAsync(t => t.Id == match.AwayTeamId);
        return homeTeam is null || awayTeam is null ? null : (homeTeam, awayTeam);
    }

    private async Task<Dictionary<Guid, Player>> LoadPlayers(Team homeTeam, Team awayTeam)
    {
        var rosteredIds = homeTeam.Roster.DepthChart.Values.SelectMany(ids => ids)
            .Concat(awayTeam.Roster.DepthChart.Values.SelectMany(ids => ids))
            .ToHashSet();
        return await _db.Players.Where(p => rosteredIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
    }

    private async Task<CurrentDownDto> BuildCurrentDownDto(
        Match match, Team homeTeam, Team awayTeam, Guid teamId, List<PlaybookFolder> yourFolders)
    {
        var isOffense = teamId == match.PossessionTeamId;
        var yourTeam = teamId == homeTeam.Id ? homeTeam : awayTeam;
        var yourPendingId = isOffense ? match.PendingOffensePlayId : match.PendingDefensePlayId;
        var opponentPendingId = isOffense ? match.PendingDefensePlayId : match.PendingOffensePlayId;

        var expectedCategory = isOffense ? Domain.Enums.PlayCategory.Offense : Domain.Enums.PlayCategory.Defense;
        var eligiblePlays = yourTeam.Playbook.Where(p => p.Category == expectedCategory).Select(DtoMapping.ToDto).ToList();
        var relevantFolders = yourFolders.Where(f => f.TeamId == teamId && f.Category == expectedCategory).ToList();

        var hint = SituationClassifier.Classify(MatchEngine.ToGameState(match)).ToString();

        return new CurrentDownDto(
            match.Id, match.Week,
            homeTeam.Id, homeTeam.Name, awayTeam.Id, awayTeam.Name,
            match.HomeScore, match.AwayScore, match.IsResolved,
            match.Quarter, match.Down, match.DistanceToGo, match.FieldPosition, match.PossessionTeamId,
            hint,
            isOffense ? "Offense" : "Defense",
            yourPendingId is not null, opponentPendingId is not null,
            relevantFolders.Select(PlaybookFoldersController.ToDto).ToList(), eligiblePlays);
    }

    private static PlayResultDto ToPlayResultDto(PlayResult play, Team homeTeam, Team awayTeam, IReadOnlyDictionary<Guid, Player> players)
    {
        var offensePlay = homeTeam.Playbook.Concat(awayTeam.Playbook).FirstOrDefault(p => p.Id == play.OffensePlayId);
        var defensePlay = homeTeam.Playbook.Concat(awayTeam.Playbook).FirstOrDefault(p => p.Id == play.DefensePlayId);

        string PlayerNameOf(Guid id) => players.TryGetValue(id, out var player) ? player.Name : "Unknown";

        return new PlayResultDto(
            play.PlayIndex, offensePlay?.Name ?? "Unknown Play", defensePlay?.Name ?? "Unknown Play", play.IsPassPlay,
            play.YardsGained, play.IsTurnover, play.IsScore,
            play.InjuryEvents.Select(e => $"{PlayerNameOf(e.PlayerId)} ({e.Result.Status})").ToList(),
            play.Quarter, play.Down, play.DistanceToGo, play.FieldPosition, play.PossessionTeamId,
            play.HomeScoreAfter, play.AwayScoreAfter);
    }
}
