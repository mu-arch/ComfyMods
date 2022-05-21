# LetMePlay

  * Collection of tweaks and mods to improve player experience and accessibility.

## Wards

  * Disable wards from flashing their blue shield.

## Camera

  * Disable camera sway (which is due to player head animation) when sitting on a chair/bench.

## Inventory

  * Fixes interaction with non-player items (GoblinSpear, etc) in player inventory and chests.
    * These items will have the `grey hammer` icon and the prefab name.
    * These items will have a special description and this mod as the crafter name.

## Build

  * Disables the yellow placement indicator when building.
    * If you are using gizmo, the gizmo indicator will also be disabled.

## Weather

  * Disable snow particles during Snow/SnowStorm weather.
  * Disable ash particles during Ashrain weather.

## Instructions

* Most toggles/options are **initially disabled**, there are two ways to toggle them.

  1) Press F1 in-game to open the ConfigurationManager and navigate to "Let Me Play" section.
  2) Start the game once to generate the configuration file, then navigate to and open the file:
     * `<install folder>\Valheim\BepInEx\config\redseiko.valheim.letmeplay.cfg`

## Changelog

### 1.3.0

  * Added patch to `SpawnArea.Awake()` where it will check if there are any null prefabs in its `m_prefabs` list and if
    so, remove them.

### 1.2.0

  * Added two toggles `disableWeatherSnowParticles` and `disableWeatherAshParticles`.
    * These disable the snow particles during Snow/SnowStorm weather and the ash particles during Ashrain weather.

### 1.1.0

  * Updated for Hearth and Home.

### 1.0.0

  * Initial release after updating to HarmonyX.