using BepInEx.Configuration;

namespace Keysential {
  public static class PluginConfig {
    public static ConfigEntry<string> GlobalKeysOverrideList { get; private set; }

    public static void BindConfig(ConfigFile config) {
      GlobalKeysOverrideList =
          config.Bind(
              "ZoneSystem",
              "globalkeysOverrideList",
              string.Empty,
              "If set, server will maintain this constant list of comma-delimited global keys.");
    }
  }
}
