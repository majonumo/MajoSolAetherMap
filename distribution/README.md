# Distribution review checklist

The project uses `Dalamud.NET.Sdk/15.0.0`, which includes DalamudPackager. A Release build produces the DLL, accurate JSON manifest, dependency manifest and `latest.zip`.

Before submitting to the official repository:

1. Create a public GitHub repository and review every committed file.
2. Add `assets/majo-and-sol.png` if Majo & Sol want it distributed.
3. Capture the three screenshots documented under `assets/screenshots/`.
4. Confirm the JSON inside `latest.zip` says API 15 and version 0.7.0.0.
5. Replace all `REQUIRED_*` values in the D17 manifest template.
6. Copy it to `testing/live/MajoSolAetherMap/manifest.toml` in a fork of `goatcorp/DalamudPluginsD17`.
7. Add a square `images/icon.png` (64–512 px) and optionally `image1.png` through `image5.png` beside the D17 manifest.
8. Disclose AI assistance accurately in the submission PR as required by the Dalamud AI Usage Policy.
9. Do not submit directly to stable; new plugins must begin in `testing/live`.

No repository URL, GitHub owner or commit is fabricated here. No push or submission is performed automatically.
