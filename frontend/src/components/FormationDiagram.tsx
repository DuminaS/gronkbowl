import type { PlayAssignment } from "../types";
import "./FormationDiagram.css";

// The backend has no spatial data for a play (PlayAssignment is just {slot, role} - see
// GronkBowl.Domain.PlayAssignment) - this is a static, honest approximation of formation shape
// from that real slot/role data, not a fabricated route tree. Offense lines up near the bottom
// (its own line of scrimmage), defense near the top, facing across it.
const OFFENSE_LAYOUT: Record<string, { x: number; y: number }> = {
  QB: { x: 50, y: 74 },
  RB: { x: 38, y: 78 },
  WR: { x: 12, y: 82 },
  TE: { x: 66, y: 80 },
  OL: { x: 50, y: 88 },
  K: { x: 50, y: 92 },
  P: { x: 50, y: 92 },
};

const DEFENSE_LAYOUT: Record<string, { x: number; y: number }> = {
  DL: { x: 50, y: 60 },
  LB: { x: 50, y: 46 },
  CB: { x: 14, y: 36 },
  S: { x: 50, y: 22 },
};

const ROLE_LABEL: Record<string, string> = {
  Route: "route",
  Block: "block",
  Rush: "rush",
  CoverageZone: "zone",
  CoverageMan: "man",
};

export function FormationDiagram({ assignments, isPassPlay }: { assignments: PlayAssignment[]; isPassPlay: boolean }) {
  const seenAtSpot = new Map<string, number>();

  const dots = assignments.map((a, i) => {
    const isOffenseSlot = a.slot in OFFENSE_LAYOUT;
    const base = isOffenseSlot ? OFFENSE_LAYOUT[a.slot] : DEFENSE_LAYOUT[a.slot];
    if (!base) return null;

    // Spread out same-position duplicates (e.g. two WRs) instead of stacking them exactly.
    const occurrence = seenAtSpot.get(a.slot) ?? 0;
    seenAtSpot.set(a.slot, occurrence + 1);
    const spread = occurrence * 14 * (occurrence % 2 === 0 ? 1 : -1);

    return (
      <div
        key={i}
        className={`formation-dot ${isOffenseSlot ? "offense" : "defense"}`}
        style={{ left: `${Math.min(92, Math.max(8, base.x + spread))}%`, top: `${base.y}%` }}
        title={`${a.slot} - ${a.role}`}
      >
        <span className="formation-dot-label">{a.slot}</span>
        <span className="formation-dot-role">{ROLE_LABEL[a.role] ?? a.role}</span>
      </div>
    );
  });

  return (
    <div className="formation-diagram" data-kind={isPassPlay ? "pass" : "run"}>
      <div className="formation-los" />
      {dots}
    </div>
  );
}
