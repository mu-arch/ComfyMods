# BetterZeeLog

*Logging zees the better way.*

## Features

  * Wraps all `ZLog.Log*` methods to prefix a timestamp and remove the trailing new-line.
  * Removes stack traces for `Info` and `Warning` log types.
  * Removes the 'Failed to send data' logging in `ZSteamSocket` to reduce log spam.

## Changelog

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