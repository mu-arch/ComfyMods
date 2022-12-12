using BepInEx.Configuration;

using ComfyLib;

namespace SearsCatalog {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<int> BuildHudPanelRows { get; private set; }
    public static ConfigEntry<int> BuildHudPanelColumns { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      BuildHudPanelRows =
          config.BindInOrder(
              "BuildHud.Panel",
              "buildHudPanelRows",
              defaultValue: 7,
              "BuildHud.Panel visible rows (vanilla: 7).",
              new AcceptableValueRange<int>(1, 14));

      BuildHudPanelRows.SettingChanged += (_, _) => SearsCatalog.SetupBuildHudPanel();

      BuildHudPanelColumns =
          config.BindInOrder(
              "BuildHud.Panel",
              "buildHudPanelColumns",
              defaultValue: 13,
              "BuildHud.Panel visible columns (vanilla: 13).",
              new AcceptableValueRange<int>(1, 26));

      BuildHudPanelColumns.SettingChanged += (_, _) => SearsCatalog.SetupBuildHudPanel();
    }
  }
}
