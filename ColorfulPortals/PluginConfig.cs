using BepInEx.Configuration;

using System;

using UnityEngine;

namespace ColorfulPortals {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> ChangePortalColorShortcut { get; private set; }

    public static ConfigEntry<Color> TargetPortalColor { get; private set; }
    public static ConfigEntry<string> TargetPortalColorHex { get; private set; }

    public static ConfigEntry<bool> ShowChangeColorHoverText { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ChangePortalColorShortcut =
          config.Bind(
              "Hotkeys",
              "changePortalColorShortcut",
              new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift),
              "Keyboard shortcut to change (or clear) the color of the hovered/targeted portal.");

      TargetPortalColor =
          config.Bind("Color", "targetPortalColor", Color.cyan, "Target color to set the portal glow effect to.");

      TargetPortalColorHex =
          config.Bind(
              "Color",
              "targetPortalColorHex",
              $"#{ColorUtility.ToHtmlStringRGB(Color.cyan)}",
              "Target color to set the portal glow effect to, in HTML hex form.");

      TargetPortalColor.SettingChanged += UpdateColorHexValue;
      TargetPortalColorHex.SettingChanged += UpdateColorValue;

      ShowChangeColorHoverText =
          config.Bind(
              "Hud", "showChangeColorHoverText", true, "Show the 'change color' text when hovering over a portal.");
    }

    static void UpdateColorHexValue(object sender, EventArgs eventArgs) {
      TargetPortalColorHex.Value = $"#{GetColorHtmlString(TargetPortalColor.Value)}";
    }

    static void UpdateColorValue(object sender, EventArgs eventArgs) {
      if (ColorUtility.TryParseHtmlString(TargetPortalColorHex.Value, out Color color)) {
        TargetPortalColor.Value = color;
      }
    }

    static string GetColorHtmlString(Color color) {
      return color.a == 1.0f
          ? ColorUtility.ToHtmlStringRGB(color)
          : ColorUtility.ToHtmlStringRGBA(color);
    }
  }
}
