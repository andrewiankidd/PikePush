# Scripts Overview

29 C# scripts total, all under [`src/Assets/PikePush/scripts/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush/scripts) (plus one editor-only script under `src/Assets/Editor/`). Organised by folder and namespace.

## Layout

```
scripts/
├── controls/                       (PikePush.Controls)
│   ├── ControlsManager.cs
│   ├── ControlScheme.cs
│   ├── ControlInputs.cs
│   └── schemes/
│       ├── ButtonControlsSimple.cs
│       ├── TouchControlsSimple.cs
│       ├── GestureControlsSimple.cs
│       └── ExternalControlsSimple.cs
├── drill/                          (PikePush.Drill)
│   ├── DrillBootstrap.cs
│   ├── Block.cs
│   ├── Soldier.cs
│   ├── BlockSelector.cs
│   ├── DrillCommand.cs
│   ├── DrillCamera.cs
│   └── ui/
│       ├── DrillCommandPanel.cs
│       └── DrillCommandButton.cs
├── menus/                          (PikePush.Menus)
│   ├── MainMenuManager.cs
│   ├── ModeSelectOverlay.cs
│   ├── CustomizationMenuManager.cs
│   ├── SettingsMenuManager.cs
│   └── CreditsOverlay.cs
├── ui/                             (PikePush.UI)
│   ├── UIBuilder.cs
│   ├── UITheme.cs
│   ├── MessageBox.cs
│   └── MeterGame.cs
├── utils/                          (PikePush.Utls — note typo)
│   └── LogHelper.cs
├── MainGame.cs                     (PikePush)
├── IRPlayer.cs                     (PikePush)
├── PlatformTile.cs                 (PikePush)
└── PikemanCustomizer.cs            (PikePush)
```

> The `utils/` namespace is `PikePush.Utls` (missing the second `i`) — not a typo worth fixing unilaterally because the rename would touch every script.

## Namespace map

| Namespace | Folder | Purpose |
|-----------|--------|---------|
| `PikePush` | (root) | Core runner gameplay — `MainGame`, `IRPlayer`, `PlatformTile`, `PikemanCustomizer`. |
| `PikePush.Controls` | `controls/`, `controls/schemes/` | Input abstraction + the four concrete schemes. |
| `PikePush.Drill` | `drill/` | Drill mode runtime — blocks, soldiers, commands, camera, selector. |
| `PikePush.Menus` | `menus/` | Scene-level menu managers (main, mode-select overlay, customize, settings, credits). |
| `PikePush.UI` | `ui/` | Cross-mode UI primitives — UGUI factory (`UIBuilder`), theme, message box, meter-combat. |
| `PikePush.Utls` | `utils/` | Logging helper. |

## Key classes

### Core (Runner)

- **[`MainGame`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/MainGame.cs)** — runner mode master. Owns the tile pool, the score, the difficulty curve, and the fight-state coroutine.
- **[`IRPlayer`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/IRPlayer.cs)** — player movement (jump, crouch, strafe). Constants for gravity/jump height live here as `static` fields so other systems can read them.
- **[`PlatformTile`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/PlatformTile.cs)** — single recyclable scroll tile. Start/end markers + an `obstacles[]` array; `ActivateRandomObstacle()` flips one on.
- **[`PikemanCustomizer`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/PikemanCustomizer.cs)** — static utility. Reads colours from `PlayerPrefs`, applies to a pikeman GameObject. Used from both `MainGame` and Drill spawn paths.

### Controls

- **[`ControlsManager`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlsManager.cs)** — singleton-like router. Owns the registry of scheme class names, instantiates/enables them, and exposes `InputCheck()` which OR-combines flags from every active scheme.
- **[`ControlScheme`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlScheme.cs)** + **[`ControlInputs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlInputs.cs)** — base class + data container.
- **[`schemes/*Simple.cs`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush/scripts/controls/schemes)** — four concrete schemes (keyboard, buttons, touch, gesture-stub).

### Drill

- **[`DrillBootstrap`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillBootstrap.cs)** — builds the scene from code (lights, ground, camera, canvas, blocks).
- **[`Block`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Block.cs)** — formation of soldiers; owns the `Issue(DrillCommand)` switch.
- **[`Soldier`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/Soldier.cs)** — single unit; lerps to its slot.
- **[`DrillCommand`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCommand.cs)** — enum (Halt / ForwardMarch / Faces / Orders / PrepareForHorse).
- **[`BlockSelector`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/BlockSelector.cs)** — mouse-pick the active block.
- **[`DrillCamera`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/DrillCamera.cs)** — orbital pan/zoom rig.
- **[`DrillCommandPanel`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/ui/DrillCommandPanel.cs)** + **[`DrillCommandButton`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/drill/ui/DrillCommandButton.cs)** — auto-built command-panel UI.

### Menus

- **[`MainMenuManager`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/MainMenuManager.cs)** — the main menu scene logic.
- **[`ModeSelectOverlay`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/ModeSelectOverlay.cs)** — modal popup with Runner / Drill / Campaign.
- **[`CustomizationMenuManager`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/CustomizationMenuManager.cs)** — colour pickers, name, flag.
- **[`SettingsMenuManager`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/SettingsMenuManager.cs)** — touch-controls dropdown.
- **[`CreditsOverlay`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/CreditsOverlay.cs)** — scrollable credits with the third-party asset list.

### UI

- **[`UIBuilder`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/ui/UIBuilder.cs)** — static factory for canvases, buttons, panels, text, scroll views. Everything UI in the game routes through this — there are no canvas prefabs baked into scenes.
- **[`UITheme`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/ui/UITheme.cs)** — dark theme constants (colours + font sizes).
- **[`MessageBox`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/ui/MessageBox.cs)** — "Press Space to Begin", "Game Over", etc.
- **[`MeterGame`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/ui/MeterGame.cs)** — the await-able meter-combat minigame; `Task<bool> Show()` returns win/lose.

### Editor

- **[`PikemanGenerator`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/Editor/PikemanGenerator.cs)** — menu item that exports `Pikeman.prefab` from the runner scene. Keeps Runner and Drill in sync without manual duplication.

## Conventions

- **No DI / no service locator.** Cross-system references are either `static` (e.g. `IRPlayer.movementSpeed`), `[SerializeField]` wiring set in scene, or `GameObject.Find(...)` calls in `Awake()`.
- **`PlayerPrefs` everywhere** for persistence — no `JsonUtility`, no `ScriptableObject` saves, no `File.WriteAllText`.
- **Legacy `Input` class**, not the new Input System package. Trivial to swap if/when it matters.
- **All UI built from code** via `UIBuilder` — there are no Canvas prefabs in any scene.
- **No async / await chains** except `MeterGame.Show()` which uses `Task<bool>` + `await Task.Yield()` to let the meter loop drive itself.

## Logging

[`PikePush.Utls.LogHelper`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/utils/LogHelper.cs) wraps `Debug.Log` / `Debug.LogWarning` / `Debug.LogError` so we can prefix or silence later without touching call sites. Use these in preference to bare `Debug.Log(...)`.
