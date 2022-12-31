using HarmonyLib;

using static Keysential.PluginConfig;

namespace Keysential {
  [HarmonyPatch(typeof(ZoneSystem))]
  static class ZoneSystemPatch {
    static readonly char[] _commaSeparator = new char[] { ',' };

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.Load))]
    static void LoadPostfix(ZoneSystem __instance) {
      if (!ZNet.m_isServer || string.IsNullOrEmpty(GlobalKeysOverrideList.Value)) {
        return;
      }

      ZLog.Log($"Saved ZoneSystem.m_globalKeys are:\n{string.Join(",", __instance.m_globalKeys)}");
      __instance.m_globalKeys.Clear();

      foreach (
          string globalKey in
              GlobalKeysOverrideList.Value.Split(_commaSeparator, System.StringSplitOptions.RemoveEmptyEntries)) {

        __instance.m_globalKeys.Add(globalKey.Trim());
      }

      ZLog.Log($"Overriding ZoneSystem.m_globalKeys to:\n{string.Join(",", __instance.m_globalKeys)}");

      ZLog.Log($"Adding VendorKeyManager component to ZoneSystem...");
      __instance.gameObject.AddComponent<VendorKeyManager>();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZoneSystem.RPC_SetGlobalKey))]
    static bool RPC_SetGlobalKeyPrefix(ZoneSystem __instance, long sender, string name) {
      if (!ZNet.m_isServer || string.IsNullOrEmpty(GlobalKeysOverrideList.Value)) {
        return true;
      }

      if (!__instance.m_globalKeys.Contains(name)) {
        ZLog.Log($"Ignoring globalKey '{name}' from senderId: {sender}");
      }

      return false;
    }
  }
}
