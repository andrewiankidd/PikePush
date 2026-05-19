# Customization — Soldier & Flag

Your pikeman has three customisable things: **two clothing colours**, **a name**, and **a custom flag** you can draw freehand. All of it lives in `PlayerPrefs` — there's no save file, no account, nothing leaves the machine.

UI entry point: main menu → **Customize** → loads `CustomizeMenu.unity` ([`CustomizationMenuManager.cs`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/CustomizationMenuManager.cs)).

## What gets persisted

| Key | Type | Default | Used for |
|-----|------|---------|----------|
| `Name` | string | `"Scotland"` | Player name shown in UI. |
| `ColourOne` | RGB string (`"r,g,b"`) | `"93,165,255"` (light blue) | Hat / beret material. |
| `ColourTwo` | RGB string | `"145,156,168"` (gray) | Torso cloth (shader prop `_CLOTH4COLOR`). |
| `ColourThree` | RGB string | `"206,207,97"` (yellow) | Reserved — loaded into the picker but **not applied visually** yet. |
| `FlagImage` | Base64 PNG | bundled default sprite | Your hand-drawn flag. |

If a key is missing, the defaults above kick in. To wipe everything (factory reset) just delete the project's `PlayerPrefs` (macOS: `~/Library/Preferences/unity.<Company>.<Product>.plist`; Windows: `HKCU\Software\<Company>\<Product>`; WebGL: clear site data; Android/iOS: app data).

## Colour picker

The customize screen uses the third-party [FlexibleColorPicker](https://github.com/AjaxGb/Unity-Flexible-Color-Picker) asset. Each of the three colour buttons opens a picker, you confirm, and the new RGB triple is serialised back into `PlayerPrefs` as a comma-separated byte string.

Application happens through [`PikemanCustomizer.Customize(GameObject pikeman)`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/PikemanCustomizer.cs) — called from `MainGame.Start()` for the Runner pikeman, and at spawn time for Drill soldiers. Roughly:

```csharp
// Hat colour
Material hatMaterial = pikeman.transform.Find("Hat").GetComponent<Renderer>().material;
hatMaterial.color = ParseRgb(PlayerPrefs.GetString("ColourOne"));

// Torso cloth colour (uses a custom shader property)
var torso = pikeman.transform.Find("PT_Male_Peasant_01_upper").GetComponent<Renderer>();
torso.material.SetColor("_CLOTH4COLOR", ParseRgb(PlayerPrefs.GetString("ColourTwo")));
```

The hat is a plain `_BaseColor`; the torso is a custom shader from the Polytope Studio peasant pack — that's why it needs the `SetColor` property call instead of `material.color =`.

## Drawing a flag

Hitting **Customize Flag** loads `FlagDraw.unity` (build index 3), a dedicated scene wired around the [FreeDraw](https://assetstore.unity.com/packages/tools/painting/free-draw-simple-drawing-on-sprites-and-2d-objects-113397) asset. You paint on a small canvas with brush + colour controls; "save" reads the canvas pixels, encodes the resulting `Texture2D` as PNG, base64-encodes the bytes, and stores them in `PlayerPrefs["FlagImage"]`.

When the customise menu loads, it decodes that base64 back into a `Texture2D` and slots it into the flag preview sprite. The flag isn't currently rendered in-game during Runner or Drill mode — it's stored, displayed in the menu, and ready for whenever it's wired into the gameplay.

## Name

`NameInput` is a TextMeshPro input field. On change it writes to `PlayerPrefs["Name"]`. Today nothing in the game reads it back beyond the menu — it's a stub for future use (leaderboards, drill formations labelled by commander, etc.).

## What this does *not* do

- No preset palettes / saved looks — every change is freeform and overwrites the last.
- No multiple loadouts — one active pikeman at a time.
- No validation on the flag dimensions; whatever you paint gets shoved into `PlayerPrefs`. (Big drawings = big base64 string = slow PlayerPrefs writes. Don't go overboard.)
