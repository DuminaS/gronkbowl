# GronkBowl — Playable Prototype Brief

## 1. Product definition

GronkBowl is a head coach duel game: two coaches call plays against each other in a full football matchup, with roster management and season consequences layered on top.

The game must feel like:
- real tactical football coaching
- a duel over play design and game script
- a live or near-live matchup strategy fight
- dangerous fantasy team identity
- brutal but readable injury consequences

This is not a generic management sim. The match is the product.

## 2. Core fantasy

Two coaches build teams, install a game plan, call plays by down and distance, react to each other, and fight through a full game. Every snap matters. The matchup is the story. The roster and injury systems shape the fight but do not replace it.

## 3. What the player feels

The player should feel like a real head coach:
- I understand the field state.
- I know what the opponent is trying to take away.
- I recognize the matchup.
- I choose a concept for the moment.
- I can win or lose because my plan was better or worse in that specific game state.
- I can see why the result happened.

## 4. The actual game loop

### Weekly prep
- build and adjust roster
- set starting lineup and depth
- install the offensive and defensive packages
- create a call sheet by situation
- review tendencies and matchup notes
- choose game plan and risk profile

### Matchup phase
- coaches or players submit play selections by situation
- game resolves full play by play
- offense and defense react to each other
- game state and player quality matter
- injuries happen on contact
- momentum shifts as the game unfolds

### Postgame
- full replay and drive summary
- tactical analysis
- key decisions explained
- injuries and long-term roster impact
- franchise consequence

## 5. What v1 must include

### Must-have features
- roster building and starter team creation
- roster depth and position roles
- fantasy race identity and matchup strength
- playbook authoring or installation
- call sheet by down, distance, red zone, goal line, backed up, two-minute, etc.
- full play-by-play matchup simulation
- deterministic engine with event log
- replay generation
- injury and casualty system
- season standings and progression
- franchise-level pressure from match outcomes

### Nice-to-have for later
- deeper economic systems
- broad rule-change meta systems
- giant faction relationship rules
- deep hidden-stat complexity
- large player trait taxonomy
- advanced league politics

## 6. What v1 must not include

- giant relationship matrix for all factions
- deep hidden-stat black box
- admin/dashboard-first UX
- huge economic resource sim
- full live synchronous playcalling at launch
- AI-generated play calling as a substitute for coach decisions
- large governance and meta-rule design before core loop works

## 7. Game pillars

### Pillar 1: head coach duel
The match is the core event. This is not a franchise management game pretending to be sports.

### Pillar 2: football flow and pressure
The game must feel like football: down, distance, clock, field position, score, risk, momentum, pressure.

### Pillar 3: roster identity
The roster matters. Player quality and role fit matter. Good coaches build winning matchups.

### Pillar 4: danger and consequence
Contact causes injuries. Players can be lost for weeks or permanently. The roster feels vulnerable.

### Pillar 5: fantasy flavor
The absurdity, weird races, and spectacle matter, but they must serve the football match.

## 8. Visual and UX direction

The UI must feel like a football tactical command center, not a management app.

### Include
- field and formation view
- down, distance, clock, score
- play selection pane
- route and block overlays
- opponent tendency readout
- replay timeline
- drive summary
- injury and impact overlays
- tactical readouts

### Avoid
- tabbed admin layout
- data-heavy dashboard panels
- generic software design
- white-box, sterile UI
- flat, boring game presentation

## 9. Match simulation requirements

Every snap should feel like a tactical decision.

The core resolution loop should be:
- pre-snap read
- line interaction
- route or block development
- QB or ball carrier decision
- throw, run, or scramble result
- pursuit and tackle
- injury check
- consequence and state update

This should be readable in replay and postgame summary.

## 10. v1 prototype target

The prototype should prove:
- the coach can call meaningful plays
- the matchup is the focus
- the game feels like football
- players matter
- injuries matter
- the replay makes sense
- the player understands why the result happened

If the prototype cannot do that, it is not ready.

## 11. What to build first

### Build order
1. field state and down/distance model
2. playbook and play selection
3. matchup resolution engine
4. replay/event log
5. injury/armor system
6. roster depth and starter lineup
7. season progression layer
8. fantasy race identity
9. polish and presentation layer

## 12. Final direction

GronkBowl should be built as a strategic football coaching duel with Blood Bowl-style danger and absurd fantasy identity.

The match is the product. The league supports it. The roster matters. The injuries matter. The fantasy races matter. Everything else is secondary.
