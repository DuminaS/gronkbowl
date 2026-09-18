# GronkBowl

A weekly async multiplayer fantasy football management sim: NFL Head Coach 09-style franchise/coaching depth, crossed with Blood Bowl-style lethal injuries and asymmetric fantasy races.

Full design reference (game design, system architecture, v1 scope lock): see the project's Master Design Document.

## v1 core loop

Off-season (draft, free agency, trades, facilities) -> Weekly Prep (install plays, build a manual call sheet, scout) -> Match Day (submit lineup + call sheet by deadline; server resolves the full game deterministically) -> Post-game (XP, injuries, standings) -> repeat.

## Repo layout

- `backend/` - C# / ASP.NET solution: domain models, deterministic match engine, league orchestrator, EF Core persistence, API.
- `frontend/` - React + TypeScript: roster, franchise, playbook/call-sheet builder, draft/market, match replay UI.
- `docs/` - supporting design/technical notes.

## Status

Roadmap tasks 1-12 done, a real persistence layer underneath it, and the draft/free-agency/trade primitives from task 11 are now wired all the way into the UI as an actual GM-mode flow.

**Running it locally:**
```
# one-time: a dedicated local Postgres container (not any other project's DB)
docker run -d --name gronkbowl-postgres -e POSTGRES_USER=gronkbowl -e POSTGRES_PASSWORD=gronkbowl_dev_local -e POSTGRES_DB=gronkbowl -p 5433:5432 postgres:16-alpine

cd backend/src/GronkBowl.Api && dotnet run --launch-profile http --urls http://localhost:5091
cd frontend && npm install && npm run dev   # http://localhost:5173
```
On startup the API applies EF Core migrations automatically and seeds a 4-team demo league (Ironclad Reapers/Ironkin, Skysprint Talons/Aelari, Warhide Crushers/Thornhide, Shiverfang Swarm/Skitterkin) plus a starting free-agent pool, **only if the database is empty** - restarting the API does not reset your league.

- `frontend/` has six tabs: **Roster**, **Playbook**, **Call Sheet** (manual only, no auto-fill), **Draft** (start a class, on-the-clock enforcement, rookie wage scale applied automatically), **Market** (sign free agents, propose trades between teams), and **League** (standings, schedule, "Resolve Week" runs the real Engine and renders the summary + box score).
- `GronkBowl.Api` REST surface: `GET /api/teams`, `.../roster`, `.../playbook`, `.../callsheet`, `PUT .../callsheet`, `GET /api/league/standings`, `/schedule`, `POST /api/league/weeks/{week}/resolve`, `GET/POST /api/draft`, `/draft/start`, `/draft/pick`, `GET /api/freeagents`, `POST /api/freeagents/{id}/sign`, `POST /api/trades`.
- **GM-mode flow (Draft, Free Agency, Trades):** a `DraftState` table persists a live draft's pick order (worst-record-first, via `DraftOrderCalculator`) and prospect pool; `DraftController` enforces that only the team on the clock can take an undrafted prospect, and applies `RookieWageScale` automatically. Free agents are derived - any player not on a roster and not part of an active draft class - and signed through the real `FreeAgencyResolver` (a single offer always "wins" its own resolution, same code path a competitive multi-offer market would use). Trades run through `TradeProcessor`'s cap check, refused outright rather than partially applied if either side would exceed its cap. All three were verified end-to-end against the live database: a pick, a signing, and a trade each actually moved a player onto a new roster.
- `GronkBowl.Infrastructure` is the real Persistence Layer: `GronkBowlDbContext` (EF Core + Npgsql/PostgreSQL). Aggregate roots (Player, Play, Team, CallSheet, Season, Match, League, DraftState) are real tables; nested structures that would otherwise need many small join tables (a Roster's depth chart, a Play's assignments, a Call Sheet's situational buckets, a Season's schedule/standings, a Match's event log, a DraftState's pick order/prospect pool) are stored as single `jsonb` columns via a generic `AsJson<T>()` conversion, with a value comparer so EF's change tracking still notices in-place mutations, not just reassignment. Team<->Play is a real many-to-many relation, since the default plays are genuinely shared rows, not copies per team. Verified end-to-end: migrations apply, the API only seeds when empty, and team/roster/standings state survives a full process restart.

Roadmap tasks 1-11 done underneath that UI and persistence layer:

- Core v1 schema in `GronkBowl.Domain` - `Player`, `Team`, `Roster`, `Play`, `CallSheet`, `Match` (carries a `SeasonId`), `Season`, `League`, `DraftState`, `InjuryResult`, `PlayerGameStats`, `PlayerSeasonStats`.
- `RaceProfiles` for the four launch races (Ironkin, Aelari, Thornhide, Skitterkin) - ceilings + one core racial ability each.
- `Roster` is a depth chart per position (starter/bench are derived views, not separate lists), so a mid-game injury auto-resolves to "next man up," and a new acquisition (`AddToDepthChart`) always joins at the bottom, not ahead of players already there.
- `GronkBowl.Engine.CasualtyResolver` implements the two-stage Casualty & Injury Table (armor check, then severity roll), deterministic given a seeded `Random`.
- `DefaultPlaybook` seeds the 5 offense / 5 defense starter plays every team ships with. `CallSheet.SelectPlay` picks a play by situation with a Standard-bucket fallback.
- `GronkBowl.Engine.MatchEngine.ResolveGame(...)` is the pure `resolve()` function from the design doc: two teams, their call sheets, and a seed in; a fully resolved `Match` with a populated, self-describing `EventLog` out. Verified deterministic and capable of producing in-game injuries and score variance across seeds.
- `GronkBowl.Engine.StatsAggregator` derives per-player game stats from the event log, folds them into `PlayerSeasonStats` keyed by (player, year, **team**) so a mid-season trade shows up as two rows, and `CareerHistory(...)` reads a player's whole career, year by year, team by team, off that same table.
- `GronkBowl.Engine.MatchSummaryBuilder` turns an `EventLog` into a readable recap and a box score.
- `GronkBowl.Engine.ScheduleGenerator` builds a round-robin schedule (circle method, handles odd team counts with a bye).
- `GronkBowl.Engine.RandomCallSheetGenerator` is the missed-deadline penalty: an unweighted shuffle of a team's own installed plays into every bucket, never a league-wide default.
- `GronkBowl.Engine.LeagueOrchestrator.ResolveWeek(...)` is the weekly submission flow end to end, with every seed (game and fallback) derived deterministically from season/week/team ids, never supplied by a caller.
- `GronkBowl.Engine.DraftProspectGenerator` / `RookieWageScale` / `DraftOrderCalculator` / `DraftBoard` (an in-memory board type the API's `DraftState` persists a live view of).
- `GronkBowl.Engine.FreeAgencyResolver` and `TradeProcessor`, described above.

`dotnet build` and `dotnet test` both pass (60 tests, all still in-memory/unit-level - no integration tests against Postgres yet).

## Task 14 (balance tuning) - six rounds, real fixes, genuinely not finished

Each round measured first, fixed the actual root cause, then re-measured - and each fix surfaced a new, deeper layer rather than wrapping the task up. In order:

1. **Ironkin-vs-Aelari.** Aelari measured 0 wins / 0 points across 40 games. Root cause: an uncapped pass-pressure penalty killed the passing game whenever the line battle was lost (which a Strength-poor race always loses), and two racial abilities (Aelari's Featherstep, Skitterkin's Scurry) existed only as text in `RaceProfiles`, never wired in. Fixed both. `BalanceTests` guards it.
2. **The other 5 race pairs.** Skitterkin was losing to everyone due to a 30-point Awareness deficit with no compensating mechanic. Made Scurry mechanically bigger than Featherstep (matching its "extra evasion die" text) - helped vs. Thornhide/Aelari, not vs. Ironkin.
3. **A defense-side Agility lever - tried, overcorrected, reverted** (stacked a third compounding advantage onto Aelari).
4. **Season-simulation harness (`SeasonSimulator`) - found Durability did nothing.** A "do nothing" season broke down to 0-0 for every team by week 6-8 (roster management isn't optional flavor), so the harness auto-signs emergency replacements and tracks how many a race needs as the real cost-of-fragility metric. First run showed Durability did nothing in `CasualtyResolver` beyond a minor duration tweak. Fixed: it now reduces the armor-break roll directly.
5. **Gave Ironkin a real offsetting weakness: Speed**, the one core attribute with zero mechanical hookup anywhere. Added a breakaway mechanic so a meaningfully faster ball carrier can turn a good gain into a big one - slow defenses (Ironkin, Thornhide) can't run it down. Overshot for Aelari (highest Speed in the game) initially, toned down. Also found and fixed the season harness always giving the same team "home"/first-possession for all 16 weeks, compounding a structural edge independent of race.
6. **Fixed the measurement tool itself.** `TestTeamFactory.BuildTeam` (used by every balance test) gave every player identical maximum-ceiling attributes, while `SeasonSimulator`'s replacements rolled realistic 40-75%-of-ceiling variance - any replacement was a categorical downgrade regardless of race, which could snowball into a death spiral unrelated to the actual matchup. Fixed: `BuildTeam` now rolls the same realistic distribution as everyone else.

**Where balance genuinely stands:** six real, verified fixes shipped, each with a permanent regression test (`BalanceTests`, `SeasonBalanceTests`, `CasualtyResolverTests.HigherDurability...`), zero regressions. Re-measuring after round 6 **changed the picture again**: Ironkin-vs-Thornhide flipped (Thornhide now leads), Thornhide-vs-Aelari tightened - but Ironkin-vs-Aelari, verified balanced under the old (flawed) methodology, now shows Ironkin dominating under the corrected one. That's the honest state: the tool is more valid now, but a single 16-week trial per pair is still a small sample against this many interacting mechanisms. Chasing individual pair numbers further without a much larger, systematic trial count would likely repeat the same round-after-round pattern rather than converge - paused here rather than starting a 7th round on that basis.

Next: a larger-scale, systematic balance pass (many trials per pair, controlling one variable at a time) if balance work continues, progression/XP tuning (untouched), task 13 (analytics/telemetry), and integration tests against the real database (everything so far is unit-level only).
