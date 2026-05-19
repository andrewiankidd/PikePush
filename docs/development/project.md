# Project Structure

PikePush is a Unity 6 (`6000.4.7f1`) project. The Unity assets and project files live under [`src/`](https://github.com/andrewiankidd/PikePush/tree/master/src); everything original to the game is under [`src/Assets/PikePush/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush). The repo root holds docs (README, CHANGELOG, this wiki under `docs/`), CI ([`.github/workflows/publish.yml`](https://github.com/andrewiankidd/PikePush/blob/master/.github/workflows/publish.yml)), the static website ([`.github/pages/`](https://github.com/andrewiankidd/PikePush/tree/master/.github/pages)), and source-only design assets (`assets/logo.psd`, `assets/screencap.gif`).

## Repo root

```
PikePush/
├── .github/
│   ├── pages/                  # Static website (deployed to gh-pages)
│   │   ├── index.html
│   │   ├── changelog.html      # Renders CHANGELOG.md
│   │   ├── docs.html           # This wiki, renders docs/*.md
│   │   └── logo.png
│   └── workflows/
│       └── publish.yml         # CI: build + release + publish web
├── assets/
│   ├── logo.psd                # Design source for the game logo
│   └── screencap.gif           # Promo GIF on README
├── docs/                       # This wiki
├── src/                        # Unity project
├── CHANGELOG.md
├── LICENSE                     # MIT (original code only)
└── README.md
```

`.gitmodules` declares the `toolbox` submodule — shared CI helpers.

## Unity project

```
src/
├── Assets/
│   ├── PikePush/               # All original game code + assets
│   │   ├── Scenes/             # 6 scenes (see below)
│   │   ├── scripts/            # 29 C# scripts — see Scripts Overview
│   │   ├── prefabs/
│   │   │   └── Pikeman.prefab  # Shared character prefab (runner + drill)
│   │   ├── materials/
│   │   ├── Textures/
│   │   │   └── logo.png        # In-game logo
│   │   └── shaders/
│   ├── Editor/
│   │   └── PikemanGenerator.cs # Menu item to regen Pikeman.prefab
│   ├── TextMesh Pro/           # Unity built-in
│   └── [third-party asset packs — see Credits]
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
└── ProjectSettings/
    ├── ProjectVersion.txt      # 6000.4.7f1
    ├── EditorBuildSettings.asset
    ├── TagManager.asset        # Tags: Finish, Fight
    └── [24 other Unity config files]
```

No `asmdef` files — the whole game compiles as one assembly. Fine at this size; revisit if compile times start to hurt.

## Scenes

All under [`src/Assets/PikePush/Scenes/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush), in build-index order:

| # | Scene | Purpose |
|---|-------|---------|
| 0 | `MainMenu.unity` | Play / Customize / Settings / Credits / Quit |
| 1 | `SettingsMenu.unity` | Touch-controls dropdown selector |
| 2 | `CustomizeMenu.unity` | Name, colours, flag preview |
| 3 | `FlagDraw.unity` | Flag drawing canvas (FreeDraw) |
| 4 | `Game.unity` | Runner mode |
| 5 | `Drill.unity` | Drill mode |

Mode selection is a literal `SceneManager.LoadScene(name)` — see [Modes](#modes).

## Third-party asset packs

These live under `src/Assets/` and are kept under their original folder names so updates from the Asset Store apply cleanly:

| Pack | Used for |
|------|----------|
| **Polytope Studio** | Modular medieval characters — the pikeman model + the `_CLOTH4COLOR` shader. |
| **Kevin Iglesias** | Character animations (idle, run, jump, crouch). |
| **FreeDraw** | Flag drawing canvas in `FlagDraw.unity`. |
| **FlexibleColorPicker** | Colour pickers in `CustomizeMenu.unity`. |
| **Low_poly_styled_rocks**, **Low_poly_styled_trees** | Environmental scatter for Runner mode tiles. |
| **VertexColorFarmAnimals** | Future wildlife / decoration. |
| **Wand and Circles** | VFX placeholders. |
| **controller_input_icons** | Button icons for future controller-input UI. |
| **TextMesh Pro** | Unity built-in text rendering. |

See [Credits](#credits) for license attributions.

## The Pikeman prefab

The pikeman model is used by **both** modes:

- Runner instantiates one as the player character.
- Drill instantiates `ranks × files` of them per block.

To keep them in sync without manual duplication, the editor tool [`PikemanGenerator`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/Editor/PikemanGenerator.cs) extracts a bereted soldier from `Game.unity`, strips its `Rigidbody`, `Collider`, and `IRPlayer` components, and exports it as `Pikeman.prefab` — preserving the asset GUID so the Drill scene's references stay valid. The menu item lives at **PikePush → Regenerate Pikeman Prefab from Runner**.

## Tags + layers

Only two custom tags, both used in Runner mode:

| Tag | Meaning |
|-----|---------|
| `Finish` | Collision triggers game over. |
| `Fight` | Collision triggers the meter-combat minigame. |

## Audio

Not implemented. `ProjectSettings/AudioManager.asset` exists at default values; no `AudioSource` / `AudioListener` setup; no audio assets imported. Reserved future work.
