# SearsCatalog

*Resize and reposition the build panel in-game.*

![Splash](https://i.imgur.com/jwYCsdf.png)

## Features

### Panel resize

  * Using ConfigurationManager, change `buildHudPanelRows` or `buildHudPanelColumns` to desired values.
  * Build panel will resize to the new `Rows x Columns` size immediately.

### Panel reposition

  * Click on any empty space on the build panel and drag it to the desired position.
  * Position is saved/loaded from config and can be reset in ConfigurationManager.

## Compatability

  * SearsCatalog is a replacement for ComfyBuildExpansion and will try to unpatch CBE.
  * To avoid any issues, please uninstall or disable ComfyBuildExpansion.

## Configuration

All configuration options (except `isModEnabled`) can be modified in-game and will take effect immediately.

![Configuration](https://i.imgur.com/Faoihs5.png)

## Installation

### Manual

  * Un-zip `SearsCatalog.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `SearsCatalog_v1.1.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Notes

  * This is the *good enough* release with more features/options to be added later.
  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/SearsCatalog).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.1.0

  * Added `PieceTable.Left/Right/Up/DownPiece()* patches for controller support.

### 1.0.0

  * Initial release.