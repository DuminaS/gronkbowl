import { useEffect, useState } from "react";
import { api } from "../api";
import type { MatchResult, ScheduledGame, StandingsRow } from "../types";
import { MatchViewer } from "./MatchViewer";
import { LiveDownView } from "./LiveDownView";

export function LeagueView({ teamNames, teamId }: { teamNames: Record<string, string>; teamId: string }) {
  const [standings, setStandings] = useState<StandingsRow[]>([]);
  const [schedule, setSchedule] = useState<ScheduledGame[]>([]);
  const [week, setWeek] = useState(1);
  const [results, setResults] = useState<MatchResult[]>([]);
  const [status, setStatus] = useState<string | null>(null);
  const [watchingMatchId, setWatchingMatchId] = useState<string | null>(null);
  const [playingMatchId, setPlayingMatchId] = useState<string | null>(null);

  function refresh() {
    api.getStandings().then(setStandings);
    api.getSchedule().then(setSchedule);
  }

  useEffect(refresh, []);

  async function handlePlay(game: ScheduledGame) {
    setStatus(null);
    try {
      const { matchId } = await api.startLiveMatch(game.week, game.homeTeamId, game.awayTeamId);
      setPlayingMatchId(matchId);
    } catch (e) {
      setStatus(`Could not start this game: ${String(e)}`);
    }
  }

  async function handleResolveWeek() {
    setStatus(`Simulating week ${week} instantly...`);
    try {
      const weekResults = await api.resolveWeek(week);
      setResults(weekResults);
      setStatus(`Week ${week} simulated.`);
      refresh();
    } catch (e) {
      setStatus(`Failed to simulate week ${week}: ${String(e)}`);
    }
  }

  return (
    <div className="league-view">
      <section>
        <h3>Standings</h3>
        <table>
          <thead>
            <tr>
              <th>Team</th>
              <th>W</th>
              <th>L</th>
              <th>T</th>
            </tr>
          </thead>
          <tbody>
            {standings.map((row) => (
              <tr key={row.teamId}>
                <td>{row.teamName}</td>
                <td>{row.wins}</td>
                <td>{row.losses}</td>
                <td>{row.ties}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section>
        <h3>Schedule</h3>
        <table>
          <thead>
            <tr>
              <th>Week</th>
              <th>Matchup</th>
              <th>Played</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {schedule.map((game, i) => (
              <tr key={i}>
                <td>{game.week}</td>
                <td>
                  {game.homeTeamName} vs {game.awayTeamName}
                </td>
                <td>{game.played ? "Yes" : "No"}</td>
                <td>
                  {!game.played && (game.homeTeamId === teamId || game.awayTeamId === teamId) && (
                    <button onClick={() => handlePlay(game)}>Play</button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      {playingMatchId && (
        <LiveDownView matchId={playingMatchId} teamId={teamId} onClose={() => { setPlayingMatchId(null); refresh(); }} />
      )}

      <section>
        <h3>Instant-Simulate a Week</h3>
        <p className="hint">
          Skips playing it down by down - resolves every game for the week at once. Useful for
          weeks nobody wants to play live, or for catching up quickly while testing.
        </p>
        <label>
          Week{" "}
          <input type="number" min={1} value={week} onChange={(e) => setWeek(Number(e.target.value))} />
        </label>
        <button className="secondary" onClick={handleResolveWeek}>
          Simulate Week Instantly
        </button>
        {status && <span className="status">{status}</span>}
      </section>

      {watchingMatchId && <MatchViewer matchId={watchingMatchId} onClose={() => setWatchingMatchId(null)} />}

      {results.length > 0 && (
        <section>
          <h3>Results</h3>
          {results.map((match) => (
            <div key={match.matchId} className="match-result">
              <h4>
                {teamNames[match.homeTeamId] ?? match.homeTeamId} {match.homeScore} - {match.awayScore}{" "}
                {teamNames[match.awayTeamId] ?? match.awayTeamId}
              </h4>
              <div className="match-result-actions">
                <button onClick={() => setWatchingMatchId(match.matchId)}>Watch</button>
              </div>
            </div>
          ))}
        </section>
      )}
    </div>
  );
}
