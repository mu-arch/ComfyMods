using BepInEx.Configuration;

using UnityEngine;

namespace Silence {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> ToggleSilenceShortcut { get; private set; }
    public static ConfigEntry<bool> HideChatWindow { get; private set; }
    public static ConfigEntry<bool> HideInWorldTexts { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled =
          config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      ToggleSilenceShortcut =
          config.Bind(
              "Silence",
              "toggleSilenceShortcut",
              new KeyboardShortcut(KeyCode.S, KeyCode.RightControl),
              "Shortcut to toggle silence.");

      HideChatWindow = config.Bind("Silence", "hideChatWindow", true, "When silenced, chat window is hidden.");
      HideInWorldTexts = config.Bind("Silence", "hideInWorldTexts", true, "When silenced, hides text in-world.");
    }
  }
}
