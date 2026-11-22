# Majo & Sol Aether Map v0.7.5

Community-testing update for multilingual UI and configurable transparency.

## Added

- Six interface languages: English, Spanish, Brazilian Portuguese, French,
  German and Japanese, with English fallback.
- Automatic FFXIV client-language detection and an immediate, persistent
  manual language selector.
- Independent 30–100% opacity controls for the main window and internal card
  backgrounds, plus a reset action.

## Compatibility and safety

- Quest, NPC, zone, Aether Current and other game-provided names remain
  untouched.
- Existing Spanish/English settings migrate without losing the selected
  language.
- Transparency changes only background colors; text, icons and controls remain
  opaque, and ImGui styles are restored after each window render.
- Aether Current detection, progress, coordinates, filters, markers and native
  flag ownership logic are unchanged.

This release is distributed only through the Majo & Sol custom repository for
community validation. It is not an official Dalamud submission.
