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
