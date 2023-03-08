# BetterZeeLog

*Logging zees the better way.*

## Features

  * Wraps all `ZLog.Log*` methods to prefix a timestamp and remove the trailing new-line.
  * Removes stack traces for `Info` and `Warning` log types.
  * Removes the 'Failed to send data' logging in `ZSteamSocket` to reduce log spam.

## Installation

### Manual

  * Un-zip `BetterZeeLog.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `BetterZeeLog_v1.4.1.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Notes

  * See source at: [GitHub/ComfyMods](https://github.com/redseiko/ComfyMods/tree/main/BetterZeeLog).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)

## Changelog

### 1.4.1

  * Fixed the PluginVersion not being referenced in `AssemblyInfo.cs`.
  * Expanded this `README.md`.

### 1.4.0

  * Updated for Valheim `v0.212.6` Mistlands PTB.
  * Extracted configuration logic into `PluginConfig` class.
  * Extracted patch logic into separate patch classes.
  * Added configuration option `RemoveFailedToSendDataLogging` for the ZSteamSocket transpiler patch.

### 1.3.0

  * Updated for Valheim `v0.209.5`.
  * Fixed the `ZSteamSocket.SendQueuedPackages` transpiler.
  * Added `manifest.json`, `icon.png` and `README.md`.
  * Modified the project file to create a versioned Thunderstore package.