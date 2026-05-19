# Drill Mode

Tactical formation mode. You command **blocks** of pikemen — pick a block with the mouse, then issue orders from the command panel (or keyboard shortcuts) to halt, march, change facings, etc. No combat yet — it's pure parade-ground drill.

Scene: `Drill.unity` (build index 5). Bootstrap: [`DrillBootstrap.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillBootstrap.cs).

## Layout

When the scene loads, `DrillBootstrap` builds the world from code (no scene-baked geometry):

- Directional sun light + ambient fill
- A flat parade field
- Camera with orbital rig — see [`DrillCamera`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCamera.cs)
- A UGUI canvas with the command panel anchored top-right
- Some number of [`Block`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Block.cs) instances spawned at preset positions, each populated with `ranks × files` soldiers

## Blocks and soldiers

A **block** is a rectangular formation of [`Soldier`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Soldier.cs) units, each instantiated from the **Pikeman prefab** (the same character used in Runner mode — see [Project Structure](#project) for how the prefab is regenerated).

- Each soldier knows its **slot** in the block (rank + file index).
- On each frame the soldier lerps its world position + rotation toward the slot's world transform.
- When the block moves or turns, slots update; soldiers chase their slots smoothly — so a "right face" looks like a coordinated turn instead of a snap.

## Commands

Commands come from the [`DrillCommand`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCommand.cs) enum:

| Command | What it does |
|---------|--------------|
| `Halt` | Stop forward motion immediately. |
| `ForwardMarch` | Start advancing in the block's current facing. |
| `Faces` | Cycle the block through right / left / about-face turns. |
| `Orders` | Reissue / acknowledge the standing order (parade ack). |
| `PrepareForHorse` | Brace formation — reserved for future cavalry interaction. |

[`Block.Issue(DrillCommand cmd)`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Block.cs) is a switch over the enum; each case mutates the block's `targetFacing`, `marching` bool, etc. and the per-frame slot recomputation does the rest.

## UI

The command panel is built by [`DrillCommandPanel.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/ui/DrillCommandPanel.cs) at scene start. It iterates the `DrillCommand` enum and creates one [`DrillCommandButton`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/ui/DrillCommandButton.cs) per value — each button:

- Shows the command label + a keyboard hint (e.g. `H` for Halt).
- Dispatches the command to the currently-selected block when clicked **or** when its hint key is pressed.

## Selecting a block

[`BlockSelector`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/BlockSelector.cs) is a single MonoBehaviour that runs the click-to-select loop:

1. On every left-click, raycast from the cursor.
2. If the hit collider belongs to a `Block`, set it as the active block.
3. Subsequent command buttons / key presses target that block.

There's no multi-select today — exactly one active block at a time.

## Camera

[`DrillCamera`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCamera.cs) is a simple orbital rig:

| Input | Action |
|-------|--------|
| Middle mouse drag | Pan the focus point across the field. |
| Mouse wheel | Zoom in / out. |
| (left mouse) | Reserved for `BlockSelector` — does not move the camera. |

## Status

Drill is the newest mode (added in the May 2026 batch — see [Changelog](changelog.html)). The core picking + commanding + animated-formation loop works; it's not yet wired to anything resembling a campaign or win/lose state. Combat, casualties, cavalry charges, etc. are all design space for later.
