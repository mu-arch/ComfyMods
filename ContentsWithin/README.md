# ContentsWithin

  * Show the contents of any chests/container when hovering over it **using the existing container UI**.

## Installation

### Manual

  * Un-zip `ContentsWithin.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual install)

  * **Uninstall** any older versions of `ContentsWithin`.
  * Go to Settings > Import local mod > Select `ContentsWithin_v1.0.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Instructions

  * Launch the game and go hover over any chest.
  * You can interact with the chest regularly and it should bring up the regular player inventory when you are the
    chest instance owner.
  * You can press hot-key `RightShift + P` (configurable) to toggle off/on the "show container contents" feature.
    * Turning this off/on (as well as toggling `isModEnabled`) while looking at a chest can cause weird UI behaviour.

## Notes

  * The info panel, crafting panel and player inventory panel are hidden when in hovering mode.
  * The info panel and crafting panel are hidden on chest interaction.
  * Hovering works when another player has the chest open and updates the contents being changed.
  * Opening a chest of which you are not instance owner of will try to go through the "chest" interact sequence and may
    take a moment to activate.

## Changelog

### 1.0.0

  * Initial release.