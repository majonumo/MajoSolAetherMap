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

## Corrective quest-association audit — pre-PR

Audit date: November 18, 2026

This audit was performed against the installed game data for version
`2026.11.03.0000.0000`. It does not use territory, NPC or position as evidence
that a quest grants an Aether Current. The authoritative check is the quest's
actual reward relation: `Quest.OtherReward` must reference
`QuestRewardOther` row 2, named **Aether Current**.

### Root cause

The plugin treated every valid `AetherCurrent.Quest` reference as authoritative.
Five active rows contain stale quest links whose referenced quests do not have
the Aether Current reward. This caused `AetherCurrent 2818328` to display
`Quest 70030` (**Curing What Ails**) in Thavnair even though that quest has
`OtherReward = 0`.

### Corrections

| AetherCurrent | Stale quest | Correct reward quest |
| --- | --- | --- |
| 2818065 | 67328 — Natural Repellent | 67326 — Stolen Munitions |
| 2818066 | 67334 — Chocobo's Last Stand | 67333 — The Hunter Becomes the Kweh |
| 2818096 | 67365 — The Unceasing Gardener | 67364 — Hide Your Moogles |
| 2818110 | 67437 — Search and Rescue | 67410 — Honoring the Past |
| 2818328 | 70030 — Curing What Ails | 69793 — In Agama's Footsteps |

The five resolved Thavnair missions are:

- 69952 — Simple Pleasures
- 69739 — Alchemist or Dancer
- 70025 — Steppe Child
- 69795 — Radiant Patrol
- 69793 — In Agama's Footsteps

All five reference `QuestRewardOther 2` (**Aether Current**).

### Global validation

After applying the exact ID corrections, the active data contains 150 quest
associations: 145 direct and 5 corrected. The bidirectional validation reports:

- invalid reward associations: 0;
- duplicate/ambiguous quest associations: 0;
- Aether Current reward quests left unassociated: 0.

At plugin startup, the same validation now rejects direct links that do not
carry the real reward, verifies every explicit correction, detects a quest
mapped to multiple currents and reports any reward quest left without a current.
Unresolved invalid associations fail closed and are omitted instead of being
misclassified as exploration currents.

### Verification and release gate

- Release build: passed with 0 warnings and 0 errors.
- Corrected Release DLL SHA-256:
  `BE732C16D171F103BA2B9A8F2747E38D10B256141E9F404C5DBDB875AFF04BC4`.
- `dotnet format --verify-no-changes`: passed.
- Overlay renderer, windows and exploration-coordinate data were not modified.
- Version `0.7.1.0` is the custom-repository validation candidate so the real
  Dalamud update path can be tested in FFXIV.
- The official Dalamud PR remains blocked until these five corrected
  associations and their completion states are validated in FFXIV.
