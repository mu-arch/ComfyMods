# Shortcuts

  * Re-assign more keyboard shortcuts that are hard-coded/unconfigurable in vanilla.
    * Console
      * toggleConsoleShortcut
    * Debugmode
      * debugKillAllShortcut
      * debugRemoveDropsShortcut
      * toggleDebugFlyShortcut
      * toggleDebugNoCostShortcut
    * GameCamera
      * takeScreenshotShorcut
      * toggleMouseCaptureShortcut
    * Hotbar
      * hotbarItem1Shortcut
      * hotbarItem2Shortcut
      * hotbarItem3Shortcut
      * hotbarItem4Shortcut
      * hotbarItem5Shortcut
      * hotbarItem6Shortcut
      * hotbarItem7Shortcut
      * hotbarItem8Shortcut
    * Hud
      * toggleHudShortcut
    * ConnectPanel
      * toggleConnectPanelShortcut

## Installation

### Manual

  * Un-zip `Shortcuts.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `Shortcuts_v1.2.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Notes

  * Un-binds the debugmode KillAll shortcut `K` by default because of many accidental pet deaths. :(
  * Un-binds the debugmode RemoveAll shortcut `L` by default.

## Changelog

### 1.2.0

  * Prototype using a special version of `IsDown` modified from BepInEx's KeyboardShortcuts code.

### 1.1.0

  * Updated for Hearth and Home.
  * Added toggles for:
    * Debugmode - removedrops `L`
    * ConnectPanel - toggle `F2`

### 1.0.0

  * Initial release.
