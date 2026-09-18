namespace GronkBowl.Api;

public sealed record TeamSummaryDto(Guid Id, string Name, int Gold, int CapSpace);

public sealed record PlayerDto(
    Guid Id, string Name, string Race, string Position,
    int Speed, int Strength, int Agility, int Awareness, int Durability,
    int ArmorValue, string InjuryStatus, int InjuryWeeksRemaining, List<string> Traits);

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

public sealed record MatchResultDto(
    Guid MatchId, int Week, Guid HomeTeamId, Guid AwayTeamId, int HomeScore, int AwayScore, string Summary, string BoxScore);

public sealed record DraftStateDto(
    Guid Id, Guid? OnTheClockTeamId, int OverallPickNumber, int TotalPicks, bool IsComplete, List<PlayerDto> AvailableProspects);

public sealed record StartDraftRequest(int ProspectCount, int Rounds);

public sealed record DraftPickRequest(Guid TeamId, Guid ProspectId);

public sealed record DraftPickResultDto(PlayerDto Player, int OverallPickNumber, int AnnualGold, int Years);

public sealed record SignFreeAgentRequest(Guid TeamId, int AnnualGold, int Years, int GuaranteedGold, int SigningBonus);

public sealed record TradeRequest(Guid TeamAId, Guid PlayerAId, Guid TeamBId, Guid PlayerBId);

public sealed record TradeResultDto(bool Success, string Message);
