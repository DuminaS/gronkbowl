import { useEffect, useState } from "react";
import { api } from "../api";
import type { PlayDto, PlaybookFolder } from "../types";

export function PlaybookOrganizer({ teamId }: { teamId: string }) {
  const [folders, setFolders] = useState<PlaybookFolder[]>([]);
  const [plays, setPlays] = useState<PlayDto[]>([]);
  const [newFolderName, setNewFolderName] = useState("");
  const [newFolderCategory, setNewFolderCategory] = useState<"Offense" | "Defense">("Offense");
  const [status, setStatus] = useState<string | null>(null);

  function refresh() {
    api.getFolders(teamId).then(setFolders).catch((e) => setStatus(String(e)));
    api.getPlaybook(teamId).then(setPlays).catch((e) => setStatus(String(e)));
  }

  useEffect(refresh, [teamId]);

  const playsById = Object.fromEntries(plays.map((p) => [p.id, p]));

  async function handleCreateFolder() {
    if (!newFolderName.trim()) {
      setStatus("Give the folder a name first.");
      return;
    }
    try {
      await api.createFolder(teamId, newFolderCategory, newFolderName.trim());
      setNewFolderName("");
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleRename(folder: PlaybookFolder, name: string) {
    try {
      await api.updateFolder(teamId, folder.id, name, folder.playIds);
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleAddPlay(folder: PlaybookFolder, playId: string) {
    if (!playId || folder.playIds.includes(playId)) return;
    try {
      await api.updateFolder(teamId, folder.id, folder.name, [...folder.playIds, playId]);
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleRemovePlay(folder: PlaybookFolder, playId: string) {
    try {
      await api.updateFolder(teamId, folder.id, folder.name, folder.playIds.filter((id) => id !== playId));
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  async function handleDeleteFolder(folder: PlaybookFolder) {
    try {
      await api.deleteFolder(teamId, folder.id);
      refresh();
    } catch (e) {
      setStatus(String(e));
    }
  }

  const renderSide = (category: "Offense" | "Defense") => {
    const sideFolders = folders.filter((f) => f.category === category);
    const sidePlays = plays.filter((p) => p.category === category);

    return (
      <section className="play-group">
        <h3>{category}</h3>
        <div className="play-cards">
          {sideFolders.map((folder) => {
            const unfiled = sidePlays.filter((p) => !folder.playIds.includes(p.id));
            return (
              <div key={folder.id} className="play-card folder-card">
                <div className="play-card-header">
                  <input
                    className="folder-name-input"
                    defaultValue={folder.name}
                    onBlur={(e) => e.target.value.trim() && e.target.value !== folder.name && handleRename(folder, e.target.value.trim())}
                  />
                  <button className="secondary" onClick={() => handleDeleteFolder(folder)}>
                    Delete
                  </button>
                </div>
                <ul>
                  {folder.playIds.map((id) => (
                    <li key={id}>
                      {playsById[id]?.name ?? "Unknown play"}{" "}
                      <button className="secondary" onClick={() => handleRemovePlay(folder, id)}>
                        remove
                      </button>
                    </li>
                  ))}
                  {folder.playIds.length === 0 && <li className="hint">No plays filed here yet.</li>}
                </ul>
                {unfiled.length > 0 && (
                  <select defaultValue="" onChange={(e) => handleAddPlay(folder, e.target.value)}>
                    <option value="" disabled>
                      + file a play...
                    </option>
                    {unfiled.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name}
                      </option>
                    ))}
                  </select>
                )}
              </div>
            );
          })}
        </div>
      </section>
    );
  };

  return (
    <div className="playbook-organizer">
      <p className="hint">
        Organize your own plays into your own named folders - however makes sense to you. During a
        live down you'll browse into these to make your call.
      </p>
      <div className="call-sheet-header">
        <input
          placeholder="New folder name"
          value={newFolderName}
          onChange={(e) => setNewFolderName(e.target.value)}
        />
        <select value={newFolderCategory} onChange={(e) => setNewFolderCategory(e.target.value as "Offense" | "Defense")}>
          <option value="Offense">Offense</option>
          <option value="Defense">Defense</option>
        </select>
        <button onClick={handleCreateFolder}>New Folder</button>
        {status && <span className="status">{status}</span>}
      </div>

      {renderSide("Offense")}
      {renderSide("Defense")}
    </div>
  );
}
