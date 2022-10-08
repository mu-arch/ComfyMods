# Chatter

*An elegant chat for a more civilized age.*

![UI-Screenshot-01](https://i.imgur.com/SPeqjsu.png)

## Installation

### Manual

  * Un-zip `Chatter.dll` to your `/Valheim/BepInEx/plugins/` folder.

### Thunderstore (manual)

  * Go to Settings > Import local mod > Select `Chatter_v1.2.0.zip`.
  * Click "OK/Import local mod" on the pop-up for information.

## Instructions

### New Features!

  * Chat filters with a dynamic UI.

### Features

  * Change the chat font to any built-in game fonts as well as fonts on the computer.
  * Configure the font colors for every chat message type.
  * Toggle on/off the display of chat messages by type (see below).
  * Use hot-keys (default: `PageUp`, `PageDown`) to scroll through the chat messages.
  * Chat panel will auto-hide by dimming to 20% (configurable) after 10s (configurable).
  * Use any color for the chat panel background!
  * Move **and** resize the chat panel using the mouse (see below).
  * Change change layouts on the fly between `WithHeaderRow` and `SingleRow` layouts (see below).

### Move/Resize/Toggle

  * Press `Escape` to show the move/resize/toggle bar.

    ![UI-Screenshot-03](https://i.imgur.com/Fv126LR.png)

### Change Message Layouts

  * The message layout can be switched at anytime in configuration manager and messages will be rebuilt immediately.

    ![UI-Screenshot-04](https://i.imgur.com/7c5T3X8.png)

### Chat Filters

  * Add/edit/remove text filters for every type of chat message using a custom drawer in ConfigurationManager.

    ![UI-Screenshot-05](https://imgur.com/vRmaiDe.png)

  - Filters only affect messages in Chatter's window.
  - Changing any text filters will only apply to **new** incoming messages.
  - This is still an *experimental* custom drawer so please report any bugs.

### Configure

  * Almost every component of the UI is configurable.

    ![UI-Screenshot-02](https://i.imgur.com/XUzlkmp.png)

### Notes

  * This has reached a point of 'good-enough' for the initial release with more features to come later.
  * See source at: [GitHub](https://github.com/redseiko/ComfyMods/tree/main/Chatter).
  * Looking for a chill Valheim server? [Comfy Valheim Discord](https://discord.gg/ameHJz5PFk)
  * Check out our community driven listing site at: [valheimlist.org](https://valheimlist.org/)

## Changelog

### 1.2.1

  * Fixed a bug where if no filters are defined it filters **everything** (sadface).

### 1.2.0

  * Added support for chat filters configurable using a custom drawer in ConfigurationManager.

### 1.1.0

  * Added support for a new chat message layout `SingleRow`.
  * Added new configuration options for timestamp and content spacing.
  * Refactored code to unify UI creation from all message types and rebuild from message history.

### 1.0.0

  * Initial release.