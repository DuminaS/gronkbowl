import type {
  CallSheetDto,
  CurrentDown,
  DraftPickResult,
  DraftState,
  MatchDetail,
  MatchResult,
  PlayDto,
  PlaybookFolder,
  PlayerDto,
  RosterEntry,
  ScheduledGame,
  SeasonStatus,
  StandingsRow,
  SubmitLivePlayResult,
  TeamSummary,
  TradeResult,
} from "./types";

// Use whatever host the page itself was loaded from (localhost on this machine, or this
// machine's LAN IP when opened from a phone) rather than a hardcoded "localhost" - the API
// runs on the same machine as the dev server, just on a different port.
const BASE_URL = `http://${window.location.hostname}:5091/api`;

async function get<T>(path: string): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`);
  if (!response.ok) {
    throw new Error(`GET ${path} failed: ${response.status}`);
  }
  return response.json() as Promise<T>;
}

async function put<T>(path: string, body: unknown): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  if (!response.ok) {
    throw new Error(`PUT ${path} failed: ${response.status}`);
  }
  return response.json() as Promise<T>;
}

async function post<T>(path: string, body?: unknown): Promise<T> {
  const response = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  if (!response.ok) {
    const detail = await response.text().catch(() => "");
    throw new Error(`POST ${path} failed: ${response.status}${detail ? ` - ${detail}` : ""}`);
  }
  return response.json() as Promise<T>;
}

async function del(path: string): Promise<void> {
  const response = await fetch(`${BASE_URL}${path}`, { method: "DELETE" });
  if (!response.ok) {
    throw new Error(`DELETE ${path} failed: ${response.status}`);
  }
}

export const api = {
  getTeams: () => get<TeamSummary[]>("/teams"),
  getRoster: (teamId: string) => get<RosterEntry[]>(`/teams/${teamId}/roster`),
  getPlaybook: (teamId: string) => get<PlayDto[]>(`/teams/${teamId}/playbook`),
  getCallSheet: (teamId: string) => get<CallSheetDto | null>(`/teams/${teamId}/callsheet`),
  submitCallSheet: (
    teamId: string,
    week: number,
    offense: Record<string, string[]>,
    defense: Record<string, string[]>,
  ) =>
    put<CallSheetDto>(`/teams/${teamId}/callsheet`, {
      week,
      offensiveSituationalPlays: offense,
      defensiveSituationalPlays: defense,
    }),
  getStandings: () => get<StandingsRow[]>("/league/standings"),
  getSchedule: () => get<ScheduledGame[]>("/league/schedule"),
  getSeasonStatus: () => get<SeasonStatus>("/league/status"),
  resolveWeek: (week: number) => post<MatchResult[]>(`/league/weeks/${week}/resolve`),
  getMatch: (matchId: string) => get<MatchDetail>(`/league/matches/${matchId}`),

  getFolders: (teamId: string) => get<PlaybookFolder[]>(`/teams/${teamId}/folders`),
  createFolder: (teamId: string, category: "Offense" | "Defense", name: string) =>
    post<PlaybookFolder>(`/teams/${teamId}/folders`, { category, name }),
  updateFolder: (teamId: string, folderId: string, name: string, playIds: string[]) =>
    put<PlaybookFolder>(`/teams/${teamId}/folders/${folderId}`, { name, playIds }),
  deleteFolder: (teamId: string, folderId: string) => del(`/teams/${teamId}/folders/${folderId}`),

  startLiveMatch: (week: number, homeTeamId: string, awayTeamId: string) =>
    post<{ matchId: string }>("/league/matches/live/start", { week, homeTeamId, awayTeamId }),
  getCurrentDown: (matchId: string, teamId: string) =>
    get<CurrentDown>(`/league/matches/${matchId}/current-down?teamId=${teamId}`),
  submitLivePlay: (matchId: string, teamId: string, playId: string) =>
    post<SubmitLivePlayResult>(`/league/matches/${matchId}/submit-play`, { teamId, playId }),
  forceResolveDown: (matchId: string) =>
    post<SubmitLivePlayResult>(`/league/matches/${matchId}/force-resolve`),

  getDraft: () => get<DraftState | null>("/draft"),
  startDraft: (prospectCount: number, rounds: number) =>
    post<DraftState>("/draft/start", { prospectCount, rounds }),
  makeDraftPick: (teamId: string, prospectId: string) =>
    post<DraftPickResult>("/draft/pick", { teamId, prospectId }),

  getFreeAgents: () => get<PlayerDto[]>("/freeagents"),
  signFreeAgent: (playerId: string, teamId: string, annualGold: number, years: number) =>
    post<PlayerDto>(`/freeagents/${playerId}/sign`, {
      teamId,
      annualGold,
      years,
      guaranteedGold: annualGold,
      signingBonus: 0,
    }),

  executeTrade: (teamAId: string, playerAId: string, teamBId: string, playerBId: string) =>
    post<TradeResult>("/trades", { teamAId, playerAId, teamBId, playerBId }),
};
