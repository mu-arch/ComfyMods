# ColorfulPortals

  * You can color the activation glow effect of any portal using RGB and HTML color codes!
  * Those without the mod will still see the vanilla yellow/red glow effect.

## Instructions

### Changing portal glow effect color

  * In-game, press F1 to bring up the ConfigurationManager and navigate to the ColorfulPortals section.
    * Change the target color using the RGB sliders or using an HTML color code.

  - Hover over any portal ***that you are the owner of*** and a prompt to change its color will appear.
    - This prompt can be hidden by disabling the `showChangeColorHoverText` setting.

  * Hit `LeftShift + E` (configurable) to change the color of the portal glow effect.

### Stone portals

  * ***Stone portal*** prefabs fixed to activate when connected.
  * Restrictions: they cannot be built and require a server-side mod to connect.

## Installation

### Manual

  * Un-zip `ColorfulPortals.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual install)

  * **Uninstall** any older versions of `ColorfulPortals`.
  * Go to Settings > Import local mod > Select `ColorfulPortals_v1.6.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Notes

  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.6.0

  * Fixed for `v0.216.9` patch.
  * Modified `Player.TakeInput()` transpiler patch to happen after `Player.UpdateHover()`.
  * Modified `ChangePortalColor` to no longer be a coroutine.
  * Created WIP `TeleportWorldColor` component to use for regular portals, stone portal to be updated later.

### 1.5.0

  * Moved change color code from `TeleportWorld.Interact()` prefix to `Player.TakeInput()` transpiler with coroutine.
    * Can now configure the hot-key to change portal color.
  * Changed some of the logic in `RemovedDestroyedTeleportWorldsCoroutine()`.
  * Removed configuration option for `colorPromptFontSize` (UI overhaul coming later).
  * Extracted configuration options into new `PluginConfig` class.
  * Extracted `TeleportWorldData` into its own class.
  * Added `manifest.json`, `icon.png` and updated this `README.md`.
  * Modified the project file to automatically create a versioned Thunderstore package.

### 1.4.0

  * Added an option to change the font-size for the text prompt on hover.

### 1.3.0

  * Updated for Hearth & Home.
  * Added `PortalLastColoredBy` ZDO property that is set whenever a player changes the portals color.

### 1.2.0

  * Fixed a memory-leak when caching TeleportWorld/Portals.

### 1.1.0

  * Adding configuration setting to hide the 'change color' prompt over a ward.
  * Now saves the target color's **alpha** value to the ZDO and reads/uses this alpha value if present in the ZDO.

### 1.0.0

  * Initial release.