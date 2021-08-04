# EulersRuler v1.0.0

  * Shows targeted piece name, health, stability and rotatios while hammer is active
  * Shows name and rotation of build piece currently being placed.

## User Interface

  * This fuctionality is available while the build hammer is active.
  * Display a panel with the piece name, health, stability and rotation (Euler & Quaternion) of the targeted piece.
    * Panel position can be configured.
    * Every row can be toggled on/off in the configuration.
  * Display a panel with the prefab name and rotation (Euler & Quaternion) of the placement ghost piece.
    * Panel position can be configured.
    * Every row can be toggled on/off in configuration.

## Instructions

  1. Extract `EulersRuler.dll` to your `/Valheim/BepInEx/plugins/` folder.
  2. Activate building mode by making your hammer your active weapon.

## Known Conflicts

  * BuildingHealthDisplay
    * Mod functionality is duplicated, disable one or the other.

## Changelog

### 1.0.0

  * Initial release.