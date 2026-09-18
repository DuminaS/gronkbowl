import { useEffect, useState } from "react";
import { api } from "../api";
import type { DraftState, TeamSummary } from "../types";

export function DraftView({ teams, teamId, seasonComplete }: { teams: TeamSummary[]; teamId: string; seasonComplete: boolean }) {
  const [draft, setDraft] = useState<DraftState | null>(null);
  const [status, setStatus] = useState<string | null>(null);

  function refresh() {
    api.getDraft().then(setDraft).catch((e) => setStatus(String(e)));
  }

  useEffect(refresh, []);

  const teamNames = Object.fromEntries(teams.map((t) => [t.id, t.name]));

  async function handleStart() {
    setStatus("Starting draft...");
    try {
      const started = await api.startDraft(20, 3);
      setDraft(started);
      setStatus("Draft started.");
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handlePick(prospectId: string) {
    if (!draft?.onTheClockTeamId) return;
    setStatus("Submitting pick...");
    try {
      const result = await api.makeDraftPick(draft.onTheClockTeamId, prospectId);
      setStatus(`Pick ${result.overallPickNumber}: ${result.player.name} (${result.annualGold}g/yr x${result.years}yr)`);
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  if (!draft) {
    return (
      <div className="draft-view">
        {seasonComplete ? (
          <>
            <p>No draft in progress.</p>
            <button onClick={handleStart}>Start Draft</button>
          </>
        ) : (
          <p className="hint">The rookie draft is an offseason event - it opens once every game in the regular season has been played.</p>
        )}
        {status && <p className="status">{status}</p>}
      </div>
    );
  }

  const onClockName = draft.onTheClockTeamId ? teamNames[draft.onTheClockTeamId] ?? draft.onTheClockTeamId : null;
  const viewingTeamIsOnClock = draft.onTheClockTeamId === teamId;

  return (
    <div className="draft-view">
      <div className="call-sheet-header">
        <strong>
          Pick {draft.overallPickNumber} of {draft.totalPicks}
        </strong>
        {draft.isComplete ? <span>Draft complete.</span> : <span>On the clock: {onClockName}</span>}
        {status && <span className="status">{status}</span>}
      </div>

      {!draft.isComplete && !viewingTeamIsOnClock && (
        <p className="hint">Select {onClockName} in the team picker above to make their pick.</p>
      )}

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>Race</th>
            <th>Pos</th>
            <th>SPD</th>
            <th>STR</th>
            <th>AGI</th>
            <th>AWR</th>
            <th>DUR</th>
            <th>Traits</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {draft.availableProspects.map((p) => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td>{p.race}</td>
              <td>{p.position}</td>
              <td>{p.speed}</td>
              <td>{p.strength}</td>
              <td>{p.agility}</td>
              <td>{p.awareness}</td>
              <td>{p.durability}</td>
              <td>{p.traits.join(", ")}</td>
              <td>
                <button disabled={!viewingTeamIsOnClock} onClick={() => handlePick(p.id)}>
                  Draft
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
