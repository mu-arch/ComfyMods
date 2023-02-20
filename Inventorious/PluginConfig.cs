using BepInEx.Configuration;

using ComfyLib;

namespace Inventorious {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<float> ShowTransitionDuration { get; private set; }
    public static ConfigEntry<float> HideTransitionDuration { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      IsModEnabled.SettingChanged += (_, _) => Inventorious.SetupInventoryGui(InventoryGui.m_instance);

      ShowTransitionDuration =
          config.BindInOrder(
              "Show.Transition",
              "showTransitionDuration",
              0.25f,
              "InventoryGui.Show transition duration.",
              new AcceptableValueRange<float>(0f, 2f));

      HideTransitionDuration =
          config.BindInOrder(
              "Hide.Transition",
              "hideTransitionDuration",
              0.25f,
              "InventoryGui.Hide transition duration.",
              new AcceptableValueRange<float>(0f, 2f));
    }
  }
}
