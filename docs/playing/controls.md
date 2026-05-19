# Controls

PikePush has four interchangeable input schemes that can run alone or alongside each other. Keyboard is **always on**; the touch/button/gesture scheme is picked from the Settings menu and persists between sessions.

## The four schemes

| Scheme | Implementation | When it's useful |
|--------|----------------|------------------|
| **Keyboard** (`ExternalControlsSimple`) | Legacy `Input.GetKey(...)` polling for W/A/S/D, Space, Escape | Desktop play. Always enabled. |
| **Buttons** (`ButtonControlsSimple`) | Unity UGUI buttons with `OnPointerDown` / `OnPointerUp` event hooks | Mouse-only play, or on-screen D-pad on mobile. |
| **Touch** (`TouchControlsSimple`) | Touchscreen taps — same `OnPointerDown` interface as buttons | Mobile builds (Android, iOS). |
| **Gesture** (`GestureControlsSimple`) | Stub gesture detector (`SwipeStart` / `SwipeEnd`) | Mobile builds; currently a scaffold for future swipe support. |

All four implement a common base — [`ControlScheme`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlScheme.cs) — and expose state through [`ControlInputs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlInputs.cs), a class with a handful of bool properties (`Left`, `Right`, `Up`, `Down`, `Space`, `Escape`) and a reflection-based indexer so the manager can read them by name.

## Key bindings (Keyboard)

| Action | Key |
|--------|-----|
| Strafe Left | `A` |
| Strafe Right | `D` |
| Jump | `W` |
| Crouch | `S` |
| Fight / Confirm | `Space` |
| Back / Quit | `Escape` |

In the [meter-combat](#runner) minigame, hold `Space` to fill the bar; release to drain.

## Picking a scheme

1. Main menu → **Settings**
2. The "Touch Controls" dropdown lists every scheme registered in [`ControlsManager`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlsManager.cs).
3. Pick one. Selection persists in `PlayerPrefs["TouchControlsDropdown"]` as the dropdown index.
4. Keyboard stays active either way — the manager hard-enables `ExternalControlsSimple` on every scene load.

## How input gets routed

[`ControlsManager.Awake()`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/controls/ControlsManager.cs) finds every scheme GameObject in the scene, disables all of them, then enables only the user-selected scheme (plus keyboard). Each scheme owns a `ControlInputs` instance.

When the game asks "is the player jumping?", it calls `ControlsManager.InputCheck()` — which polls every active scheme, OR-combines their state into a `Controls` bitflag enum, and returns it. So if you're holding `W` AND tapping a touch-up button at the same time, both fire and the higher-level code just sees "Up pressed".

```csharp
[System.Flags]
public enum Controls {
    Left   = 1,
    Right  = 2,
    Up     = 4,
    Down   = 8,
    Space  = 16,
    Escape = 32,
}
```

## Adding a scheme

If you want a new input source (e.g. gamepad), create a new `MonoBehaviour` under [`scripts/controls/schemes/`](https://github.com/andrewiankidd/PikePush/tree/master/src/Assets/PikePush/scripts/controls/schemes), inherit from `ControlScheme`, fill out its `ControlInputs`, and add the class name to the `controlSchemes` string array in `ControlsManager`. The Settings dropdown picks it up automatically.
