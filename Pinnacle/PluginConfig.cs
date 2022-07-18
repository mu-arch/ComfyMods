using BepInEx.Configuration;

using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Pinnacle {
  public class PluginConfig {
    public static ConfigFile Config { get; private set; }

    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static ConfigEntry<KeyboardShortcut> PinListPanelToggleShortcut { get; private set; }
    public static ConfigEntry<float> CenterMapLerpDuration { get; private set; }

    public static void BindConfig(ConfigFile config) {
      if (Config != null) {
        return;
      }

      Config = config;

      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      PinListPanelToggleShortcut =
          config.Bind(
              "PinListPanel",
              "pinListPanelToggleShortcut",
              new KeyboardShortcut(KeyCode.Tab),
              "Keyboard shortcut to toggle the PinListPanel on/off.");

      CenterMapLerpDuration =
          config.Bind("CenterMap", "lerpDuration", 1f, "Duration (in seconds) for the CenterMap lerp.");
    }
  }

  public class MinimapConfig {
    static bool _isConfigBound = false;

    public static ConfigEntry<string> PinFont { get; private set; }
    public static ConfigEntry<int> PinFontSize { get; private set; }

    public static void BindConfig(ConfigFile config) {
      if (_isConfigBound) {
        return;
      }

      PinFont =
          config.Bind(
              "Minimap",
              "Pin.Font",
              defaultValue: "Norsebold",
              new ConfigDescription(
                  "The font for the Pin text on the Minimap.",
                  new AcceptableValueList<string>(
                      Resources.FindObjectsOfTypeAll<Font>().Select(f => f.name).OrderBy(f => f).ToArray())));

      PinFontSize =
          config.Bind(
              "Minimap",
              "Pin.FontSize",
              defaultValue: 18,
              new ConfigDescription(
                  "The font size for the Pin text on the Minimap.", new AcceptableValueRange<int>(2, 26)));

      PinFont.SettingChanged += (_, _) => SetMinimapPinFont();
      PinFontSize.SettingChanged += (_, _) => SetMinimapPinFont();

      _isConfigBound = true;
    }

    public static void SetMinimapPinFont() {
      SetMinimapPinFont(Minimap.m_instance, UIResources.FindFont(PinFont.Value), PinFontSize.Value);
    }

    static void SetMinimapPinFont(Minimap minimap, Font font, int fontSize) {
      if (!minimap || !font) {
        return;
      }

      foreach (Text text in minimap.m_nameInput.GetComponentsInChildren<Text>(includeInactive: true)) {
        text.SetFont(font);
      }

      foreach (Text text in minimap.m_pinPrefab.GetComponentsInChildren<Text>(includeInactive: true)) {
        text.SetFont(font)
            .SetFontSize(fontSize)
            .SetResizeTextForBestFit(false);
      }

      foreach (Text text in minimap.m_pinRootLarge.GetComponentsInChildren<Text>(includeInactive: true)) {
        text.SetFont(font)
            .SetFontSize(fontSize)
            .SetResizeTextForBestFit(false);
      }
    }
  }
}
