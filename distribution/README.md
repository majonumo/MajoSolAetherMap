# Distribution review checklist

The project uses `Dalamud.NET.Sdk/15.0.0`, which includes DalamudPackager. A Release build produces the DLL, accurate JSON manifest, dependency manifest, official author artwork and `latest.zip`.

Before submitting to the official repository:

1. Confirm the JSON inside `latest.zip` says API 15 and version 0.7.1.0.
2. Provide a square `images/icon.png` (64-512 px) for the D17 submission. The landscape author artwork must not be cropped or altered for this purpose.
3. Optionally capture the screenshots documented under `assets/screenshots/` after privacy review.
4. Copy the prepared template to `testing/live/MajoSolAetherMap/manifest.toml` in a fork of `goatcorp/DalamudPluginsD17`.
5. Disclose AI assistance accurately in the submission PR as required by the Dalamud AI Usage Policy.
6. Do not submit directly to stable; new plugins must begin in `testing/live`.

The repository URL, GitHub owner and release commit in the template are real and verified. No official submission has been opened.
