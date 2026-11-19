# Majo & Sol Aether Map v0.7.2

Community-testing update focused on marker lifecycle and scrolling stability.

## Improved

- Fixed subtle text movement inside objective cards when scrolling with the
  mouse wheel at fractional UI scales.
- Plugin-managed overlay markers now disappear when their associated Aether
  Current is registered as unlocked.
- Added a confirmed **Clear All Markers** action for the Majo & Sol overlay.
- Marker suppression remains synchronized across map and territory changes
  without adding per-frame data scans.

## Safety

Dalamud API 15 does not expose selective ownership or removal for the native
flag created through `OpenMapWithMapLink`. Native flags are therefore left
untouched so manual markers and markers from other plugins cannot be removed.

This release is distributed only through the Majo & Sol custom repository for
community validation. It is not an official Dalamud submission.
