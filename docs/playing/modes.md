# Modes

PikePush ships with three modes today. Two are playable, one is a placeholder for future work. Mode selection happens on the main menu via the **Play** button, which pops a [`ModeSelectOverlay`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/ModeSelectOverlay.cs).

| Mode | Status | Description | Scene |
|------|--------|-------------|-------|
| **Runner** | Playable | The arcade dash. Auto-running pikeman dodges obstacles, fights enemies in a quick meter minigame. See [Runner Mode](#runner). | `Game.unity` |
| **Drill** | Playable | Tactical formation mode. Command blocks of soldiers with halt / march / facings / orders. See [Drill Mode](#drill). | `Drill.unity` |
| **Campaign** | Disabled | Placeholder for a future story / scenario mode. The button exists in the picker but is greyed out (`enabled: false`). | — |

## How mode selection works

When the player hits **Play** on the main menu, [`MainMenuManager.GameStart()`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/MainMenuManager.cs) instantiates an overlay and calls `Show()`. The overlay builds three buttons via the shared [`UIBuilder`](#scripts) factory:

```csharp
BuildModeButton("Runner",   "the arcade dash",     "Game",  enabled: true);
BuildModeButton("Drill",    "command the block",    "Drill", enabled: true);
BuildModeButton("Campaign", "coming soon",          null,    enabled: false);
```

Each enabled button calls `SceneManager.LoadScene(sceneName)` — that's the entire mode-routing logic. There's no enum, no registry, no factory; the picker is a literal switchboard.

## Adding a new mode

1. Create the scene under [`src/Assets/PikePush/Scenes/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush) and register it in `ProjectSettings/EditorBuildSettings.asset` so it gets built.
2. Add a `BuildModeButton(...)` call in `ModeSelectOverlay.cs` with the scene name.
3. Done. No data layer or persistence to touch — modes don't know about each other.

The flip side is the cost of intentionally **not** having a mode registry: a Campaign mode that's gated on unlocks, or shares assets across modes, would need a real layer — today there is none.
