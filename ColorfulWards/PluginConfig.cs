using System;

using BepInEx.Configuration;

using UnityEngine;

namespace ColorfulWards {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> ChangeWardColorShortcut { get; private set; }

    public static ConfigEntry<Color> TargetWardColor { get; private set; }
    public static ConfigEntry<string> TargetWardColorHex { get; private set; }

    public static ConfigEntry<bool> UseRadiusForVerticalCheck { get; private set; }

    public static ConfigEntry<bool> ShowChangeColorHoverText { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ChangeWardColorShortcut =
          config.Bind(
              "Hotkeys",
              "changeWardColorShortcut",
              new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift),
              "Keyboard shortcut to change (or clear) the color of the hovered ward.");

      TargetWardColor =
          config.Bind("Color", "targetWardColor", Color.cyan, "Target color to set the ward glow effect to.");

      TargetWardColorHex =
          config.Bind(
              "Color",
              "targetWardColorHex",
              $"#{ColorUtility.ToHtmlStringRGB(Color.cyan)}",
              "Target color to set the ward glow effect to, in HTML hex form.");

      TargetWardColor.SettingChanged += UpdateColorHexValue;
      TargetWardColorHex.SettingChanged += UpdateColorValue;

      UseRadiusForVerticalCheck =
          config.Bind(
              "PrivateArea",
              "useRadiusForVerticalCheck",
              true,
              "Use the ward radius for access/permission checks vertically. Vanilla is infinite up/down.");

      ShowChangeColorHoverText =
          config.Bind(
              "Hud",
              "showChangeColorHoverText",
              true,
              "Show the 'change color' text when hovering over a lightsoure.");
    }

    static void UpdateColorHexValue(object sender, EventArgs eventArgs) {
      TargetWardColorHex.Value = $"#{GetColorHtmlString(TargetWardColor.Value)}";
    }

    static void UpdateColorValue(object sender, EventArgs eventArgs) {
      if (ColorUtility.TryParseHtmlString(TargetWardColorHex.Value, out Color color)) {
        TargetWardColor.Value = color;
      }
    }

    public static string GetColorHtmlString(Color color) {
      return color.a == 1.0f
          ? ColorUtility.ToHtmlStringRGB(color)
          : ColorUtility.ToHtmlStringRGBA(color);
    }
  }
}
