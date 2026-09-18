import { useEffect, useMemo, useRef, useState } from "react";
import { api } from "../api";
import type { MatchDetail, PlayResult } from "../types";
import "./MatchViewer.css";

const ORDINALS = ["", "1st", "2nd", "3rd", "4th"];

function fieldPositionLabel(fieldPosition: number): string {
  if (fieldPosition === 50) return "midfield";
  return fieldPosition < 50 ? `own ${fieldPosition}` : `opp ${100 - fieldPosition}`;
}

function describePlay(play: PlayResult, offenseTeamName: string, defenseTeamName: string): string {
  if (play.isScore) {
    return `TOUCHDOWN - ${offenseTeamName} ${play.offensePlayName} goes for ${play.yardsGained} yards.`;
  }
  if (play.isTurnover) {
    const kind = play.isPassPlay ? "intercepted" : "fumbled";
    return `TURNOVER - ${offenseTeamName}'s ${play.offensePlayName} is ${kind} by ${defenseTeamName}.`;
  }
  const gain = play.yardsGained >= 0 ? `gains ${play.yardsGained}` : `loses ${Math.abs(play.yardsGained)}`;
  return `${offenseTeamName} runs ${play.offensePlayName} vs. ${defenseTeamName}'s ${play.defensePlayName} - ${gain} yards.`;
}

export function MatchViewer({ matchId, onClose }: { matchId: string; onClose: () => void }) {
  const [detail, setDetail] = useState<MatchDetail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [index, setIndex] = useState(0);
  const [playing, setPlaying] = useState(false);
  const logEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    setDetail(null);
    setError(null);
    setIndex(0);
    setPlaying(false);
    api.getMatch(matchId).then(setDetail).catch((e) => setError(String(e)));
  }, [matchId]);

  useEffect(() => {
    if (!playing || !detail) return;
    if (index >= detail.plays.length - 1) {
      setPlaying(false);
      return;
    }
    const timer = setTimeout(() => setIndex((i) => i + 1), 900);
    return () => clearTimeout(timer);
  }, [playing, index, detail]);

  useEffect(() => {
    logEndRef.current?.scrollIntoView({ block: "nearest" });
  }, [index]);

  const play = detail?.plays[index];
  const offenseIsHome = play && detail ? play.possessionTeamId === detail.homeTeamId : true;
  const offenseTeamName = detail ? (offenseIsHome ? detail.homeTeamName : detail.awayTeamName) : "";
  const defenseTeamName = detail ? (offenseIsHome ? detail.awayTeamName : detail.homeTeamName) : "";

  const scoreBefore = useMemo(() => {
    if (!detail) return { home: 0, away: 0 };
    if (index === 0) return { home: 0, away: 0 };
    const prev = detail.plays[index - 1];
    return { home: prev.homeScoreAfter, away: prev.awayScoreAfter };
  }, [detail, index]);

  if (error) return <p className="error">Could not load match: {error}</p>;
  if (!detail) return <p className="status">Loading match...</p>;
  if (detail.plays.length === 0) return <p className="status">This match has no recorded plays.</p>;
  if (!play) return null;

  const isLast = index >= detail.plays.length - 1;
  const finalScore = isLast;
  const displayHomeScore = finalScore ? detail.homeScore : scoreBefore.home;
  const displayAwayScore = finalScore ? detail.awayScore : scoreBefore.away;

  return (
    <div className="match-viewer">
      <div className="match-viewer-header">
        <h3>
          Week {detail.week}: {detail.homeTeamName} vs {detail.awayTeamName}
        </h3>
        <button className="secondary" onClick={onClose}>
          Close
        </button>
      </div>

      <div className="scoreboard-bug">
        <div className="scoreboard-team">
          <span className="scoreboard-team-name">{detail.homeTeamName}</span>
          <span className="scoreboard-score">{displayHomeScore}</span>
        </div>
        <div className="scoreboard-situation">
          <span className="scoreboard-quarter">Q{play.quarter}</span>
          <span className="scoreboard-down">
            {ORDINALS[play.down]} &amp; {play.distanceToGo}
          </span>
          <span className="scoreboard-spot">{fieldPositionLabel(play.fieldPosition)}</span>
        </div>
        <div className="scoreboard-team">
          <span className="scoreboard-team-name">{detail.awayTeamName}</span>
          <span className="scoreboard-score">{displayAwayScore}</span>
        </div>
      </div>

      <div className="field-strip">
        <div className="field-strip-track">
          <div className="field-strip-endzone left" />
          <div className="field-strip-endzone right" />
          <div
            className="field-strip-marker"
            style={{ left: `${10 + (offenseIsHome ? play.fieldPosition : 100 - play.fieldPosition) * 0.8}%` }}
            title={`${offenseTeamName} ball at ${fieldPositionLabel(play.fieldPosition)}`}
          />
        </div>
        <div className="field-strip-caption">
          {offenseTeamName} ball, {fieldPositionLabel(play.fieldPosition)}
        </div>
      </div>

      {(play.isScore || play.isTurnover || play.injuryDescriptions.length > 0) && (
        <div className={`event-banner ${play.isScore ? "score" : play.isTurnover ? "turnover" : "injury"}`}>
          {play.isScore && "TOUCHDOWN"}
          {play.isTurnover && "TURNOVER"}
          {!play.isScore && !play.isTurnover && "INJURY"}
          {play.injuryDescriptions.length > 0 && (
            <span className="event-banner-detail"> - {play.injuryDescriptions.join(", ")}</span>
          )}
        </div>
      )}

      <p className="play-description">{describePlay(play, offenseTeamName, defenseTeamName)}</p>

      <div className="match-viewer-controls">
        <button className="secondary" disabled={index === 0} onClick={() => setIndex((i) => Math.max(0, i - 1))}>
          Prev
        </button>
        <button onClick={() => setPlaying((p) => !p)} disabled={isLast && !playing}>
          {playing ? "Pause" : "Play"}
        </button>
        <button
          className="secondary"
          disabled={isLast}
          onClick={() => setIndex((i) => Math.min(detail.plays.length - 1, i + 1))}
        >
          Next
        </button>
        <span className="status">
          Play {index + 1} of {detail.plays.length}
        </span>
      </div>

      <div className="drive-log">
        {detail.plays.slice(0, index + 1).map((p, i) => {
          const home = p.possessionTeamId === detail.homeTeamId;
          const offense = home ? detail.homeTeamName : detail.awayTeamName;
          const defense = home ? detail.awayTeamName : detail.homeTeamName;
          return (
            <div key={i} className={`drive-log-row ${i === index ? "current" : ""}`}>
              <span className="drive-log-situation">
                Q{p.quarter} {ORDINALS[p.down]}&amp;{p.distanceToGo} {fieldPositionLabel(p.fieldPosition)}
              </span>
              <span className="drive-log-text">{describePlay(p, offense, defense)}</span>
            </div>
          );
        })}
        <div ref={logEndRef} />
      </div>

      <h3>Box Score</h3>
      <table className="box-score-table">
        <thead>
          <tr>
            <th>Player</th>
            <th>Rush</th>
            <th>Rec</th>
            <th>Tkl/Hit</th>
            <th>TO Forced</th>
            <th>Inj Caused</th>
            <th>Times Inj</th>
          </tr>
        </thead>
        <tbody>
          {detail.boxScore.map((row) => (
            <tr key={row.playerId}>
              <td>
                {row.playerName} <span className="status">({row.race}, {row.position})</span>
              </td>
              <td>
                {row.rushingAttempts > 0 ? `${row.rushingAttempts}-${row.rushingYards}${row.rushingTouchdowns > 0 ? ` (${row.rushingTouchdowns} TD)` : ""}` : "-"}
              </td>
              <td>
                {row.receptions > 0 ? `${row.receptions}-${row.receivingYards}${row.receivingTouchdowns > 0 ? ` (${row.receivingTouchdowns} TD)` : ""}` : "-"}
              </td>
              <td>{row.tacklesOrHits || "-"}</td>
              <td>{row.turnoversForced || "-"}</td>
              <td>{row.injuriesCaused || "-"}</td>
              <td>{row.timesInjured || "-"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
