namespace GronkBowl.Api;

public sealed record TeamSummaryDto(Guid Id, string Name, int Gold, int CapSpace);

public sealed record PlayerDto(
    Guid Id, string Name, string Race, string Position,
    int Speed, int Strength, int Agility, int Awareness, int Durability,
    int ArmorValue, string InjuryStatus, int InjuryWeeksRemaining, List<string> Traits,
    int Level, int SkillPoints, int? SppNeededForNextLevel, List<string> Skills,
    string? DevelopmentPotential, bool DevelopmentPotentialRevealed, int GamesPlayed);

public sealed record LevelUpRollDto(
    int Die1, int Die2, bool IsDoubles, string BaseOutcome,
    List<string> EligiblePrimarySkills, List<string> EligibleSecondarySkills);

public sealed record ApplyLevelUpRequest(int Die1, int Die2, bool UseSecondarySkill, string? SkillChoice, string? StatChoice);

public sealed record RosterEntryDto(string Position, List<PlayerDto> DepthChart);

public sealed record PlayAssignmentDto(string Slot, string Role);

public sealed record PlayDto(
    Guid Id, string Name, string Category, bool IsDefaultPlay, bool IsPassPlay,
    string PrimaryPosition, List<PlayAssignmentDto> Assignments);

public sealed record CallSheetBucketDto(string Bucket, List<Guid> OffensePlayIds, List<Guid> DefensePlayIds);

public sealed record CallSheetDto(Guid TeamId, int Week, bool Submitted, bool WasRandomlyGenerated, List<CallSheetBucketDto> Buckets);

public sealed record SubmitCallSheetRequest(
    int Week,
    Dictionary<string, List<Guid>> OffensiveSituationalPlays,
    Dictionary<string, List<Guid>> DefensiveSituationalPlays);

public sealed record StandingsRowDto(Guid TeamId, string TeamName, int Wins, int Losses, int Ties);

public sealed record ScheduledGameDto(int Week, Guid HomeTeamId, string HomeTeamName, Guid AwayTeamId, string AwayTeamName, bool Played);

public sealed record SeasonStatusDto(int TotalGames, int CompletedGames, int? NextUnplayedWeek, bool IsRegularSeasonComplete);

public sealed record MatchResultDto(
    Guid MatchId, int Week, Guid HomeTeamId, Guid AwayTeamId, int HomeScore, int AwayScore, string Summary, string BoxScore);

public sealed record PlayResultDto(
    int PlayIndex, string OffensePlayName, string DefensePlayName, bool IsPassPlay,
    int YardsGained, bool IsTurnover, bool IsScore, List<string> InjuryDescriptions,
    int Quarter, int Down, int DistanceToGo, int FieldPosition, Guid PossessionTeamId,
    int HomeScoreAfter, int AwayScoreAfter);

public sealed record BoxScoreRowDto(
    Guid PlayerId, string PlayerName, string Race, string Position,
    int RushingAttempts, int RushingYards, int RushingTouchdowns,
    int Receptions, int ReceivingYards, int ReceivingTouchdowns,
    int TacklesOrHits, int TurnoversForced, int InjuriesCaused, int TimesInjured);

public sealed record MatchDetailDto(
    Guid MatchId, int Week,
    Guid HomeTeamId, string HomeTeamName, Guid AwayTeamId, string AwayTeamName,
    int HomeScore, int AwayScore, List<PlayResultDto> Plays, List<BoxScoreRowDto> BoxScore);

public sealed record DraftStateDto(
    Guid Id, Guid? OnTheClockTeamId, int OverallPickNumber, int TotalPicks, bool IsComplete, List<PlayerDto> AvailableProspects);

public sealed record StartDraftRequest(int ProspectCount, int Rounds);

public sealed record DraftPickRequest(Guid TeamId, Guid ProspectId);

public sealed record DraftPickResultDto(PlayerDto Player, int OverallPickNumber, int AnnualGold, int Years);

public sealed record SignFreeAgentRequest(Guid TeamId, int AnnualGold, int Years, int GuaranteedGold, int SigningBonus);

public sealed record TradeRequest(Guid TeamAId, Guid PlayerAId, Guid TeamBId, Guid PlayerBId);

public sealed record TradeResultDto(bool Success, string Message);

public sealed record PlaybookFolderDto(Guid Id, Guid TeamId, string Category, string Name, List<Guid> PlayIds);

public sealed record CreateFolderRequest(string Category, string Name);

public sealed record UpdateFolderRequest(string Name, List<Guid> PlayIds);

public sealed record StartLiveMatchRequest(int Week, Guid HomeTeamId, Guid AwayTeamId);

/// <summary>Everything a coach needs to make (or watch someone else make) the call for the
/// down a live match is currently waiting on, scoped to whichever team is asking.</summary>
public sealed record CurrentDownDto(
    Guid MatchId, int Week,
    Guid HomeTeamId, string HomeTeamName, Guid AwayTeamId, string AwayTeamName,
    int HomeScore, int AwayScore, bool IsResolved,
    int Quarter, int Down, int DistanceToGo, int FieldPosition, Guid PossessionTeamId,
    string SituationHint,
    string YourSide, bool YouHaveSubmitted, bool OpponentHasSubmitted,
    List<PlaybookFolderDto> YourFolders, List<PlayDto> YourEligiblePlays);

public sealed record SubmitLivePlayRequest(Guid TeamId, Guid PlayId);

public sealed record SubmitLivePlayResultDto(bool DownResolved, PlayResultDto? ResolvedPlay, CurrentDownDto CurrentDown);

public sealed record LiveMatchStartedDto(Guid MatchId);
