using BepInEx.Configuration;

namespace LicensePlate {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<float> ShipNameCutoffDistance { get; private set; }
    public static ConfigEntry<float> CartNameCutoffDistance { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      ShipNameCutoffDistance =
          config.Bind(
              "ShipName",
              "shipNameCutoffDistance",
              25f,
              new ConfigDescription(
                  "Cutoff distance for ShipName to appear/disappear.", new AcceptableValueRange<float>(1f, 40f)));

      CartNameCutoffDistance =
          config.Bind(
              "CartName",
              "cartNameCutoffDistance",
              10f,
              new ConfigDescription(
                  "Cutoff distance for CartName to appear/disappear.", new AcceptableValueRange<float>(1f, 25f)));
    }
  }
}
