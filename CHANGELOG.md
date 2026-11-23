# Changelog

All notable changes to Majo & Sol Aether Map are documented here.

## Unreleased

- Fixed live progress becoming stale while the plugin remained open by retaining
  the immediate unlock-event refresh and adding a throttled one-second check of
  only unresolved Aether Current and quest states.
- Progress changes now invalidate current/selected-zone snapshots, update cards
  and counters, and run the existing managed-marker and native-flag cleanup.

## [0.7.5.0] - 2026-11-22

- Added centralized UI localization with English fallback for English, Spanish,
  Brazilian Portuguese, French, German and Japanese.
- Added automatic FFXIV client-language detection plus an immediate, persistent
  manual selector while leaving all game-provided names and data intact.
- Added independent 30–100% window and card background opacity controls and a
  reset action; text, icons and controls remain opaque.
- Added backward-compatible configuration migration and technical agent guidance.

## [0.7.4.0] - 2026-11-19

- Fixed native flag ownership detection by matching the integer world
  coordinates serialized by Dalamud's `OpenMapWithMapLink`, allowing the
  per-objective **Remove marker** button to appear after placing a flag.

## [0.7.3.0] - 2026-11-19

- Replaced the global marker-clearing control with a per-objective remove action
  beside the native flag button.
- Added ownership validation against the live native flag territory, map, icon
  and coordinates before removing it, so manual and third-party flags remain
  untouched.
- Native flags placed by the plugin are also removed when their associated
  Aether Current is unlocked.

## [0.7.2.0] - 2026-11-19

- Prevented nested objective cards from consuming mouse-wheel scrolling and
  shifting their text independently from the outer objective list.
- Added event-driven removal of plugin-managed overlay markers when their
  associated Aether Current is unlocked.
- Added a confirmed **Clear All Markers** action for the plugin overlay.
- Kept native shared map flags untouched because Dalamud API 15 does not expose
  selective ownership or removal for `OpenMapWithMapLink` flags.

## [0.7.1.0] - 2026-11-18

- Corrected five stale `AetherCurrent.Quest` links by validating the actual
  `QuestRewardOther` reward instead of inferring associations from territory,
  NPC or position.
- Fixed Thavnair so its quest currents are Steppe Child, Alchemist or Dancer,
  Radiant Patrol, In Agama's Footsteps and Simple Pleasures.
- Corrected four equivalent stale links in Heavensward zones.
- Added startup validation for invalid, duplicate and unassociated Aether
  Current quest rewards.
- Kept the overlay, interface and exploration coordinates unchanged.
- Published to the custom repository as an in-game validation candidate; the
  official Dalamud submission remains blocked until that validation passes.

## [0.7.0.0] - 2026-11-18

- Added an About window with credits, motto and creation date.
- Added the official `assets/majo-and-sol.png` cover and packaged About artwork.
- Added Spanish/English interface selection.
- Added a persistent zone selector for checking progress in other maps.
- Added bilingual repository documentation, MIT license and publication checklist.
- Added official DalamudPluginsD17 testing-manifest template.
- Added a versioned custom Dalamud repository manifest and automated build/release workflows.
- Expanded repository hygiene, safety and limitation documentation.

## [0.6.0.0] - 2026-11-18

- Introduced the Majo & Sol visual theme and compact responsive UI.
- Added zone loading artwork, circular progress and horizontal progress cards.
- Redesigned current and quest rows with icons, truncation and aligned actions.
- Moved persistent options into a dedicated settings window.

## [0.5.0.0] - 2026-11-18

- Added the passive multi-marker overlay over the native FFXIV map.
- Added markers for exploration currents, side-quest NPCs and MSQ NPCs.
- Added zoom, pan, resize and viewport clipping synchronization.
- Added marker filters, size and opacity configuration.
- Added conservative localized map identification and per-map caching.

## [0.4.0.0] - 2026-11-18

- Added real Aether Current quest names and completion state.
- Added quest issuer NPC resolution through `Quest`, `ENpcBase`, `ENpcResident` and `Level`.
- Added MSQ/side-quest classification and native NPC map flags.
- Added ambiguity handling and full location-coverage reporting.

## [0.3.0.0] - 2026-11-18

- Added all real exploration-current coordinates.
- Added pending/completed current lists.
- Added native map flags and verified world-to-map conversion.
- Completed the coordinate and map-interaction phases.

## [0.2.0.0] - 2026-11-18

- Added real zone detection and Lumina Aether Current relationships.
- Added real unlock and flight status through Dalamud API 15.
- Added exploration/quest categorization and progress summaries.
- Added ARR, urban and no-current zone handling.

## [0.1.0.0] - 2026-11-18

- Created the .NET 10 / Dalamud API 15 plugin project.
- Added `/aethermap`, the initial ImGui window and generated plugin manifest.
- Produced the first loadable Release DLL.
