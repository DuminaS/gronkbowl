import { useEffect, useState } from "react";
import "./App.css";
import { api } from "./api";
import type { SeasonStatus, TeamSummary } from "./types";
import { RosterView } from "./components/RosterView";
import { PlaybookView } from "./components/PlaybookView";
import { PlaybookOrganizer } from "./components/PlaybookOrganizer";
import { LeagueView } from "./components/LeagueView";
import { DraftView } from "./components/DraftView";
import { MarketView } from "./components/MarketView";

type Tab = "roster" | "playbook" | "folders" | "league" | "draft" | "market";

function App() {
  const [teams, setTeams] = useState<TeamSummary[]>([]);
  const [teamId, setTeamId] = useState<string>("");
  const [tab, setTab] = useState<Tab>("roster");
  const [error, setError] = useState<string | null>(null);
  const [seasonStatus, setSeasonStatus] = useState<SeasonStatus | null>(null);

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
    api.getSeasonStatus().then(setSeasonStatus).catch(() => {});
  }, []);

  const teamNames = Object.fromEntries(teams.map((t) => [t.id, t.name]));
  const activeTeam = teams.find((t) => t.id === teamId);
  const isOffseason = seasonStatus?.isRegularSeasonComplete ?? false;

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
            {seasonStatus && (
              <span className="team-meta">
                {isOffseason
                  ? "Offseason - Draft & Free Agency open"
                  : `Regular season - ${seasonStatus.completedGames}/${seasonStatus.totalGames} games played`}
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
            <button className={tab === "folders" ? "active" : ""} onClick={() => setTab("folders")}>
              Folders
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
            {teamId && tab === "folders" && <PlaybookOrganizer teamId={teamId} />}
            {teamId && tab === "draft" && <DraftView teams={teams} teamId={teamId} seasonComplete={isOffseason} />}
            {teamId && tab === "market" && <MarketView teams={teams} teamId={teamId} seasonComplete={isOffseason} />}
            {tab === "league" && <LeagueView teamNames={teamNames} teamId={teamId} />}
          </main>
        </>
      )}
    </div>
  );
}

export default App;
