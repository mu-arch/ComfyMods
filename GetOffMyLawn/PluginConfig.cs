using BepInEx.Configuration;

namespace GetOffMyLawn {
  public class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static ConfigEntry<float> PieceHealth { get; private set; }

    public static ConfigEntry<bool> EnablePieceHealthDamageThreshold { get; private set; }

    public static ConfigEntry<bool> ShowTopLeftMessageOnPieceRepair { get; private set; }
    public static ConfigEntry<bool> ShowRepairEffectOnWardActivation { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.Bind("Global", "isModEnabled", true, "Globally enable or disable this mod.");

      PieceHealth =
          config.Bind(
              "PieceValue",
              "targetPieceHealth",
              100_000_000_000_000_000f,
              "Target value to set piece health to when creating and repairing.");

      EnablePieceHealthDamageThreshold =
          config.Bind(
              "Optimization",
              "enablePieceHealthDamageThreshold",
              true,
              "If piece health exceeds 100K, DO NOT execute ApplyDamage() or send WNTHealthChanged messages.");

      ShowTopLeftMessageOnPieceRepair =
          config.Bind(
              "Indicators",
              "showTopLeftMessageOnPieceRepair",
              false,
              "Shows a message in the top-left message area on piece repair.");

      ShowRepairEffectOnWardActivation =
          config.Bind(
              "Indicators",
              "showRepairEffectOnWardActivation",
              false,
              "Shows the repair effect on affected pieces when activating a ward.");
    }
  }
}
