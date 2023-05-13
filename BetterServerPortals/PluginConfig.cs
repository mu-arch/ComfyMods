using System.Collections.Generic;
using System.Linq;

using BepInEx;
using BepInEx.Configuration;

namespace BetterServerPortals {
  public static class PluginConfig {
    public static ConfigEntry<float> ConnectPortalCoroutineWait { get; private set; }
    public static ConfigEntry<string> PortalPrefabNames { get; private set; }
    public static HashSet<int> PortalPrefabHashCodes { get; private set; }

    public static void BindConfig(ConfigFile config) {
      ConnectPortalCoroutineWait =
          config.Bind(
              "Portals",
              "connectPortalCoroutineWait",
              5f,
              "Wait time (seconds) when ConnectPortal coroutine yields.");

      PortalPrefabNames =
          config.Bind(
              "Portals",
              "portalPrefabNames",
              "portal_wood,portal",
              "Comma-separated list of portal prefab names to search for.");

      PortalPrefabHashCodes = CreatePrefabHashCodes(PortalPrefabNames.Value);
    }

    static HashSet<int> CreatePrefabHashCodes(string prefabsText) {
      return prefabsText
          .Split(',')
          .Select(p => p.Trim())
          .Where(p => !p.IsNullOrWhiteSpace())
          .Select(p => p.GetStableHashCode())
          .ToHashSet();
    }
  }
}
