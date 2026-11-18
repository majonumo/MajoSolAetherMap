# Majo & Sol Aether Map v0.7.1

Corrective release for Aether Current quest associations.

## Fixed

- Replaced the stale Thavnair link to **Curing What Ails** with the actual
  Aether Current reward quest **In Agama's Footsteps**.
- Corrected four equivalent stale quest links in Heavensward zones.
- Added bidirectional validation against the real `QuestRewardOther`
  **Aether Current** reward.
- Invalid or ambiguous future associations now fail closed and are logged.

The overlay renderer, interface and exploration-current coordinates are
unchanged. This custom-repository release must be validated inside FFXIV before
the official Dalamud submission proceeds.
