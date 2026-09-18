import { useEffect, useState } from "react";
import "./App.css";
import { api } from "./api";
import type { TeamSummary } from "./types";
import { RosterView } from "./components/RosterView";
import { PlaybookView } from "./components/PlaybookView";
import { CallSheetBuilder } from "./components/CallSheetBuilder";
import { LeagueView } from "./components/LeagueView";
import { DraftView } from "./components/DraftView";
import { MarketView } from "./components/MarketView";

type Tab = "roster" | "playbook" | "callsheet" | "league" | "draft" | "market";

function App() {
  const [teams, setTeams] = useState<TeamSummary[]>([]);
  const [teamId, setTeamId] = useState<string>("");
  const [tab, setTab] = useState<Tab>("roster");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api
      .getTeams()
      .then((loaded) => {
        setTeams(loaded);
        if (loaded.length > 0) setTeamId(loaded[0].id);
      })
      .catch((e) =>
        setError(
          `Could not reach the API at http://${window.location.hostname}:5091 - is the backend running, and reachable from this device? (${String(e)})`,
        ),
      );
  }, []);

  const teamNames = Object.fromEntries(teams.map((t) => [t.id, t.name]));
  const activeTeam = teams.find((t) => t.id === teamId);

  return (
    <div className="app">
      <header>
        <h1>GronkBowl</h1>
        <p className="tagline">Weekly async fantasy football management</p>
      </header>

      {error && <p className="error">{error}</p>}

      {!error && (
        <>
          <div className="toolbar">
            <label>
              Team{" "}
              <select value={teamId} onChange={(e) => setTeamId(e.target.value)}>
                {teams.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </select>
            </label>
            {activeTeam && (
              <span className="team-meta">
                Gold: {activeTeam.gold} | Cap space: {activeTeam.capSpace}
              </span>
            )}
          </div>

          <nav className="tabs">
            <button className={tab === "roster" ? "active" : ""} onClick={() => setTab("roster")}>
              Roster
            </button>
            <button className={tab === "playbook" ? "active" : ""} onClick={() => setTab("playbook")}>
              Playbook
            </button>
            <button className={tab === "callsheet" ? "active" : ""} onClick={() => setTab("callsheet")}>
              Call Sheet
            </button>
            <button className={tab === "draft" ? "active" : ""} onClick={() => setTab("draft")}>
              Draft
            </button>
            <button className={tab === "market" ? "active" : ""} onClick={() => setTab("market")}>
              Market
            </button>
            <button className={tab === "league" ? "active" : ""} onClick={() => setTab("league")}>
              League
            </button>
          </nav>

          <main>
            {teamId && tab === "roster" && <RosterView teamId={teamId} />}
            {teamId && tab === "playbook" && <PlaybookView teamId={teamId} />}
            {teamId && tab === "callsheet" && <CallSheetBuilder teamId={teamId} />}
            {teamId && tab === "draft" && <DraftView teams={teams} teamId={teamId} />}
            {teamId && tab === "market" && <MarketView teams={teams} teamId={teamId} />}
            {tab === "league" && <LeagueView teamNames={teamNames} />}
          </main>
        </>
      )}
    </div>
  );
}

export default App;
