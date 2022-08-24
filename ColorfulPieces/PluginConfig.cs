using BepInEx.Configuration;
using System;

using UnityEngine;

namespace ColorfulPieces {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> ChangePieceColorShortcut { get; private set; }
    public static ConfigEntry<KeyboardShortcut> ClearPieceColorShortcut { get; private set; }
    public static ConfigEntry<KeyboardShortcut> CopyPieceColorShortcut { get; private set; }

    public static ConfigEntry<Color> TargetPieceColor { get; private set; }
    public static ConfigEntry<string> TargetPieceColorHex { get; private set; }
    public static ConfigEntry<float> TargetPieceEmissionColorFactor { get; private set; }

    public static ConfigEntry<bool> ShowChangeRemoveColorPrompt { get; private set; }
    public static ConfigEntry<int> ColorPromptFontSize { get; private set; }

    public static Vector3 TargetPieceColorAsVec3 = Vector3.zero;

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ChangePieceColorShortcut =
          config.Bind(
              "Hotkeys",
              "changePieceColorShortcut",
              new KeyboardShortcut(KeyCode.R, KeyCode.LeftShift),
              "Shortcut to change the color of the hovered piece.");

      ClearPieceColorShortcut =
          config.Bind(
              "Hotkeys",
              "clearPieceColorShortcut",
              new KeyboardShortcut(KeyCode.R, KeyCode.LeftAlt),
              "Shortcut to clear the color of the hovered piece.");

      CopyPieceColorShortcut =
          config.Bind(
              "Hotkeys",
              "copyPieceColorShortcut",
              new KeyboardShortcut(KeyCode.R, KeyCode.LeftControl),
              "Shortcut to copy the color of the hovered piece.");

      TargetPieceColor =
          config.Bind("Color", "targetPieceColor", Color.cyan, "Target color to set the piece material to.");

      TargetPieceColorAsVec3 = Utils.ColorToVec3(TargetPieceColor.Value);

      TargetPieceColorHex =
          config.Bind(
              "Color",
              "targetPieceColorHex",
              $"#{ColorUtility.ToHtmlStringRGB(Color.cyan)}",
              "Target color to set the piece material to, in HTML hex form (alpha unsupported).");

      TargetPieceColor.SettingChanged += (s, e) => UpdateColorHexValue();
      TargetPieceColorHex.SettingChanged += (s, e) => UpdateColorValue();

      TargetPieceEmissionColorFactor =
          config.Bind(
              "Color",
              "targetPieceEmissionColorFactor",
              0.4f,
              new ConfigDescription(
                  "Factor to multiply the target color by and set as emission color.",
                  new AcceptableValueRange<float>(0f, 0.8f)));

      ShowChangeRemoveColorPrompt =
          config.Bind("Hud", "showChangeRemoveColorPrompt", true, "Show the 'change/remove/copy' color text prompt.");

      ColorPromptFontSize =
          config.Bind("Hud", "colorPromptFontSize", 15, "Font size for the 'change/remove/copy' color text prompt.");
    }

    static void UpdateColorHexValue() {
      Color color = TargetPieceColor.Value;
      color.a = 1.0f;

      TargetPieceColorHex.Value = $"#{ColorUtility.ToHtmlStringRGB(color)}";
      TargetPieceColor.Value = color;
      TargetPieceColorAsVec3 = Utils.ColorToVec3(color);
    }

    static void UpdateColorValue() {
      if (ColorUtility.TryParseHtmlString(TargetPieceColorHex.Value, out Color color)) {
        color.a = 1.0f;

        TargetPieceColor.Value = color;
        TargetPieceColorAsVec3 = Utils.ColorToVec3(color);
      }
    }
  }
}
