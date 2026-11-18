# Final audit — 0.7.0.0

Audit date: November 18, 2026

The author reports that v0.7.0.0 was personally tested successfully inside FFXIV. This audit does not claim that every one of the 30 selectable zones was individually exercised.

## Build and packaging

- Release build: passed with 0 warnings and 0 errors.
- `dotnet format --verify-no-changes`: passed.
- Generated manifest: version `0.7.0.0`, Dalamud API `15`, accurate English metadata.
- Generated `latest.zip`: contains the plugin DLL, JSON manifest, dependency manifest and the official author artwork.
- `assets/majo-and-sol.png` is included unchanged in the package. Source, build-output and ZIP copies share SHA-256 `901DCA2C3D10BB5B566932117BA2B4D44C9CD30E5EFE439F5F7665FD631E5301`.
- No Dalamud, Lumina or FFXIVClientStructs binaries are redistributed.
- Final package SHA-256: `2CF2BA9DD1760498403FD541469BFE415498DDF220BE93FFCBDD461238CB0E91`.

## Data coverage

- Active Aether Currents found in installed game data: 302.
- Selectable Aether Current zones: 30.
- Maps containing overlay markers: 35, including cross-zone quest NPC locations.
- Localized map-name collisions among marker maps: 0.
- Exploration locations and quest NPC resolution retain the previously validated coverage documented in the changelog and private project notes.

## Performance

- Static Lumina relationships are indexed once during plugin construction.
- Cross-zone snapshots are cached by territory and invalidated only on login, logout, territory change or unlock events.
- Overlay marker states are cached by `MapId` and the same revision counter.
- Native-map identity is recalculated only when area, subarea or title text changes.
- Zoom, pan, resize, clipping and base-map transforms remain per-frame reads because those values are dynamic.
- No Lumina sheet scan or texture decode is performed per frame.

## Overlay stability

- The overlay remains passive: foreground draw-list commands only, with no ImGui hitboxes over the native map.
- It reads typed `AddonAreaMap`, `AtkComponentMap`, `BaseMapImage` and `AtkResNode` fields.
- It does not write game memory, `AgentMap`, native marker arrays or addon nodes.
- Map-to-screen conversion still derives from the native base-image transform, so zoom and pan share the game's own matrix.
- Markers are clipped to the component viewport and skipped when the map identity is ambiguous.
- The 0.7 feature work does not alter the transform formula. A final in-game regression pass at minimum and maximum zoom is still required before publication.

## Lifecycle and failure handling

- Command, UI and configuration events are unsubscribed on disposal.
- Service login, logout, territory and unlock events are unsubscribed on disposal.
- Shared textures are owned by the plugin-scoped `ITextureProvider`; missing optional artwork falls back to a native branded card.
- Invalid persisted territory selections reset to the current-zone mode.
- Missing, ambiguous or unsupported data is omitted or labelled instead of guessed.

## Repository hygiene

- Local caches, build output, research tools, plugin configs, logs and IDE state are ignored.
- Private collaboration context and machine-specific investigation notes are ignored.
- No credentials, private keys, account identifiers or machine paths were found in public files.
- No extracted or proprietary FFXIV resources are committed.

## Manual release gates

The following cannot be completed safely without author input and an in-game release-candidate session:

1. Capture and privacy-review any additional repository screenshots.
2. Insert the public release commit SHA in the prepared D17 manifest.
3. Review the Dalamud AI Usage Policy and disclose assistance accurately in the submission PR.
