import { useEffect, useState } from "react";
import { api } from "../api";
import type { MatchResult, ScheduledGame, StandingsRow } from "../types";

export function LeagueView({ teamNames }: { teamNames: Record<string, string> }) {
  const [standings, setStandings] = useState<StandingsRow[]>([]);
  const [schedule, setSchedule] = useState<ScheduledGame[]>([]);
  const [week, setWeek] = useState(1);
  const [results, setResults] = useState<MatchResult[]>([]);
  const [status, setStatus] = useState<string | null>(null);

  function refresh() {
    api.getStandings().then(setStandings);
    api.getSchedule().then(setSchedule);
  }

  useEffect(refresh, []);

  async function handleResolveWeek() {
    setStatus(`Resolving week ${week}...`);
    try {
      const weekResults = await api.resolveWeek(week);
      setResults(weekResults);
      setStatus(`Week ${week} resolved.`);
      refresh();
    } catch (e) {
      setStatus(`Failed to resolve week ${week}: ${String(e)}`);
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
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section>
        <h3>Resolve a Week</h3>
        <label>
          Week{" "}
          <input type="number" min={1} value={week} onChange={(e) => setWeek(Number(e.target.value))} />
        </label>
        <button onClick={handleResolveWeek}>Resolve Week</button>
        {status && <span className="status">{status}</span>}
      </section>

      {results.length > 0 && (
        <section>
          <h3>Results</h3>
          {results.map((match) => (
            <div key={match.matchId} className="match-result">
              <h4>
                {teamNames[match.homeTeamId] ?? match.homeTeamId} {match.homeScore} - {match.awayScore}{" "}
                {teamNames[match.awayTeamId] ?? match.awayTeamId}
              </h4>
              <pre className="summary-text">{match.summary}</pre>
              <pre className="summary-text">{match.boxScore}</pre>
            </div>
          ))}
        </section>
      )}
    </div>
  );
}
