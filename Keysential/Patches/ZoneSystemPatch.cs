using System.Collections.Generic;

using ComfyLib;

using HarmonyLib;

using static Keysential.PluginConfig;

namespace Keysential {
  [HarmonyPatch(typeof(ZoneSystem))]
  static class ZoneSystemPatch {
    static readonly char[] _commaSeparator = new char[] { ',' };

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.Load))]
    static void LoadPostfix(ZoneSystem __instance) {
      if (!ZNet.m_isServer) {
        return;
      }

      ZLog.Log($"Saved ZoneSystem.m_globalKeys are:\n{string.Join(",", __instance.m_globalKeys)}");

      List<string> globalKeysOverrideList = GlobalKeysOverrideList.GetCachedStringList();

      if (globalKeysOverrideList.Count > 0) {
        __instance.m_globalKeys.Clear();
        __instance.m_globalKeys.UnionWith(globalKeysOverrideList);

        ZLog.Log($"Overriding ZoneSystem.m_globalKeys to:\n{string.Join(",", __instance.m_globalKeys)}");
      }

      ZLog.Log($"Adding VendorKeyManager component to ZoneSystem...");
      __instance.gameObject.AddComponent<VendorKeyManager>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZoneSystem.RPC_SetGlobalKey))]
    static bool RPC_SetGlobalKeyPrefix(ZoneSystem __instance, long sender, string name) {
      if (!ZNet.m_isServer || IsGlobalKeyAllowed(name)) {
        return true;
      }

      ZLog.Log($"Ignoring GlobalKey '{name}' from sender: {sender}");
      return false;
    }

    static bool IsGlobalKeyAllowed(string globalKey) {
      return
          GlobalKeysOverrideList.GetCachedStringList().IsEmptyOrContains(globalKey)
          && GlobalKeysAllowedList.GetCachedStringList().IsEmptyOrContains(globalKey);
    }
  }
}
