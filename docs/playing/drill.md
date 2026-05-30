# Drill Mode

Tactical formation mode. You command **blocks** of pikemen on a parade
field — pick blocks with the mouse, then issue orders. No combat yet —
this is the sandbox for the campaign tactical layer.

Scene: `Drill.unity` (build index 5). Bootstrap:
[`DrillBootstrap.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillBootstrap.cs).

## Layout

When the scene loads, `DrillBootstrap` builds the world from code (no
scene-baked geometry):

- Directional sun light + ambient fill
- A flat parade field
- Camera with orbital rig — see
  [`DrillCamera`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCamera.cs)
- A UGUI canvas with the command panel anchored bottom-centre and a
  block-count panel top-right
- Between 1 and 4 [`Block`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Block.cs)
  instances spawned at preset positions, each populated with `ranks ×
  files` soldiers

## Blocks and soldiers

A **block** is a rectangular formation of [`Soldier`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Soldier.cs)
units, each instantiated from the **Pikeman prefab** (the same character
used in Runner mode).

Each soldier knows its **slot** in the block (rank + file index) and
lerps its world position + rotation toward the slot's world transform
every frame. When the block moves or turns, slots update; soldiers chase
their slots smoothly — so a "right face" reads as a coordinated turn
rather than a snap.

A block carries the state that drives every command:

- **Posture** — Order, Advance, Charge, Charge for Horse, etc.
- **Spacing** — Closest, Close, Order, Open (and the wider variants)
- **Wheeling / marching** — whether the block is in motion and rotating

Posture and spacing changes affect what other commands are legal —
e.g. while braced for cavalry the block is committed to its stance.

## Adding and removing blocks

Two **+ / −** panels in the top-right corner control how many blocks
are on the field per faction:

- **Friendly** — 1 to 4 blocks. Always at least one.
- **Enemy** — 0 to 4 blocks. Optional; stand up an opposing force to
  spar against.

Friendly blocks spawn facing north; enemy blocks spawn facing south.
Each side has its own colour palette (Covenanter blues / mustards on
your side, Royalist reds / burgundies on theirs).

## Spar mode

The moment you spawn any enemy blocks, drill mode is in **spar mode** —
the field has two opposing forces and you can command either by clicking
to select. There's no AI; the player controls every block on the field.

When a friendly block and an enemy block come into contact, both halt
and an **Engagement** opens — two parallel push meters, one per block.
The top-left overview lists every active engagement with both sides'
meter percentages.

| Input | Effect |
|-------|--------|
| Hold **Space** | Push for every block in your current selection. |
| (no input on a block) | That block's meter drains. |

Drill spar mode has no AI, so a block whose owner stops paying attention
to it will drain. To push two engagements at once you have to either
multi-select (shift-click both sides of a fight, or one block from each
of two fights) or cycle selection between them.

Once engaged, a block is **locked in** — Halt, Posture changes,
Spacing changes, and Reform are still legal (and the spacing dial is
how you outmuscle the other side via the counter-matrix), but Forward
March, Wheel, Facings, Doublings, and Countermarch all grey out until
the engagement resolves.

First meter to fill (Won) declares its block the winner; the loser
breaks and is removed from the field. First meter to drain to zero
(Lost) declares the *other* block the winner. The block-count panels
refresh as the field empties out.

### Formation matters

Spacing modifies each side's fill rate during a push:

| Spacing | vs Pike Push |
|---------|--------------|
| Open Order | −10% (low density) |
| Order | baseline |
| Close Order | +15% (push power) |
| Closest Order | +25% (max push) |

So forming **Closest Order** mid-engagement is the canonical move — your
meter fills faster than the enemy's. The multiplier recomputes every
frame, so you can swap spacings on the fly. The full matrix (including
cavalry, where Bracing for Horse swings the other way) is in
[`CounterMatrix`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/combat/CounterMatrix.cs).

## Commands

The full period-authentic drill manual is in
[glossary/drill-commands.md](../glossary/drill-commands.md). The drill
mode UI surfaces a working subset on the command bar — Halt, Forward
March, Faces, Orders, Charge for Horse, Advance Pike, Reform — with the
rest reachable via the categorised palette (coming).

Buttons **grey out** when the current state of every selected block
won't accept the command (e.g. spacings while braced for cavalry, faces
while at Closest Order).

Commands come from the [`DrillCommand`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCommand.cs)
enum; gating is centralised in
[`BlockRules.AllowsCommand`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/BlockRules.cs)
so it can be reused in Campaign mode.

## Selecting blocks

[`BlockSelector`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/BlockSelector.cs)
supports multi-block selection:

| Input | Action |
|-------|--------|
| Left click on a block | Select only that block. |
| Shift + left click on a block | Add or remove the block from the selection. |
| Left click on empty ground | Clear the selection. |
| Esc | Clear the selection. |

Any command you issue applies to every block in the selection. The
command bar disables a button if **any** selected block can't currently
accept that command.

## Camera

[`DrillCamera`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCamera.cs)
is a simple orbital rig:

| Input | Action |
|-------|--------|
| Middle mouse drag | Pan the focus point across the field. |
| Mouse wheel | Zoom in / out. |
| (left mouse) | Reserved for `BlockSelector` — does not move the camera. |

## Status

Drill mode is currently the sandbox for the upcoming Campaign tactical
layer. The data layer (commands, gating, multi-select) lives here first
so it can be exercised before campaign battles need it. Combat,
casualties, cavalry, and the full categorised command palette are
tracked on the [backlog](../backlog.md).
