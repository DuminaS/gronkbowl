import { useEffect, useState } from "react";
import { api } from "../api";
import type { PlayDto } from "../types";
import { FormationDiagram } from "./FormationDiagram";

export function PlaybookView({ teamId }: { teamId: string }) {
  const [plays, setPlays] = useState<PlayDto[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setError(null);
    api.getPlaybook(teamId).then(setPlays).catch((e) => setError(String(e)));
  }, [teamId]);

  if (error) return <p className="error">Failed to load playbook: {error}</p>;

  const offense = plays.filter((p) => p.category === "Offense");
  const defense = plays.filter((p) => p.category === "Defense");

  const renderGroup = (title: string, group: PlayDto[]) => (
    <section className="play-group">
      <h3>{title}</h3>
      <div className="play-cards">
        {group.map((play) => (
          <div key={play.id} className="play-card">
            <div className="play-card-header">
              <strong>{play.name}</strong>
              {play.isDefaultPlay && <span className="badge">starter - always public</span>}
            </div>
            <p>
              Featured: {play.primaryPosition} {play.isPassPlay ? "(pass)" : "(run)"}
            </p>
            <FormationDiagram assignments={play.assignments} isPassPlay={play.isPassPlay} />
          </div>
        ))}
      </div>
    </section>
  );

  return (
    <div className="playbook-view">
      {renderGroup("Offense", offense)}
      {renderGroup("Defense", defense)}
    </div>
  );
}
