import { useEffect, useState } from "react";
import { api } from "../api";
import type { PlayDto } from "../types";

const BUCKETS = [
  "Standard",
  "SecondAndShort",
  "SecondAndLong",
  "ThirdAndShort",
  "ThirdAndLong",
  "RedZone",
  "GoalLine",
  "TwoMinuteDrill",
  "BackedUp",
];

export function CallSheetBuilder({ teamId }: { teamId: string }) {
  const [plays, setPlays] = useState<PlayDto[]>([]);
  const [week, setWeek] = useState(1);
  const [offenseChoice, setOffenseChoice] = useState<Record<string, string>>({});
  const [defenseChoice, setDefenseChoice] = useState<Record<string, string>>({});
  const [status, setStatus] = useState<string | null>(null);

  useEffect(() => {
    setStatus(null);
    api.getPlaybook(teamId).then((loadedPlays) => {
      setPlays(loadedPlays);
      const offenseDefault = loadedPlays.find((p) => p.category === "Offense")?.id ?? "";
      const defenseDefault = loadedPlays.find((p) => p.category === "Defense")?.id ?? "";
      setOffenseChoice(Object.fromEntries(BUCKETS.map((b) => [b, offenseDefault])));
      setDefenseChoice(Object.fromEntries(BUCKETS.map((b) => [b, defenseDefault])));
    });

    api.getCallSheet(teamId).then((sheet) => {
      if (!sheet) return;
      setWeek(sheet.week);
      const offense: Record<string, string> = {};
      const defense: Record<string, string> = {};
      for (const bucket of sheet.buckets) {
        if (bucket.offensePlayIds[0]) offense[bucket.bucket] = bucket.offensePlayIds[0];
        if (bucket.defensePlayIds[0]) defense[bucket.bucket] = bucket.defensePlayIds[0];
      }
      setOffenseChoice((prev) => ({ ...prev, ...offense }));
      setDefenseChoice((prev) => ({ ...prev, ...defense }));
    });
  }, [teamId]);

  const offensePlays = plays.filter((p) => p.category === "Offense");
  const defensePlays = plays.filter((p) => p.category === "Defense");

  async function handleSubmit() {
    setStatus("Submitting...");
    try {
      const offensePayload = Object.fromEntries(BUCKETS.map((b) => [b, [offenseChoice[b]]]));
      const defensePayload = Object.fromEntries(BUCKETS.map((b) => [b, [defenseChoice[b]]]));
      await api.submitCallSheet(teamId, week, offensePayload, defensePayload);
      setStatus("Call sheet submitted.");
    } catch (e) {
      setStatus(`Failed to submit: ${String(e)}`);
    }
  }

  return (
    <div className="call-sheet-builder">
      <div className="call-sheet-header">
        <label>
          Week{" "}
          <input
            type="number"
            min={1}
            value={week}
            onChange={(e) => setWeek(Number(e.target.value))}
          />
        </label>
        <button onClick={handleSubmit}>Submit Call Sheet</button>
        {status && <span className="status">{status}</span>}
      </div>

      <table>
        <thead>
          <tr>
            <th>Situation</th>
            <th>Offensive Play</th>
            <th>Defensive Play</th>
          </tr>
        </thead>
        <tbody>
          {BUCKETS.map((bucket) => (
            <tr key={bucket}>
              <td>{bucket}</td>
              <td>
                <select
                  value={offenseChoice[bucket] ?? ""}
                  onChange={(e) => setOffenseChoice((prev) => ({ ...prev, [bucket]: e.target.value }))}
                >
                  {offensePlays.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </td>
              <td>
                <select
                  value={defenseChoice[bucket] ?? ""}
                  onChange={(e) => setDefenseChoice((prev) => ({ ...prev, [bucket]: e.target.value }))}
                >
                  {defensePlays.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name}
                    </option>
                  ))}
                </select>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <p className="hint">
        Manual only, by design - the top-priority play in each situation is exactly what the
        Engine will call. Miss the weekly deadline and the league generates a random sheet from
        your own installed plays instead.
      </p>
    </div>
  );
}
