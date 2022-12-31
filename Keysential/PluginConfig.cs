using BepInEx.Configuration;

namespace Keysential {
  public static class PluginConfig {
    public static ConfigEntry<string> GlobalKeysOverrideList { get; private set; }
    public static ConfigEntry<string> GlobalKeysAllowedList { get; private set; }

    public static void BindConfig(ConfigFile config) {
      GlobalKeysOverrideList =
          config.Bind(
              "ZoneSystem",
              "globalKeysOverrideList",
              string.Empty,
              "If set, server will maintain this constant list of comma-delimited global keys.");

      GlobalKeysAllowedList =
          config.Bind(
              "ZoneSystem",
              "globalKeysAllowedList",
              string.Empty,
              "If set, server will only accept these global keys (comma-delimited) in RPC_SetGlobalKey().");
    }
  }
}
