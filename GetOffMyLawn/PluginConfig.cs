using BepInEx.Configuration;

using ComfyLib;

namespace GetOffMyLawn {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static FloatConfigEntry TargetPieceHealth { get; private set; }

    public static ConfigEntry<bool> EnablePieceHealthDamageThreshold { get; private set; }

    public static ConfigEntry<bool> ShowTopLeftMessageOnPieceRepair { get; private set; }
    public static ConfigEntry<bool> ShowRepairEffectOnWardActivation { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      TargetPieceHealth =
          new(
              config,
              "PieceValue",
              "targetPieceHealth",
              100_000_000_000_000_000f,
              "Target value to set piece health to when creating and repairing.");

      EnablePieceHealthDamageThreshold =
          config.BindInOrder(
              "Optimization",
              "enablePieceHealthDamageThreshold",
              true,
              "If piece health exceeds 100K, DO NOT execute ApplyDamage() or send WNTHealthChanged messages.");

      ShowTopLeftMessageOnPieceRepair =
          config.BindInOrder(
              "Indicators",
              "showTopLeftMessageOnPieceRepair",
              false,
              "Shows a message in the top-left message area on piece repair.");

      ShowRepairEffectOnWardActivation =
          config.BindInOrder(
              "Indicators",
              "showRepairEffectOnWardActivation",
              false,
              "Shows the repair effect on affected pieces when activating a ward.");
    }
  }
}
