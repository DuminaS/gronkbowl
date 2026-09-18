import type {
  CallSheetDto,
  DraftPickResult,
  DraftState,
  MatchResult,
  PlayDto,
  PlayerDto,
  RosterEntry,
  ScheduledGame,
  StandingsRow,
  TeamSummary,
  TradeResult,
} from "./types";

const BASE_URL = "http://localhost:5091/api";

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
  resolveWeek: (week: number) => post<MatchResult[]>(`/league/weeks/${week}/resolve`),

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
