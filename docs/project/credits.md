# Credits

Original game code is **MIT-licensed** (`LICENSE` at the repo root). Bundled third-party assets keep their original licenses from Unity Asset Store / GitHub. This page is the canonical attribution list and mirrors the in-game **Credits** screen (driven by [`CreditsOverlay`](https://github.com/andrewiankidd/PikePush/blob/master/src/Assets/PikePush/scripts/menus/CreditsOverlay.cs)) — keep them in sync when packs are added or removed.

## Dedicated to

**The Earl of Loudoun's Regiment of Foote** — a 17th-century pike block re-enactment society.
[loudouns.org.uk](https://loudouns.org.uk/)

This game would not exist without their work keeping the drill, kit, and spirit of the period alive.

## Third-party assets (bundled in `src/Assets/`)

Each asset folder sits at its original Asset Store name so updates apply cleanly.

| Asset | Folder | Used for |
|-------|--------|----------|
| **Polytope Studio — Modular Medieval Peasants** | `Polytope Studio/` | Pikeman character model + the `_CLOTH4COLOR` shader the customizer drives. |
| **Kevin Iglesias — Basic Motions** | `KevinIglesias/` (or similar) | Character animations — idle, run, jump, crouch. |
| **FlexibleColorPicker** | `FlexibleColorPicker/` | RGB colour pickers in `CustomizeMenu.unity`. |
| **FreeDraw — Simple Drawing on Sprites and 2D Objects** | `FreeDraw/` | Flag drawing canvas in `FlagDraw.unity`. |
| **Low Poly Styled Rocks** | `Low_poly_styled_rocks/` | Tile decoration. |
| **Low Poly Styled Trees** | `Low_poly_styled_trees/` | Tile decoration. |
| **Vertex Color Farm Animals** | `VertexColorFarmAnimals/` | Reserved — future decoration. |
| **Wand and Circles** | `Wand and Circles/` | VFX placeholders + selection feedback. |
| **Controller Input Icons** | `controller_input_icons/` | Button icons for future controller-input UI. |
| **TextMesh Pro** | `TextMesh Pro/` | Unity built-in — text rendering. |
| **SimpleInput** | `SimpleInput/` | Touch input plugin. |

## Engine

Built with **Unity** (`6000.4.7f1`).

## CI / tooling

| Project | Purpose |
|---------|---------|
| [`game-ci/unity-builder`](https://github.com/game-ci/unity-builder) | Headless Unity multi-platform builds. |
| [`JamesIves/github-pages-deploy-action`](https://github.com/JamesIves/github-pages-deploy-action) | Pushes the assembled site to `gh-pages`. |
| [`robinraju/release-downloader`](https://github.com/robinraju/release-downloader) | Pulls the WebGL zip from the GitHub Release in `publish_web`. |
| [`marked`](https://github.com/markedjs/marked) | Markdown renderer used by [`docs.html`](https://github.com/andrewiankidd/PikePush/blob/master/.github/pages/docs.html). |

## License summary

- **PikePush source code** (everything under `src/Assets/PikePush/`, `.github/`, `docs/`): MIT.
- **Bundled third-party packs**: each pack's original license; see `LICENSE` and the individual folders.

If you're forking and intend to redistribute commercially, double-check the Unity Asset Store licenses for each pack — most are *per-seat* and **not transferable**, so you'll need your own license to ship them in a derivative project.
