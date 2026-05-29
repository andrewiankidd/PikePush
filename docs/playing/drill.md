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

A small **+ / −** panel in the top-right corner controls how many blocks
are on the field, clamped between **1 and 4**. New blocks spawn at a
sensible default position; the button greys out when the limit is hit.

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
