# Compress

  * Opt-in server and client data compression when syncing ZDOs.
  * ZPackage/ZSteamSocket optimizations for related code in the SendZDOs hot-loop.

## Notes

  * This mod **must be installed** on the **server** to enable compression.
    * As a client, if the server **does not have the mod** then Compress will send uncompressed/vanilla data.
      * Your Send rate in the F2 Panel will be the usual ~150 KB/s.
    * As a client, if the server **does have the mod** it will start sending and receiving compressed ZDO data.
      * You should see a drop in your Send rate of around 50% to ~80 KB/s.

## Installation

  * Manual/Vortex
    * Un-zip `Compress.dll` to your `/Valheim/BepInEx/plugins/` folder.
  * Thunderstore
    * Go to `Settings > Browse profile folder` and un-zip `Compress.dll` to the `/BepInEx/plugins/` folder.

## Configuration

  * `isModEnabled`
    * Globally enable or disable this mod (restart required).

## Changelog

### 1.2.0

  * Added `ZPackage.Write(ZPackage)` patch that copies the passed in ZPackage buffer directly.

### 1.1.0

  * Added `ZSteamSocket.Send()` patch that directly sends the ZPackage data on the socket.

### 1.0.0

  * Initial release.