import { useEffect, useState } from "react";
import { api } from "../api";
import type { PlayerDto, RosterEntry, TeamSummary } from "../types";

export function MarketView({ teams, teamId, seasonComplete }: { teams: TeamSummary[]; teamId: string; seasonComplete: boolean }) {
  const [freeAgents, setFreeAgents] = useState<PlayerDto[]>([]);
  const [status, setStatus] = useState<string | null>(null);
  const [offerGold, setOfferGold] = useState(100);
  const [offerYears, setOfferYears] = useState(2);

  const [myRoster, setMyRoster] = useState<RosterEntry[]>([]);
  const [otherTeamId, setOtherTeamId] = useState("");
  const [otherRoster, setOtherRoster] = useState<RosterEntry[]>([]);
  const [myPlayerId, setMyPlayerId] = useState("");
  const [theirPlayerId, setTheirPlayerId] = useState("");

  function refreshFreeAgents() {
    api.getFreeAgents().then(setFreeAgents).catch((e) => setStatus(String(e)));
  }

  useEffect(refreshFreeAgents, []);
  useEffect(() => {
    api.getRoster(teamId).then(setMyRoster);
  }, [teamId]);

  useEffect(() => {
    if (otherTeamId) api.getRoster(otherTeamId).then(setOtherRoster);
  }, [otherTeamId]);

  async function handleSign(playerId: string) {
    setStatus("Signing...");
    try {
      const player = await api.signFreeAgent(playerId, teamId, offerGold, offerYears);
      setStatus(`Signed ${player.name} for ${offerGold}g/yr x${offerYears}yr.`);
      refreshFreeAgents();
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleTrade() {
    if (!otherTeamId || !myPlayerId || !theirPlayerId) {
      setStatus("Pick a team and a player on each side first.");
      return;
    }
    setStatus("Proposing trade...");
    try {
      const result = await api.executeTrade(teamId, myPlayerId, otherTeamId, theirPlayerId);
      setStatus(result.message);
      if (result.success) {
        api.getRoster(teamId).then(setMyRoster);
        api.getRoster(otherTeamId).then(setOtherRoster);
      }
    } catch (e) {
      setStatus(String(e));
    }
  }

  const myPlayers = myRoster.flatMap((r) => r.depthChart);
  const theirPlayers = otherRoster.flatMap((r) => r.depthChart);
  const otherTeams = teams.filter((t) => t.id !== teamId);

  return (
    <div className="market-view">
      <section>
        <h3>Free Agents</h3>
        {!seasonComplete && (
          <p className="hint">Free agency is an offseason event - signings open once every game in the regular season has been played. Browse below, but signing is locked until then.</p>
        )}
        <div className="call-sheet-header">
          <label>
            Offer: Gold/yr{" "}
            <input type="number" value={offerGold} onChange={(e) => setOfferGold(Number(e.target.value))} style={{ width: 70 }} />
          </label>
          <label>
            Years{" "}
            <input type="number" min={1} value={offerYears} onChange={(e) => setOfferYears(Number(e.target.value))} style={{ width: 50 }} />
          </label>
        </div>
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
              <th></th>
            </tr>
          </thead>
          <tbody>
            {freeAgents.map((p) => (
              <tr key={p.id}>
                <td>{p.name}</td>
                <td>{p.race}</td>
                <td>{p.position}</td>
                <td>{p.speed}</td>
                <td>{p.strength}</td>
                <td>{p.agility}</td>
                <td>{p.awareness}</td>
                <td>{p.durability}</td>
                <td>
                  <button disabled={!seasonComplete} onClick={() => handleSign(p.id)}>
                    Sign
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section>
        <h3>Propose a Trade</h3>
        <div className="call-sheet-header">
          <label>
            My player{" "}
            <select value={myPlayerId} onChange={(e) => setMyPlayerId(e.target.value)}>
              <option value="">-</option>
              {myPlayers.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} ({p.position})
                </option>
              ))}
            </select>
          </label>
          <label>
            Trade with{" "}
            <select value={otherTeamId} onChange={(e) => setOtherTeamId(e.target.value)}>
              <option value="">-</option>
              {otherTeams.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.name}
                </option>
              ))}
            </select>
          </label>
          <label>
            Their player{" "}
            <select value={theirPlayerId} onChange={(e) => setTheirPlayerId(e.target.value)}>
              <option value="">-</option>
              {theirPlayers.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} ({p.position})
                </option>
              ))}
            </select>
          </label>
          <button onClick={handleTrade}>Propose Trade</button>
        </div>
      </section>

      {status && <p className="status">{status}</p>}
    </div>
  );
}
