using System;

using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace ColorfulPortals {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> ChangePortalColorShortcut { get; private set; }

    public static ConfigEntry<Color> TargetPortalColor { get; private set; }
    public static ConfigEntry<string> TargetPortalColorHex { get; private set; }

    public static ConfigEntry<bool> ShowChangeColorHoverText { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ChangePortalColorShortcut =
          config.BindInOrder(
              "Hotkeys",
              "changePortalColorShortcut",
              new KeyboardShortcut(KeyCode.E, KeyCode.LeftShift),
              "Keyboard shortcut to change (or clear) the color of the hovered/targeted portal.");

      TargetPortalColor =
          config.BindInOrder("Color", "targetPortalColor", Color.cyan, "Target color to set the portal glow effect to.");

      TargetPortalColorHex =
          config.BindInOrder(
              "Color",
              "targetPortalColorHex",
              $"#{ColorUtility.ToHtmlStringRGB(Color.cyan)}",
              "Target color to set the portal glow effect to, in HTML hex form.");

      TargetPortalColor.SettingChanged += UpdateColorHexValue;
      TargetPortalColorHex.SettingChanged += UpdateColorValue;

      ShowChangeColorHoverText =
          config.BindInOrder(
              "Hud", "showChangeColorHoverText", false, "Show the 'change color' text when hovering over a portal.");

      BindUpdateColorsConfig(config);
    }

    public static ConfigEntry<int> UpdateColorsFrameLimit { get; private set; }
    public static ConfigEntry<float> UpdateColorsWaitInterval { get; private set; }

    static void BindUpdateColorsConfig(ConfigFile config) {
      UpdateColorsFrameLimit =
          config.BindInOrder(
              "UpdateColors",
              "updateColorsFrameLimit",
              100,
              "Limit for how many TelepwortWorldColor.UpdateColors to process per update frame.",
              new AcceptableValueRange<int>(50, 250));

      UpdateColorsWaitInterval =
          config.BindInOrder(
              "UpdateColors",
              "updateColorsWaitInterval",
              5f,
              "Interval to wait after each TelepwortWorldColor.UpdateColors loop. *Restart required!*",
              new AcceptableValueRange<float>(0.5f, 10f));
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
