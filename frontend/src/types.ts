export interface TeamSummary {
  id: string;
  name: string;
  gold: number;
  capSpace: number;
}

export interface PlayerDto {
  id: string;
  name: string;
  race: string;
  position: string;
  speed: number;
  strength: number;
  agility: number;
  awareness: number;
  durability: number;
  armorValue: number;
  injuryStatus: string;
  injuryWeeksRemaining: number;
  traits: string[];
}

export interface RosterEntry {
  position: string;
  depthChart: PlayerDto[];
}

export interface PlayAssignment {
  slot: string;
  role: string;
}

export interface PlayDto {
  id: string;
  name: string;
  category: "Offense" | "Defense";
  isDefaultPlay: boolean;
  isPassPlay: boolean;
  primaryPosition: string;
  assignments: PlayAssignment[];
}

export interface CallSheetBucket {
  bucket: string;
  offensePlayIds: string[];
  defensePlayIds: string[];
}

export interface CallSheetDto {
  teamId: string;
  week: number;
  submitted: boolean;
  wasRandomlyGenerated: boolean;
  buckets: CallSheetBucket[];
}

export interface StandingsRow {
  teamId: string;
  teamName: string;
  wins: number;
  losses: number;
  ties: number;
}

export interface ScheduledGame {
  week: number;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  played: boolean;
}

export interface MatchResult {
  matchId: string;
  week: number;
  homeTeamId: string;
  awayTeamId: string;
  homeScore: number;
  awayScore: number;
  summary: string;
  boxScore: string;
}

export interface PlayResult {
  playIndex: number;
  offensePlayName: string;
  defensePlayName: string;
  isPassPlay: boolean;
  yardsGained: number;
  isTurnover: boolean;
  isScore: boolean;
  injuryDescriptions: string[];
  quarter: number;
  down: number;
  distanceToGo: number;
  fieldPosition: number;
  possessionTeamId: string;
  homeScoreAfter: number;
  awayScoreAfter: number;
}

export interface BoxScoreRow {
  playerId: string;
  playerName: string;
  race: string;
  position: string;
  rushingAttempts: number;
  rushingYards: number;
  rushingTouchdowns: number;
  receptions: number;
  receivingYards: number;
  receivingTouchdowns: number;
  tacklesOrHits: number;
  turnoversForced: number;
  injuriesCaused: number;
  timesInjured: number;
}

export interface MatchDetail {
  matchId: string;
  week: number;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  homeScore: number;
  awayScore: number;
  plays: PlayResult[];
  boxScore: BoxScoreRow[];
}

export interface DraftState {
  id: string;
  onTheClockTeamId: string | null;
  overallPickNumber: number;
  totalPicks: number;
  isComplete: boolean;
  availableProspects: PlayerDto[];
}

export interface DraftPickResult {
  player: PlayerDto;
  overallPickNumber: number;
  annualGold: number;
  years: number;
}

export interface TradeResult {
  success: boolean;
  message: string;
}

export interface SeasonStatus {
  totalGames: number;
  completedGames: number;
  nextUnplayedWeek: number | null;
  isRegularSeasonComplete: boolean;
}

export interface PlaybookFolder {
  id: string;
  teamId: string;
  category: "Offense" | "Defense";
  name: string;
  playIds: string[];
}

export interface CurrentDown {
  matchId: string;
  week: number;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  homeScore: number;
  awayScore: number;
  isResolved: boolean;
  quarter: number;
  down: number;
  distanceToGo: number;
  fieldPosition: number;
  possessionTeamId: string;
  situationHint: string;
  yourSide: "Offense" | "Defense";
  youHaveSubmitted: boolean;
  opponentHasSubmitted: boolean;
  yourFolders: PlaybookFolder[];
  yourEligiblePlays: PlayDto[];
}

export interface SubmitLivePlayResult {
  downResolved: boolean;
  resolvedPlay: PlayResult | null;
  currentDown: CurrentDown;
}
