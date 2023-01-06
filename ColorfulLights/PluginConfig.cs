using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace ColorfulLights {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static ConfigEntry<KeyboardShortcut> ChangeColorActionShortcut { get; private set; }

    public static ExtendedColorConfigEntry TargetFireplaceColor { get; private set; }

    public static ConfigEntry<bool> ShowChangeColorHoverText { get; private set; }
    public static ConfigEntry<int> ColorPromptFontSize { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ChangeColorActionShortcut =
          config.Bind(
              "Hotkeys",
              "changeColorActionShortcut",
              new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift),
              "Keyboard shortcut to change (or clear) the color of the hovered torch/fire.");

      TargetFireplaceColor =
          new(
              config,
              "Color",
              "targetFireplaceColor",
              Color.cyan,
              "Target color to set any torch/fire to.",
              colorPaletteKey: "targetFireplaceColoPalette");

      ShowChangeColorHoverText =
          config.Bind(
              "Hud",
              "showChangeColorHoverText",
              true,
              "Show the 'change color' text when hovering over a lightsoure.");

      ColorPromptFontSize =
          config.Bind("Hud", "colorPrompFontSize", 15, "Font size for the 'change color' text prompt.");

      config.SaveOnConfigSet = true;
    }
  }
}
