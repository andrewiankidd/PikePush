# PikePush — Backlog

Living TODO. Items are **removed** when shipped, not archived here. If you
want historical narrative on the bigger design ideas, see
`c:/git/pikepush.md` (the working design doc — kept outside the repo).

This is the only dev-internal page in `docs/` — everything else is
client-facing reference material.

Campaign work is split into milestones: **V1** = must-ship for the first
campaign release; **V2** = follow-up wave; **V3** = much later. Drill
and Engineering items don't carry a milestone — they support whatever's
currently being built.

---

## Architectural decisions (locked-in for V1)

These are answered and not up for re-litigation while V1 is in flight.

- **Per-block state.** The block is the playable unit. Soldiers are
  visual fodder — no per-soldier morale, casualties, or kit. Posture,
  morale, kit-tier, strength all live on `Block`.
- **Scenario-based with dynamic map swap.** Not a scene per battle.
  One generic battle scene loads a `BattleScenario` ScriptableObject
  that swaps map, friendly forces, enemy forces, victory conditions.
- **No mid-battle save.** Campaign progress saves only between
  engagements. Drops a huge category of serialisation complexity.
- **Pause / speed-up is blocked during interactive MeterGame.** The
  player's own mash rate can't be warped. Speed-up applies to march /
  movement / NPC-vs-NPC phases only.
- **Multi-block selection is standard.** Shift-click adds to selection;
  command issued applies to all selected. Land it in Drill mode first so
  it's tested before Campaign needs it.
- **Refactor early, KISS / DRY.** When a system gains a second consumer
  (e.g. customisation in Runner *and* Drill *and* Campaign), centralise
  immediately. Don't carry duplication into V1.
- **Army-wide loot for V1.** No per-soldier customisation, no
  recruitment, no captains. Loot upgrades apply to the whole army.
  Regimental roster mechanics are V2.
- **No mid-battle save format yet** — save format includes a
  `save_version` int from day one so V2 can migrate.

---

## Drill mode

### Drill spar mode  *(V1)*
Drill mode currently spawns friendly blocks only. Add an "Enemy" toggle
when spawning so the player can stand up a second force and command
both. No AI involved — the player controls every block on the field via
selection. If a friendly block and an enemy block come into contact a
`MeterGame` triggers as it would in Runner / Campaign. This is the
sandbox for testing the formation counter-matrix and command gating
without needing the full Campaign scenario layer.

### Drill command — remaining visual / movement work

The data layer is landed: every command in the period manual has a state
transition in `BlockRules` and a routed handler in `Block.Issue`. What's
left is **visible behaviour**, which mostly belongs to the animation
suite (see Engineering). What's still cheap to add here in Drill code:

- **Auto-dressings** — post-step a soldier forward into any hole in the
  rank-in-front after facing changes. Pure slot bookkeeping.
- **Wheeling pivot semantics** — currently a wheel just spins the whole
  block. Real wheeling pivots about one corner (or the midst). Needs the
  outside-file soldiers to traverse a wider arc than the inside-file.
- **Doubling visual** — half-files and ranks visibly rearrange. Today
  they only flip a (currently unused) state flag.
- **Countermarch choreography** — front rank routes through the
  formation to become the new rear. Three ground-keeping variants
  determine where the block ends up.
- **Form Circle** — outer ring at Charge for Horse, interior at Charge
  Your Pike. Currently just a flat posture flag.

All of these can wait until the animation suite is in flight — the data
layer correctly gates and routes them, so the UI can be wired without
visuals catching up first.

### Categorised command palette (submenu UI)
The current flat horizontal bar won't scale past ~12 buttons. Restructure
into a two-level menu:

**Top-level bar** (always visible): Halt, Forward March, Reform, and one
category-opener per group below.

**Categories (submenus):** Postures, Distancing, Facings, Doublings,
Filing, Countermarch, Wheel. Each opens a submenu replacing the bar's
contents. Selecting a sub-option fires the command and auto-collapses
back to top-level. A Back / Esc cancels without selecting.

**Data model:**
- New `DrillCommandGroup` enum.
- Extend `DrillCommandPanel.Entry` with a `Group` field; flag a few as
  `IsTopLevel` (Halt, Forward March, Reform).
- `DrillCommandPanel.CurrentGroup` (nullable). `null` = top-level;
  non-null = filtered submenu. Issuing a command resets to `null`.

**Keyboard shortcuts:** sequential / RTS-style (`P` opens Postures,
second key picks the posture). Desktop only; touch users tap-tap. Both
paths converge on `Block.Issue(command)`.

**Open design questions:**
- Distancing directional variants — nested third tap or modifier toggle?
- Doublings is the busiest submenu (~13 buttons). Split into "Doublings"
  + "Recovers", or accept the density?
- Countermarch & Wheeling are historically two-part orders. Model as two
  presses, or auto-execute "prepare" on next frame?

---

## Campaign mode — V1 (must ship)

### Save / load system
`PlayerPrefs` only holds customisation colours today. Campaign needs
real serialisation for: owned kit (army-wide tier), money (Pay &
Plunder), battle history, unlocked content. JSON to a local file is the
straw-man — fast to ship and trivially diff-able when debugging.

- Include a top-level `save_version` int from the very first save so
  V2 (recruitment, per-block kit, captains) can migrate forward.
- **No mid-battle save.** Save points are between engagements only.
- Save-slot UI: 3 slots is plenty for a hobby project.

### Scenario / battle-definition system
One generic `Battle` scene loads a `BattleScenario` ScriptableObject —
no scene per battle. A scenario defines: map / terrain layout, friendly
force composition, enemy force composition, victory conditions, weather,
time of day, historical context blurb. Scene swap is dynamic — pick a
scenario, the same scene mutates to host it. Pairs with the **Linear
campaign progression** item: the campaign is a sequence of
`BattleScenario` assets.

### Linear campaign progression
Story arc follows the Covenanter timeline:
Newburn Ford (1640) → Marston Moor (1644) → Tippermuir (1644) →
Auldearn (1645) → Philiphaugh (1645) → Preston (1648) → Dunbar (1650) →
Worcester (1651). Linear progression for V1; branching deferred to V2.
Affects save format and Quartermaster gating.

### Multi-block field battles (architecture)
- Each engaged block owns its own `MeterGame` instance.
- NPC blocks self-fight via weighted-random AI.
- Player input routes to the **selected** block — same selection paradigm as Drill
  (multi-select honoured; mash routes to all selected engaged blocks).
- HUD: floating mini-meter per engaged block + big meter for the selected one.

### Formation counter-matrix
Defender formation × attacker type modifies the engaged block's
MeterGame parameters. Table in
`c:/Users/Andrew/.claude/projects/c--git-PikePush/memory/project_campaign_combat.md`.

### Enemy tactical AI (scripted-per-scenario)
For V1, enemy block behaviour is **scripted by the scenario**, not
emergent. Each `BattleScenario` carries an enemy command timeline
(simple list of `{time, blockId, DrillCommand}` entries) plus a few
reactive triggers (e.g. "when player closes within 30m, issue Charge
Your Pike"). Re-uses existing `Block.Issue(command)` plumbing — zero
new combat code.

If the player cheeses a script by manoeuvring unexpectedly, that's on
them. V2 (post-V1) can layer a state-machine on top for genuinely
reactive enemies. Don't gold-plate this — period battles followed
scripted plans anyway.

### Difficulty curve (historically anchored)
Difficulty is per-scenario, not a global slider. The campaign arc maps
to historical outcomes:
- **Early battles** (Newburn Ford, Marston Moor side, Tippermuir) — the
  Covenanters won against the odds. Tuned **easy**: formation mistakes
  forgivable, AI conservative, kit gap manageable.
- **Mid battles** (Auldearn, Philiphaugh) — mixed. Tuned **medium**.
- **Late battles** (Preston, Dunbar) — punishing. Tuned **hard**.
- **Worcester (1651)** — historically a Covenanter defeat. Tuned
  **near-impossible**: tight time window, brutal AI, severe kit gap. Slim
  chance to win. If the player *does* win, end-game screen acknowledges
  the alt-history with a cheeky "You changed history" beat (still ships
  the historical defeat ending as canon).

Implementation lives on the `BattleScenario` ScriptableObject:
`DifficultyTuning` sub-asset with MeterGame multipliers, enemy AI
aggressiveness, time limits.

### Army-wide loot upgrades (V1 substitute for regimental roster)
V1 has no recruitment, captains, or per-block customisation. Loot from
defeated enemies upgrades the **whole army's kit tier** for the next
battle (e.g. unlock Tier 2 → all blocks render at Tier 2). Kit visuals
applied uniformly via `PikemanCustomizer`. Regimental roster mechanics
(per-block captains, recruit slots, named soldiers) are V2.

### Command gating during engagement
Extends Drill-mode gating: in `Engaged` state, allow only `Halt`. Allow
`Prepare for Horse` mid-engagement if cavalry is incoming.

### Read-the-battlefield layer
Player must see what's coming to pick the right formation. Options:
overhead camera, scout/officer warning ("rider approaching from the
right flank"), spotter unit mechanic. Pick one and prototype.

### Kit progression
Five tiers (Levy → Trained Band → Regimented Foot → Veteran → Officer).
Visualised via Polytope Studio modular character sub-mesh toggles (same
machinery as the existing naked-skin disables). Stored as
ScriptableObjects under `Assets/PikePush/data/kit/`. Player picks owned
kit on a "Kit" screen.

### Quartermaster
New scene or modal panel. Two-currency system: **Pay** (regular, sparse)
+ **Plunder** (loot-derived). Regional QMs with different stock
(Edinburgh = muskets & coats, Highland muster = dirks & targes).
Grumbling flavour text intended.

### Loot tables by enemy
Defeated NPC blocks drop kit per ScriptableObject table keyed by enemy
type and engagement difficulty. Enemy types: English Royalist Foot,
New Model Army, Irish Confederate, Scots Royalist (Montrose), Cavalry.
Skirmish → bandoliers, bonnets. Major engagement → possible
back-and-breast.

### Historical battle tidbits
3–4 sentence pre-engagement intro: date, location, who's fighting, one
human detail. Roster matches the campaign progression above.

### Pause + game speed
Genre-standard for tactics: 1×/2×/4× + pause. Issue commands during
pause; observe consequences at higher speed. Currently real-time only.

**Hard rule:** speed-up is **disabled while the player is directly
driving a MeterGame** (selected block is engaged and receiving mash
input). Otherwise the player's own mash rate gets warped. UI: dim the
speed buttons during interactive MeterGame frames.

Affects every system that uses `Time.deltaTime` — central
`GameClock.TimeScale` rather than fiddling `Time.timeScale` directly,
because the latter would scale animations and audio in ways that feel
wrong.

### After-action report
End-of-battle screen: casualties (yours and theirs), loot acquired,
notable moments, money earned (Pay & Plunder split). Closure on the
engagement. Hooks into save and Quartermaster.

### Audio — command drums
Period-authentic drum signals — the drum was the actual command medium
on the battlefield. Play a drum hit when a command is issued; layer a
**march cadence loop** when a block is moving. Tempo **increases as the
block closes on the enemy** (proximity-driven beats-per-minute). Public
domain drum samples for now; specific regimental samples can be sourced
later (Loudoun's may have its own).

### Audio — combat sounds
Pike clash on contact, musket reports (once pike-and-shot lands in V2),
push-of-pike grunts, soldier shouts on command.

### Audio — ambient / scene
Wind on terrain, distant battle hum on field scenes, menu underscore.
Keep light — combat sounds carry the load.

### Audio — UI feedback
Button clicks (themed parchment / wood), menu transitions, command
confirmation sting, victory / defeat stings.

### Glossary surface
The glossary itself is seeded — see [glossary/](glossary/). What's left:
hook it into the game as tap-to-define popovers on first occurrence of
a period term in any UI, and/or a dedicated info screen. Continue
adding terms as new content surfaces.

---

## Campaign mode — V2 (follow-up wave)

### Regimental roster
The full "manage your regiment" layer: per-block captains (named
historicals where possible), recruit slots, per-block customisation,
named soldier tracking (the file leader, the bringer-up, NCOs), assign
kit per-block instead of army-wide. Replaces the V1 army-wide loot
upgrades with a real roster UI. Hooks into [[recruit-replacement]] and
[[officers-leadership]].

### Morale
Per-block `Morale` stat. Modifies MeterGame drain rate (low morale → faster
drain). Determines rout threshold: meter → 0 isn't the only fail state;
high losses + low morale can break a block earlier. Affected by officer
presence, casualties, weather/fatigue, sustained pressure. Pure mechanic
unlock — no morale system, no wavering, no rallying.

### Casualties / unit strength
Per-block `Strength` (current soldier count). Decrements on contact and
volley exchange. Below threshold = forced rout regardless of meter. Empty
file slots visible in the formation. Killed soldiers don't immediately
reappear — feeds the **Recruit replacement** mechanic.

### Pike-and-shot — musketeer units
Loudoun's was *pike-and-shot*, not pure pike. Musketeer files do volley
fire at range, separately from the contact MeterGame. Probably a
separate minigame (load/aim/fire timing) or a contribution to the
contact meter when the blocks close. Period drill source mentions
H/M2/M3/M5/B designations for shot positions in the file — we're
entirely pike-focused today.

### Officers / leadership
Per-block "officer slot". Named historicals (Loudoun himself at multiple
battles). Officer attached → morale boost, command effectiveness bonus.
Officer killed → morale shock, possible immediate waver. Hooks into Kit
progression — officer kit is rank-gated.

### Branching campaign decisions
V2 expands the V1 linear arc with choice points: stay with the main army
or detach to the Highlands? Stand at Tippermuir or fall back? Choices
unlock or lock specific battles, kit, and quartermasters.

### Terrain effects
High/low ground, woods, hedges, fords modify MeterGame parameters and
movement speed. Defending a hedge = significant drain reduction.
Down-hill charge = fill rate boost. Period battles cared about this
deeply.

### Cavalry
Friendly cavalry as a player-controllable unit type (currently cavalry
exists only as enemies in the counter-matrix). Different control
paradigm — gallop, charge, melee. Hits the counter-matrix from a new
angle.

### Recruit replacement
Soldiers die in battle; replenish between engagements at Pay cost.
Green recruits have lower stats; trained recruits cost more. Ties
into Kit progression (you arm new recruits at whatever tier you can
afford).

### Cosmetic favours / bonnet badges
Per-battle cosmetic unlocks applied to the modular character (e.g.
sprig of oak for Worcester). Pure visual; no stat effect. Polish.

### Weather & fatigue
- Wet powder → musket effectiveness drops (depends on pike-and-shot).
- Fatigue → mash-effectiveness drops over the engagement (depends on morale).
- Battles where it'd matter: Marston Moor (thunderstorm), Dunbar (dawn fog).

### Replay sharing
Record a battle, share with the regiment. Async community challenges
("can you hold the line at Dunbar?"). Genuinely interesting hook for
the re-enactment crowd.

---

## Campaign mode — V3 (later)

### Localisation
Multi-language support. Scots dialect for flavour text would be a nice
touch but isn't critical.

---

## Engineering

### Soldier animation suite
Pikemen currently always run in place (`1H@RunForward`) because that's
the only controller in the project. Need a proper animation set:
- **Idle** (the default when halted)
- **Marching** (forward / cadence-locked)
- **Pike postures** — one clip per posture (Order, Advance, Shoulder,
  Charge, Port, Low Port, Charge for Horse, Form Circle, Trail,
  Shorten/Halve)
- **Hit / recoil / death** (eventually, for casualties)
- **Brace transition** (replace the current Y-scale crouch placeholder)

Could come from another Kevin Iglesias pack or be hand-authored. Either
way it's the single biggest visual lift this project will take.

### Centralise the runner's pikeman customisation
`MainGame.Start()` has inline `HatMaterial.color` + `_CLOTH4COLOR` code
that duplicates `PikemanCustomizer.Customize(pikeman, hatMaterial)`.
Migrate the runner onto the customizer. Single source of truth, zero
behaviour change.

### Tests
Reinstate test coverage. Previous scaffolding was removed because Unity 6
asmdefs can't reference `Assembly-CSharp`. Workaround: move the user
source under an asmdef (`PikePush.Runtime`) so a `PikePush.Tests` asmdef
can reference it cleanly. Cover at least:
- `Block.AllowsCommand(...)` predicate (formation-gating logic)
- `MeterGame` fill/drain math with various counter-matrix modifiers
- `BattleScenario` deserialisation round-trip
- Save round-trip (write → read → equal)

Edit-mode tests for pure logic; Play-mode tests only where the
behaviour needs the engine loop.

### KISS / DRY refactor pass (running, never "done")
A standing item — not a one-shot task. When a system gains a second
consumer, centralise it then and there rather than deferring. Examples
already on the radar:
- Pikeman customisation is consumed by Runner, Drill, and (incoming)
  Campaign — see [[centralise-the-runners-pikeman-customisation]].
- `MeterGame` will gain N-instance multi-block use; current single-block
  assumptions need fishing out before they bite.
- `BlockSelector` becomes multi-select; the existing single-block API
  needs to disappear, not gain a parallel path.

Carry duplication only when the second consumer hasn't materialised
yet.

### Polytope Pikeman prefab — re-sync on Asset Store updates
`Pikeman.prefab` is a full-copy of `PT_Male_Modular_Free_Pack` with
overrides baked in (not a variant). If the source pack is ever updated,
the changes won't propagate. Run the `PikemanGenerator` editor menu
item to re-extract from `Game.unity`'s Formation if needed.

### docs/playing/drill.md is out of date
The command table on the public Drill mode docs page lists 5 commands
(`Halt`, `ForwardMarch`, `Faces`, `Orders`, `PrepareForHorse`) — the
real `DrillCommand` enum has 10+, with the Faces split into
right/left/about-L/about-R and the Orders split into
Open/Close/Closest. Update the page when the command system stabilises,
and link out to [glossary/drill-commands.md](glossary/drill-commands.md)
for the period reference.
