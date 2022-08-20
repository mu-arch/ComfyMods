using BepInEx.Configuration;

using UnityEngine;

namespace Recipedia {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<KeyboardShortcut> RecipeListPanelToggleShortcut { get; private set; }
    public static ConfigEntry<float> RecipeListPanelToggleLerpDuration { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled ??= config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      RecipeListPanelToggleShortcut =
          config.Bind(
              "RecipeListPanel.Toggle",
              "recipeListPanelToggleShortcut",
              new KeyboardShortcut(KeyCode.Y, KeyCode.RightShift),
              "Keyboard shortcut to toggle the RecipeListPanel.");

      RecipeListPanelToggleLerpDuration =
          config.Bind(
              "RecipeListPanel.Toggle",
              "recipeListPanelToggleLerpDuration",
              0.25f,
              new ConfigDescription(
                  "Duration (in seconds) for the RecipeListPanel on/off lerp.",
                  new AcceptableValueRange<float>(0f, 3f)));
    }
  }
}
