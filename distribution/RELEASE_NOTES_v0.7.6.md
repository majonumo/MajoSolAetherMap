# Majo & Sol Aether Map v0.7.6

Community-testing fix for live Aether Current and quest progress updates.

## Fixed

- Aether Currents obtained while the plugin window is open now update cards,
  counters and pending/completed lists without reopening the window.
- Completed Aether Current quests now receive the same live update behavior.
- Managed overlay markers and owned native flags are refreshed through the
  existing safe cleanup path when their objective completes.

## Performance and safety

- Dalamud's unlock event remains the immediate update path.
- A lightweight fallback checks only unresolved objectives once per second and
  rebuilds snapshots only when a progress boolean actually changes.
- IDs, coordinates, configuration, filters and native-flag ownership validation
  are unchanged.

This release is distributed only through the Majo & Sol custom repository for
community validation. It is not an official Dalamud submission.
