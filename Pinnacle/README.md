# Pinnacle

*Pinnacle perpetually provides premium pin performance.*

![Pinnacle - At a Glance](https://imgur.com/Wabfnru.png)

## Installation

### Manual

  * Un-zip `Pinnacle.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `Pinnacle_v1.0.3.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Features

### PinEditPanel

  * Edit existing pins and add new pins with more icon types!

    ![Pinnacle - Edit an Existing Pin](https://imgur.com/ODB2jVz.png)

  - Edit an existing pin
    - Left-click on an existing pin on the map.
    - The *PinEditPanel* will toggle on with the default pin data.
    - You can modify the pin name, icon and position.
    - You can toggle the pin checked state and shared state.

  * Add a new pin
    * Left-double-click on the target point on the map to add a new pin.
    * The *PinEditPanel* will toggle on with default pin data.

  - Click anywhere on the map without a pin to toggle off the *PinEditPanel*.

### PinListPanel

  * Lists all your pins or filter them by pin name!

    ![Pinnacle - Show the PinListPanel](https://imgur.com/IrE36jV.png)

  - Show/hide the *PinListPanel*
    - Press `Tab` (configurable) to toggle the *PinlistPanel* on and off.
    - All pins will initially be listed and pin count shown on the bottom.

  * Filter pins by name
    * Enter text in the input field at the top of the panel.
    * Pins will by filtered by matching text in their name.

  - Center map on pin
    - Left-click on the target pin row and the map will center onto that pin
    - Scrolling animation can be disabled by setting `CenterMap.lerpDuration` config to 0.

  * Reposition the *PinListPanel*
    * Left-click on an open space on the panel and drag to reposition.

  - Resize the *PinListPanel*
    - Hover near the **bottom-right corner** to show the resize button.
    - Left-click and drag on this button to resize the panel.

### PinFilterPanel

  * Filter pins on the map by **any icon type** (replaces vanilla panel).

    ![Pinnacle - PinFilterPanel](https://imgur.com/fPs7fDd.png)

### Map Teleporting

  * **Requirements!**
    * `devcommands` must be enabled via console.
    * You must be in single-player mode or the local server-host.

  - Teleport to map point
    - Hold `LeftShift` and click on the target point on the map.

  * Teleport to pin position
    * Hold `LeftShift` and click on the target row in the *PinListPanel*.

### Configuration

  * Important/critical configuration options are available (more to come later).

    ![Pinnacle - Configuration](https://imgur.com/DBUH4Jq.png)

  - Changing the Minimap.Pin font/font-size
    - These two options are available once you are logged into any world.

## Notes

  * This is the *good enough* release with more features/options to be added later.
  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/Pinnacle).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.0.3

  * Fixed a bug where the PinEditPanel would not default the Pin.Icon to the last selected one.

### 1.0.2

  * Fixed label for the Z-value in VectorCell incorrectly showing 'X'.
  * Removed code in `UIBuilder.CreateRoundedCornerSprite()` that saved the sprite to disk, was used for debugging.

### 1.0.1

  * Fixed a bug where the *PinEditPanel* was blocking map-movement when toggled off.

### 1.0.0

  * Initial release.