using BepInEx.Configuration;

namespace Atlas {
  public static class PluginConfig {
    public static ConfigEntry<bool> IgnoreGenerateLocationsIfNeeded { get; private set; }
    public static ConfigEntry<bool> IgnorePgwVersion { get; private set; }
    public static ConfigEntry<bool> IgnoreLocationVersion { get; private set; }

    public static ConfigFile BindConfig(ConfigFile config) {
      IgnoreGenerateLocationsIfNeeded =
          config.Bind(
              "ZoneSystem",
              "ignoreGenerateLocationsIfNeeded",
              true,
              "If set, ignores any calls to ZoneSystem.GenerateLocationsIfNeeded().");

      IgnorePgwVersion =
          config.Bind(
              "ZoneSystem",
              "ignorePgwVersion",
              true,
              "If set, ignores the ZoneSystem.m_pgwVersion check in ZoneSystem.Load().");

      IgnoreLocationVersion =
          config.Bind(
              "ZoneSystem",
              "ignoreLocationVersion",
              true,
              "If set, ignores the ZoneSystem.m_locationVersion check in ZoneSystem.Load().");

      return config;
    }
  }
}
