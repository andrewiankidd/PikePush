# PikePush — Docs

PikePush started out as a dumb lil ten-minute runner game built in Unity. That loop is still here — it's now the **Runner** mode — but scope crept and there's also a **Drill** mode for commanding pikeman formations, with a third **Campaign** slot reserved for whatever comes next. Unity 6 (`6000.4.7f1`), four input schemes, full character + flag customisation, all wired through `PlayerPrefs` — no save files, no accounts, no telemetry.

This wiki is for **players who want to know what the buttons do**, **modders who want to poke at the project**, and **future-me who keeps forgetting what `_CLOTH4COLOR` does**.

## What's in here

### Playing

- [Controls](#controls) — the four input schemes (Keyboard, Buttons, Touch, Gesture), key bindings, how to pick one.
- [Modes](#modes) — Runner, Drill, Campaign (placeholder). What each is, how to launch one.
- [Runner Mode](#runner) — the arcade dash. Scoring, obstacles, the meter-combat fight, the difficulty curve.
- [Drill Mode](#drill) — command formations of pikemen. Blocks, soldiers, commands, the orbital camera.

### Customization

- [Soldier & Flag](#customize) — colours (hat / torso / accent), name, and the hand-drawn flag.

### Development

- [Project Structure](#project) — repo layout, scenes, third-party packs.
- [Scripts Overview](#scripts) — the 29 C# scripts grouped by namespace.
- [Build & CI](#build) — Unity multi-platform build matrix, GitHub Pages deploy.

### Project

- [Credits](#credits) — third-party assets, fonts, licenses.

### Glossary

- [Period Terms](#glossary-terms) — vocabulary used across the game: kit, ranks, formation structure, currencies, mechanics.
- [Drill Commands](#glossary-drill-commands) — the pike drill manual: every command, how to perform it, when it's used.

## At a glance

| Thing | Value |
|-------|-------|
| Engine | Unity `6000.4.7f1` |
| Pipeline | Built-in (no URP/HDRP) |
| Input | Legacy `Input` class (not New Input System) |
| UI | UGUI, built dynamically via [`UIBuilder`](#scripts) — no canvas prefabs in scenes |
| Persistence | `PlayerPrefs` only |
| Scenes | 6 — MainMenu, SettingsMenu, CustomizeMenu, FlagDraw, Game, Drill |
| Modes | Runner (live), Drill (live), Campaign (placeholder) |
| Platforms | Windows / macOS / Linux / Android / iOS / WebGL |

## Source layout

The Unity project lives under [`src/`](https://github.com/andrewiankidd/PikePush/tree/master/src). Everything original to PikePush is under [`src/Assets/PikePush/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush) — scripts, scenes, prefabs. Third-party asset packs (colour picker, drawing tool, character models, animations) sit alongside under `src/Assets/`. See [Project Structure](#project) for the full tree.
