# Insightful

*For those that seek words inscribed upon the world.*

## Installation

### Manual

  * Un-zip `Insightful.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `Insightful_v1.3.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Instructions

  * Hover/target an object with an embedded *Inscription* and a prompt will appear to read it.
  * Press `RightShift + R` (configurable) to read the hidden text.

## Notes

  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/Insightful).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.3.0

  * Fixed for the `v0.214.2` patch.
  * Changed the `Player.TakeInput()` delegate to a `Player.UpdateHover()` delegate with better key-down handling.

### 1.2.0

  * Fixed the `Player.Update()` transpiler TakeInput delegate to properly work with other mods that also patch it.
  * Extracted configuration-related code into a new `PluginConfig` class.
  * Extracted extension methods into a new `PluginExtensions` class.
  * Extracted patch-related code into new `PlayerPatch` and `HudPatch` classes.
  * Added `manifest.json`, `icon.png` and this `README.md`.
  * Modified the project file to automatically create a versioned Thunderstore package.

### 1.1.0

  * New support for Inscriptions `v1.1.0` in enabling inscriptions for **anything**.
  * Moved hover text modification to `Hud.UpdateCrosshair()` to support the new feature.

### 1.0.0

  * Initial release.