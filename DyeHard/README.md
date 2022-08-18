# DyeHard

*Change your player hair type/color and beard type/color!*

## Installation

### Manual

  * Un-zip `DyeHard.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual install)

  * Go to Settings > Import local mod > Select `DyeHard_v1.4.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Instructions

  * This mod only overrides the visual appearance of your character and does not modify your player data.
  * Press `F1` to bring up the ConfigurationManager and navigate to `DyeHard`.
    * The character preview in the character select screen is offset (configurable) to not be hidden.

### Change your hair style

  * `overridePlayerHairItem` toggles this feature on/off.
  * Change the hair type using the drop-down.

### Change your hair color

  * `overridePlayerHairColor` toggles this feature on/off.
  * Adjust your hair color using the RGB sliders or by entering in an HTML color code in hex form.

### Change your beard style

  * `overridePlayerBeardItem` toggles this feature on/off.
  * Change your beard type using the drop-down.

### Notes

  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/DyeHard).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.4.0

  * Added option to override player hair.
  * Added individual override toggles for player hair, hair color and player beard.
  * Added `OffsetCharacterPreviewPosition` for character select screen to handle ConfigurationManager hiding player.
  * Extracted all patch-related code into new patch classes.
  * Extracted configuration-related code into PluginConfig class.
  * Extracted extensions-related code into PluginExtensions class.

### 1.3.0

  * Added `manifest.json`, changed the `icon.png` and updated this `README.md`.
  * Modified the project file to automatically create a versioned Thunderstore package.

### 1.2.0

  * Added option to override player beard (regardless of gender).

### 1.1.0

  * Updated for Hearth & Home.

### 1.0.1

  * Minor changes, bump to .NET framework v4.8.

### 1.0.0

  * Initial release.