# SkyTree

*Makes the tree in the sky (Yggdrasil / World Tree) solid and a foundation to build on.*

## Instructions

  * SkyTree activates on server login or local world load (so enabling/disabling requires a restart) and...
    * Adds a `MeshCollider` component to the `YggdrasilBranch` prefab as well as its `branch` children.
    * Sets the layer for the `YggdrasilBranch` prefab/children from 19 (skybox) to 15 (static_solid).

  - **Disable this mod when generating a new world locally.**
    - The vanilla world generation code makes use of `RayCasts` for vegetation/location placement.
    - These `RayCasts` go from top-down and the collider on the SkyTree will block placement anywhere below.
    - Mod should be re-enabled once zone/sector generation has occurred.

## Installation

### Manual

  * Un-zip `SkyTree.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual install)

  * Go to Settings > Import local mod > Select `SkyTree_v1.4.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

### Notes

  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/SkyTree).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.4.0

  * Updated for `v0.216.9` patch.
  * Properly uses the PluginGuid for the HarmonyInstanceId instead of the PluginVersion.

### 1.3.0

  * Moved all configuration code into new `PluginConfig` class.
  * Added some more logging when changing layers to get the layer name.
  * Added `manifest.json`, changed the `icon.png` and updated this `README.md`.
  * Modified the project file to automatically create a versioned Thunderstore package.

### 1.2.0

  * Updated for Hearth & Home.
  * Fixed a bug in adding a collider to the SkyTree.

### 1.1.0

  * Updated project template and references to use latest DLLs.
  * Small code clean-up.

### 1.0.0

  * Initial release.