# Changelog

All notable changes to Majo & Sol Aether Map are documented here.

## [0.7.0.0] - 2026-11-18

- Added an About window with credits, motto and creation date.
- Added the official `assets/majo-and-sol.png` cover and packaged About artwork.
- Added Spanish/English interface selection.
- Added a persistent zone selector for checking progress in other maps.
- Added bilingual repository documentation, MIT license and publication checklist.
- Added official DalamudPluginsD17 testing-manifest template.
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
