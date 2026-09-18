# GronkBowl — Corrected Design Direction

## 1. Product vision

GronkBowl is a hybrid of Head Coach 09 and Blood Bowl, but the core fantasy is not management for its own sake.

The core fantasy is:

Two head coaches build a roster, install plays, create a game plan, and duel across a full football matchup play by play. The game is about real coaching decisions under pressure, then surviving the long-term consequences of roster damage, injury, and season pressure.

This is the actual product.

Not a generic sports management sim.
Not a spreadsheet-driven league admin app.
Not a passive simulation with a few tabs.

## 2. The actual fantasy we want

The player should feel like a real head coach:
- I know the down and distance.
- I know the field position.
- I know the mismatch.
- I know what the defense is trying to take away.
- I know which concept I am trying to exploit.
- I know when the risk is worth it.
- I can react when the opponent adjusts.
- I can win or lose because I called a good game.

That is the heart of the game.

## 3. The design fusion

### Head Coach 09 layer
- actual football decision-making
- play calling by situation
- game flow and pressure management
- down/distance adaptation
- tendency exploitation
- tactical response to opponent adjustments

### Blood Bowl layer
- absurd fantasy roster identity
- dangerous physicality
- injury risk and lethality
- weird team archetypes
- chaotic spectacle and violence
- permanent consequence for roster loss

This should feel like a football game with brutal fantasy consequences, not a stats-driven management app.

## 4. Product priorities

### Priority 1: actual matchup gameplay
This is the main event.
- full game simulation
- play-by-play execution
- actual coach duels
- down-and-distance decisions
- game script pressure

### Priority 2: roster and team identity
- rosters matter
- player quality matters
- fantasy race identity matters
- player roles matter

### Priority 3: consequences
- injuries change games
- injuries change seasons
- player loss matters
- roster attrition matters

### Priority 4: season and league flow
- standings
- weekly schedule
- long-term progression
- rivalries and franchise tension

Everything else is secondary.

## 5. What we are cutting for v1

These are not priority for the first playable version:
- giant relationship matrices between every faction
- mass hidden-stat complexity
- huge trait trees for every race
- deeply layered league governance
- broad ruleset mutation
- giant economic sim with many spreadsheets
- full live synchronous playcalling
- dashboard-heavy admin UI
- AI-generated “smart” planning that removes human coaching decisions

This is the scope reduction that makes the game real.

## 6. v1 gameplay loop

### Weekly preparation
- build roster and starting lineup
- review injuries and player availability
- install key offensive and defensive plays
- create call sheet by situation
- study opponent tendencies
- choose game plan and package mix

### Matchup phase
- both coaches take their turn in the matchup
- offense and defense choose plays based on game state
- the system resolves the full football game play by play
- contact can produce injuries and penalties
- momentum and tactical adaptation matter

### Postgame
- full replay and summary
- key drives and decision moments
- injuries and player impact
- scoreboard and season consequences

### Long-term loop
- seasons matter
- roster loses matter
- progression matters
- rivalries build over time

## 7. Core design pillars

### Pillar 1: actual coaching fantasy
The player is making choices that affect the game.

### Pillar 2: actual football matchups
It should feel like a football game, not a decision table.

### Pillar 3: brutal consequences
Players can be injured, disabled, and lost for long periods or permanently.

### Pillar 4: fantasy identity
Teams and races are distinctive and matter in game plan and matchup logic.

### Pillar 5: readable action
The player should understand what happened and why.

### Pillar 6: strong visual identity
The game should look and feel like sports theater, not software.

## 8. UI direction

The UI is not a dashboard.
The UI is a football tactical screen.

It should include:
- field and formation view
- down/distance display
- clock and score
- play selection panel
- opponent tendencies and tendencies readout
- drive summary and game script
- replay/timeline
- injury and consequence overlays
- important event cards

It should not be:
- tab-heavy management software
- generic forms and data panels
- bland admin layout
- “AI generated” styling with no identity

## 9. Match simulation model

The match engine must be central.

Each play resolves through a clear event chain:
- pre-snap read
- line interaction
- route or block development
- QB or ball carrier decision
- completion, run, or scramble result
- pressure and coverage effect
- tackle / pursuit / turnover
- injury check if contact occurs
- result and state update

This should be deterministic, reproducible, and replayable.

The server should generate an immutable event log that can drive:
- replay
- stat aggregation
- injury consequence logic
- post-game summary 
- long-term roster updates

## 10. Fantasy race direction

Races should matter in actual gameplay.
Not just flavor, not just cosmetics.

They should influence:
- roster identity
- matchup strengths and weaknesses
- play style fit
- injury tolerance
- aggression and pressure profile
- team strategy

The fantasy race system should be simpler and more legible than the earlier design draft.
A few strong identities are better than a giant network of complexity.

## 11. Injury and permanence

The injury layer is critical.
It is a defining element of the Blood Bowl fusion.

The system should create:
- dangerous contact
- big consequences for bad plays
- season-level roster stress
- meaningful long-term planning
- violent, memorable on-field events

But it should not be so swingy that the game feels random or unfair.

The key is high impact and clear consequences without complete chaos.

## 12. League and season layer

This supports the actual match fantasy, but the match stays primary.

League systems should include:
- standings
- schedule
- season progression
- roster attrition
- injuries and player loss
- rivalry tension
- franchise identity

This layer should not dominate the design.

## 13. The actual v1 promise

The first playable build must prove this:
- the coach can call plays meaningfully
- the matchup feels like an actual game
- the roster matters
- injuries reshape the game and season
- the fantasy identity is present
- the match feels exciting and readable
- the replay is satisfying

If that is not true, the game is not ready.

## 14. Execution rules for future work

Every feature should be judged by this filter:
- Does it improve the actual dueling football fantasy?
- Does it make the match more interesting or visible?
- Does it deepen the player’s sense of coaching control?
- Does it add spectacle, tension, or strategic clarity?

If the answer is no, it is not a v1 priority.

## 15. Final direction

This should be a tactical football coaching game with fantasy brutality and season-long consequences.

It should feel like:
- two head coaches matching wits
- real football game flow
- real play selection under pressure
- roster loss and injury consequences
- strange fantasy teams clashing in a brutal sporting war

That is the game.

Everything else should support that fantasy.
