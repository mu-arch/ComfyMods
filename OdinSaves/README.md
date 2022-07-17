# OdinSaves

  * Your game will now save your character more often!
    * This means less annoyances if you crash or bug out!
    * Defaults to saving every 300s, configurable! (Game default and maximum is every 1200s).
    * Saves your current position as your logout point (defaults to true).
    * Shows a message when saving (also configurable).
  * Compress the map data (using vanilla compression) of other worlds you have visited.
    * Note: **this is a non-reversible** process. Please make a back-up of your character file as described below.

## Installation

### Manual

  * Un-zip `OdinSaves.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `OdinSaves_v1.2.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Notes

  * Game crashes or other issues during game saves can always be a cause for corruption and so you should make a backup
    of your character saves files often.
  * Your character save files (in Windows 10) can be found at:
    * `C:\Users\<YourWindowsUsernameHere>\AppData\LocalLow\IronGate\Valheim\characters`

## Changelog

### 1.2.0

  * Added `manifest.json`, `icon.png` and `README.md`.
    * Modified the project file to automatically create a versioned Thunderstore package.

### 1.1.1

  * Minor fixes to the UI.

### 1.1.0

  * Moved to compression-only of map data to utilized newly added compression in H&H update.

### 1.0.0

  * Added compression/decompression of map data for player profiles.