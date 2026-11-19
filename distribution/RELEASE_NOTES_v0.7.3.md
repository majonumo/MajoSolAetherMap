# Majo & Sol Aether Map v0.7.3

Community-testing update focused on safe native flag removal.

## Improved

- Replaced the global marker-clearing action with an **Eliminar marcador** /
  **Remove marker** button beside the flag action for the selected objective.
- Native flags placed by the plugin are removed automatically when their
  associated Aether Current is registered as unlocked.
- Preserved the existing overlay, interface layout, progress and exploration
  coordinates.

## Safety

FFXIV exposes one shared native map flag. Before removing it, the plugin checks
that its territory, map, icon and coordinates still match the exact flag placed
for the selected objective. If the player or another plugin replaces it, Majo &
Sol discards ownership and leaves the replacement untouched.

This release is distributed only through the Majo & Sol custom repository for
community validation. It is not an official Dalamud submission.
