using BepInEx.Configuration;

using UnityEngine;

namespace Insightful {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static ConfigEntry<KeyboardShortcut> ReadHiddenTextShortcut { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ReadHiddenTextShortcut =
          config.Bind(
              "Hotkeys",
              "readHiddenTextShortcut",
              new KeyboardShortcut(KeyCode.R, KeyCode.RightShift),
              "Shortcut to read hidden text inscriptions embedded within objects.");
    }
  }
}
