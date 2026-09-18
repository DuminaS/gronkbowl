# GronkBowl — Claude guardrail file

This project is not a generic sports management sim.

## Core fantasy

The game must feel like two head coaches facing each other across the field, calling plays, reacting to the game state, and dueling through a full matchup play by play.

The real product fantasy is:

Two coaches build rosters, install plays, plan a game, and then fight through an actual football game where down, distance, field position, opponent tendencies, pressure, clock, and player quality all matter.

This is a hybrid of:
- Head Coach 09: coaching decisions, tactical play calling, game flow, pressure, matchup adaptation
- Blood Bowl: absurdity, violence, danger, weird factions, injury consequences, chaotic spectacle

## Non-negotiable design direction

1. Match gameplay is the center of gravity.
2. The player must feel like a real head coach.
3. The UI must feel like a football tactical game, not a dashboard.
4. The season and roster systems support the match, not replace it.
5. Injury and roster consequences matter, but the actual matchup remains the heart of the game.
6. Fantasy races and factions matter in gameplay and identity, not just aesthetics.
7. Keep v1 small, legible, and playable.
8. If a feature does not improve the coaching duel, cut it.
9. If a UI element does not feel like a football game, cut it.
10. If a mechanic makes the game opaque, simplify it.

## Do not drift into

- management sim first
- spreadsheet dashboard design
- generic sports admin UI
- large hidden-stat black box
- bloated economy systems
- giant faction relationship matrices
- broad league politics as core pillar
- AI auto-generated plays as a crutch
- broad ruleset governance before gameplay is proven
- live synchronous playcalling in v1

## v1 scope

In scope:
- roster creation and management
- player roles, skills, and injury states
- fantasy race identity and matchup strength
- playbook and game-plan authoring
- call sheet by down, distance, and game state
- full game simulation play by play
- deterministic match engine with replays
- injury and casualty consequences
- basic season and standings
- team and coach identity

Out of scope for v1:
- giant faction politics
- massive economic simulation
- huge hidden-stat complexity
- broad league governance systems
- giant relationship graph between all races
- complex meta-rule evolution before core loop works
- live synchronous play-calling
- generic admin/dashboard-first UX

## Product priorities

Priority 1: actual coach-vs-coach gameplay
Priority 2: readable tactical game presentation
Priority 3: roster and player quality
Priority 4: fantasy race asymmetry
Priority 5: injury and permanent consequences
Priority 6: season and league flow
Priority 7: advanced meta systems

## Gameplay structure

### Weekly preparation
- build roster
- install offensive and defensive plays
- build call sheet by situation
- scout tendencies
- choose starters
- manage injuries and depth

### Matchup phase
- both coaches select plays by situation
- the system resolves the full game play by play
- offense and defense react to tendencies and game state
- injuries and contact matter
- replay is generated from event logs

### Post-game
- full drive recap
- tactical breakdown
- key decisions
- injuries and roster effect
- franchise impact

## UI direction

The UI should feel like a football command center, tactical board, and game film room.

It should include:
- field view
- down, distance, clock, score
- play selection pane
- formation and route overlays
- opponent tendencies and readout
- replay/timeline
- injury and impact overlays
- clear coach-facing tactical information

It should not look like:
- generic software tabs
- data tables everywhere
- admin app layout
- bland dashboard panels

## Match simulation expectations

The full game must be resolved through a play-by-play engine. Every snap should feel like a tactical decision:

- pre-snap read
- line interaction
- route/block development
- QB or ball carrier decision
- throw/run execution
- pursuit and tackle
- injury check on contact
- result and state update

The game must be explainable after the fact.

## Guardrail for implementation

When adding features, ask three questions:
1. Does this improve the actual coach-vs-coach game?
2. Does this make the game feel more like a football duel and less like a dashboard?
3. Does this help the player understand the matchup better?

If the answer is no, cut or simplify it.

## Final truth

This project is not a generic sports sim.

It is a tactical football coaching duel with Blood Bowl-style danger, absurdity, and lasting roster consequences.

The match is the product.
The league supports it.
The fantasy races support it.
The injury system supports it.
Everything else is secondary.
