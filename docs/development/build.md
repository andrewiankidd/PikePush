# Build & CI

Unity project builds via [`game-ci/unity-builder@v4`](https://github.com/game-ci/unity-builder) for 6 platforms in parallel, each one uploaded to the GitHub Release for the commit. A separate `publish_web` job pulls the WebGL build, stages the website, and deploys to GitHub Pages.

CI lives in [`.github/workflows/publish.yml`](https://github.com/andrewiankidd/PikePush/blob/master/.github/workflows/publish.yml).

## Triggers

- `push` to `master`
- `pull_request` against `master`
- `workflow_dispatch` (manual)

There are **no path filters** — every push runs the full matrix. The Unity caches keep cold time down, but a docs-only change still spins all six builds. Path filters are an open improvement.

## Pipeline

```
prepare_release ──┬── Building for OSX 🍎       ──┐
                  ├── Building for Windows 🪟    ──┤
                  ├── Building for Linux 🐧     ──┼── publish_web ── pages build & deployment
                  ├── Building for WebGL 🌐     ──┤
                  ├── Building for Android 🤖   ──┤
                  └── Building for iOS 🍏        ──┘
```

### `prepare_release`

Creates either a draft release (non-`master` branches) or a real release (`master`) using `actions/create-release@v1`. The release upload URL is exposed as a job output so every build can attach its zipped binary.

### `build` matrix

Six target platforms, all built on `ubuntu-latest` via Unity Linux Editor:

| Target | Asset name | Notes |
|--------|------------|-------|
| `StandaloneWindows64` | `StandaloneWindows64-release.zip` | |
| `StandaloneOSX` | `StandaloneOSX-release.zip` | Intel binary; not signed. |
| `StandaloneLinux64` | `StandaloneLinux64-release.zip` | |
| `WebGL` | `WebGL-release.zip` | Consumed by `publish_web`. |
| `Android` | `Android-release.zip` | Unsigned APK. |
| `iOS` | `iOS-release.zip` | Xcode project bundle, not a built `.ipa`. |

Each job:

1. `actions/checkout@v3` with `lfs: true`.
2. `actions/cache@v4` keyed on `src/Library-<platform>-<unityVersion>` — saves cold time on rebuilds.
3. Creates `Build/`, runs `game-ci/unity-builder@v4` with `buildMethod: UnityBuilderAction.Builder.BuildProject`.
4. Zips `Build/<platform>/` into `Build-<platform>.zip`.
5. Uploads to the prepare-release URL with `actions/upload-release-asset@v1`.

Unity version is parameterised at the top of the workflow:

```yaml
env:
  UNITY_MAJOR_VERSION: 6000
  UNITY_MINOR_VERSION: 4
  UNITY_PATCH_VERSION: 7f1
```

Bump these together to upgrade the editor. The `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` secrets are required (personal/plus license).

There's a commented-out `request_alf` / `acquire_ulf` pair earlier in the workflow for fully-automated license activation via `unity-license-activate` and `actions-set-secret`. Left in as documentation; not wired today because the license is set manually.

### `publish_web`

Only runs on `master` after all `build` jobs succeed. Steps:

1. Checkout.
2. Download the latest `WebGL-release.zip` from GitHub Releases via `robinraju/release-downloader@v1.8`, extract.
3. `mv WebGL/WebGL .github/pages` — puts the WebGL output at `.github/pages/WebGL/`, which the site's "Play in Browser" button links to.
4. Stage docs sources for the wiki pages:
   ```bash
   cp CHANGELOG.md .github/pages/
   cp README.md .github/pages/
   cp -r docs .github/pages/docs
   ```
   These are what `changelog.html` and `docs.html` fetch at runtime.
5. Deploy `.github/pages/` to the `gh-pages` branch with `JamesIves/github-pages-deploy-action@4.1.4`. GitHub Pages then auto-publishes that branch.

The end state: [`https://andrewiankidd.github.io/PikePush/`](https://andrewiankidd.github.io/PikePush/) for the landing page, `/WebGL/index.html` for the playable build, `/docs.html` for the wiki, `/changelog.html` for the changelog.

## Releases

Every push to `master` (or non-`master` with the draft path) produces a tagged GitHub Release:

- `master` → `release-<yyyy-mm-dd-HH-MM-SS>` (public, not draft).
- Non-`master` → `<short-sha>-<yyyy-mm-dd-HH-MM-SS>` (draft).

All six platform zips attach as release assets. The website's download buttons resolve to `https://github.com/andrewiankidd/PikePush/releases/latest/download/<asset>` — see the `data-download` rewrite block in [index.html](https://github.com/andrewiankidd/PikePush/blob/master/.github/pages/index.html).

## Known rough edges

- `actions/create-release@v1` is deprecated. Replacing with `softprops/action-gh-release@v2` is in the workflow as commented blocks — left as a follow-up.
- No code signing for macOS or iOS — binaries trigger Gatekeeper / TestFlight warnings.
- Android APK is unsigned — install requires "unknown sources".
- No path filters; docs-only pushes burn the full Unity build matrix.

## Running locally

Open `src/` in Unity Hub with editor version `6000.4.7f1`. Press play in `MainMenu.unity`. No CLI build commands wired today — for local platform builds, use **File → Build Settings…**.
