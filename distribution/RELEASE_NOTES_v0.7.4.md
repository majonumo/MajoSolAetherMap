# Majo & Sol Aether Map v0.7.4

Community-testing hotfix for the per-objective native flag removal action.

## Fixed

- Fixed the **Eliminar marcador** / **Remove marker** button not appearing
  after placing a native flag.
- Ownership validation now matches the integer world coordinates serialized by
  Dalamud's `OpenMapWithMapLink` instead of comparing them with the original
  decimal coordinates.

## Safety

Territory, map, icon and coordinates must still match the exact flag placed by
the plugin. If the player or another plugin replaces it, Majo & Sol discards
ownership and leaves the replacement untouched. The game's native right-click
removal remains available as an alternative.

This release is distributed only through the Majo & Sol custom repository for
community validation. It is not an official Dalamud submission.
