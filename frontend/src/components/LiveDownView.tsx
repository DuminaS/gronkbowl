import { useEffect, useState } from "react";
import { api } from "../api";
import type { CurrentDown, PlayResult } from "../types";
import { MatchViewer } from "./MatchViewer";
import "./MatchViewer.css";

const ORDINALS = ["", "1st", "2nd", "3rd", "4th"];

function fieldPositionLabel(fieldPosition: number): string {
  if (fieldPosition === 50) return "midfield";
  return fieldPosition < 50 ? `own ${fieldPosition}` : `opp ${100 - fieldPosition}`;
}

function describeResolvedPlay(play: PlayResult, wasYourOffense: boolean): string {
  if (play.isScore) return `TOUCHDOWN on ${play.offensePlayName} for ${play.yardsGained} yards.`;
  if (play.isTurnover) return `TURNOVER - ${wasYourOffense ? "you" : "they"} lose the ball.`;
  const gain = play.yardsGained >= 0 ? `gained ${play.yardsGained}` : `lost ${Math.abs(play.yardsGained)}`;
  return `${play.offensePlayName} vs ${play.defensePlayName} - ${gain} yards.`;
}

export function LiveDownView({ matchId, teamId, onClose }: { matchId: string; teamId: string; onClose: () => void }) {
  const [current, setCurrent] = useState<CurrentDown | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [lastResolved, setLastResolved] = useState<PlayResult | null>(null);
  const [expandedFolderId, setExpandedFolderId] = useState<string | null>(null);

  function refresh() {
    api.getCurrentDown(matchId, teamId).then(setCurrent).catch((e) => setError(String(e)));
  }

  useEffect(refresh, [matchId, teamId]);

  useEffect(() => {
    if (!current || current.isResolved || !current.youHaveSubmitted || current.opponentHasSubmitted) return;
    const timer = setInterval(refresh, 4000);
    return () => clearInterval(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [current?.youHaveSubmitted, current?.opponentHasSubmitted, current?.isResolved]);

  async function handleSubmit(playId: string) {
    setStatus("Submitting...");
    try {
      const result = await api.submitLivePlay(matchId, teamId, playId);
      setCurrent(result.currentDown);
      setLastResolved(result.downResolved ? result.resolvedPlay : null);
      setExpandedFolderId(null);
      setStatus(result.downResolved ? null : "Submitted - waiting on the other coach.");
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleForceResolve() {
    setStatus("Forcing this down through...");
    try {
      const result = await api.forceResolveDown(matchId);
      setCurrent(result.currentDown);
      setLastResolved(result.downResolved ? result.resolvedPlay : null);
      setStatus(null);
    } catch (e) {
      setStatus(String(e));
    }
  }

  if (error) return <p className="error">Could not load this match: {error}</p>;
  if (!current) return <p className="status">Loading...</p>;

  if (current.isResolved) {
    return (
      <div className="match-viewer">
        <p className="status">
          Final: {current.homeTeamName} {current.homeScore} - {current.awayScore} {current.awayTeamName}
        </p>
        <MatchViewer matchId={matchId} onClose={onClose} />
      </div>
    );
  }

  const wasYourOffense = lastResolved?.possessionTeamId === teamId;

  return (
    <div className="match-viewer">
      <div className="match-viewer-header">
        <h3>
          Week {current.week}: {current.homeTeamName} vs {current.awayTeamName}
        </h3>
        <button className="secondary" onClick={onClose}>
          Close
        </button>
      </div>

      <div className="scoreboard-bug">
        <div className="scoreboard-team">
          <span className="scoreboard-team-name">{current.homeTeamName}</span>
          <span className="scoreboard-score">{current.homeScore}</span>
        </div>
        <div className="scoreboard-situation">
          <span className="scoreboard-quarter">Q{current.quarter}</span>
          <span className="scoreboard-down">
            {ORDINALS[current.down]} &amp; {current.distanceToGo}
          </span>
          <span className="scoreboard-spot">{fieldPositionLabel(current.fieldPosition)}</span>
        </div>
        <div className="scoreboard-team">
          <span className="scoreboard-team-name">{current.awayTeamName}</span>
          <span className="scoreboard-score">{current.awayScore}</span>
        </div>
      </div>

      <p className="hint">
        You're on <strong>{current.yourSide}</strong> this down ({current.situationHint}).
      </p>

      {lastResolved && (
        <div className={`event-banner ${lastResolved.isScore ? "score" : lastResolved.isTurnover ? "turnover" : "injury"}`}>
          {describeResolvedPlay(lastResolved, wasYourOffense)}
        </div>
      )}

      {current.youHaveSubmitted ? (
        <div>
          <p className="status">Waiting on the other coach to make their call...</p>
          <div className="match-viewer-controls">
            <button className="secondary" onClick={refresh}>
              Refresh
            </button>
            <button className="secondary" onClick={handleForceResolve}>
              Force This Down Through
            </button>
          </div>
        </div>
      ) : (
        <div>
          <h3>Your Call</h3>
          {current.yourFolders.length === 0 && (
            <p className="hint">No folders yet - pick from your whole playbook below.</p>
          )}
          <div className="play-cards">
            {current.yourFolders.map((folder) => (
              <div key={folder.id} className="play-card">
                <div className="play-card-header">
                  <strong>{folder.name}</strong>
                  <button className="secondary" onClick={() => setExpandedFolderId(expandedFolderId === folder.id ? null : folder.id)}>
                    {expandedFolderId === folder.id ? "Hide" : "Browse"}
                  </button>
                </div>
                {expandedFolderId === folder.id && (
                  <ul>
                    {folder.playIds.map((playId) => {
                      const play = current.yourEligiblePlays.find((p) => p.id === playId);
                      if (!play) return null;
                      return (
                        <li key={playId}>
                          <button onClick={() => handleSubmit(playId)}>{play.name}</button>
                        </li>
                      );
                    })}
                  </ul>
                )}
              </div>
            ))}
          </div>

          <h3>All Eligible Plays</h3>
          <div className="play-cards">
            {current.yourEligiblePlays.map((play) => (
              <div key={play.id} className="play-card">
                <div className="play-card-header">
                  <strong>{play.name}</strong>
                </div>
                <button onClick={() => handleSubmit(play.id)}>Call This Play</button>
              </div>
            ))}
          </div>
        </div>
      )}

      {status && <p className="status">{status}</p>}
    </div>
  );
}
