# GronkBowl — Claude do-not-drift instructions

## Project identity

This project is not a generic sports management sim.
This project is not a spreadsheet-heavy admin game.
This project is not a broad league politics sim.
This project is not a hidden-stat black box.

The product is a football coaching duel game that blends:
- Head Coach 09 tactical decision-making
- Blood Bowl absurdity, danger, and lasting roster consequences

## Core fantasy

Two head coaches build rosters, install plays, prepare a game plan, and then fight through a full football matchup play by play. The match is the centerpiece.

## Non-negotiable design principles

1. Match gameplay is primary.
2. The player must feel like a real head coach.
3. The UI must feel like a tactical football game, not a dashboard.
4. The roster and season systems support the game, not replace it.
5. Injury consequences matter, but they should deepen the match, not distract from it.
6. Fantasy races matter in gameplay and identity, not just flavor.
7. Keep v1 small, readable, and playable.
8. If a feature does not improve the actual coaching duel, cut it.
9. If a UI element does not feel like football, cut it.
10. If a mechanic makes the game opaque, simplify it.

## Do not drift into

- management sim first
- spreadsheet dashboard design
- generic sports admin UX
- huge hidden-stat complexity
- bloated economy systems
- giant faction relationship matrices
- broad league politics as a core pillar
- AI auto-generated plays as a crutch
- broad ruleset governance before gameplay is proven
- live synchronous playcalling in v1

## v1 scope

In scope:
- roster creation and management
- player roles and player quality
- fantasy race identity and tactical strength
- playbook and game-plan authoring
- call sheet by down, distance, and game state
- full play-by-play game simulation
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
- generic admin dashboard-first UX

## Product priorities

Priority 1: actual coach-vs-coach gameplay
Priority 2: readable tactical presentation
Priority 3: roster and player quality
Priority 4: fantasy race asymmetry
Priority 5: injury and permanent consequences
Priority 6: season and league flow
Priority 7: advanced meta systems

## Guardrail questions

Before adding any feature, ask:
1. Does this improve the actual coach-vs-coach game?
2. Does this make the game feel more like a football duel and less like a dashboard?
3. Does this help the player understand the matchup better?

If the answer is no, cut or simplify it.

## Final truth

This project is not a generic sports sim.
This project is a tactical football coaching duel with Blood Bowl-style danger, absurdity, and lasting roster consequences.

The match is the product.
The league supports it.
The fantasy races support it.
The injury system supports it.
Everything else is secondary.
