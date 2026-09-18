import { useEffect, useState } from "react";
import { api } from "../api";
import type { RosterEntry } from "../types";

const INJURY_COLORS: Record<string, string> = {
  Healthy: "#3fbf60",
  BangedUp: "#e0a800",
  Niggling: "#e0a800",
  Out: "#e07b00",
  SeasonEnding: "#d9463a",
  CareerEnding: "#8b1a1a",
  Deceased: "#000000",
};

export function RosterView({ teamId }: { teamId: string }) {
  const [roster, setRoster] = useState<RosterEntry[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setError(null);
    api.getRoster(teamId).then(setRoster).catch((e) => setError(String(e)));
  }, [teamId]);

  if (error) return <p className="error">Failed to load roster: {error}</p>;

  return (
    <div className="roster-view">
      {roster.map((entry) => (
        <section key={entry.position} className="position-group">
          <h3>{entry.position}</h3>
          <table>
            <thead>
              <tr>
                <th>Depth</th>
                <th>Name</th>
                <th>Race</th>
                <th>SPD</th>
                <th>STR</th>
                <th>AGI</th>
                <th>AWR</th>
                <th>DUR</th>
                <th>AV</th>
                <th>Status</th>
                <th>Traits</th>
              </tr>
            </thead>
            <tbody>
              {entry.depthChart.map((player, index) => (
                <tr key={player.id}>
                  <td>{index + 1}</td>
                  <td>{player.name}</td>
                  <td>{player.race}</td>
                  <td>{player.speed}</td>
                  <td>{player.strength}</td>
                  <td>{player.agility}</td>
                  <td>{player.awareness}</td>
                  <td>{player.durability}</td>
                  <td>{player.armorValue}</td>
                  <td>
                    <span
                      className="injury-pill"
                      style={{ backgroundColor: INJURY_COLORS[player.injuryStatus] ?? "#999" }}
                    >
                      {player.injuryStatus}
                      {player.injuryWeeksRemaining > 0 ? ` (${player.injuryWeeksRemaining}wk)` : ""}
                    </span>
                  </td>
                  <td>{player.traits.join(", ")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      ))}
    </div>
  );
}
