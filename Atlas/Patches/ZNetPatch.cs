using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZNet))]
  static class ZNetPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZNet.SaveWorld))]
    static bool SaveWorldPrefix(ref ZNet __instance, ref bool sync) {
      if (sync) {
        return true;
      }

      __instance.StartCoroutine(Atlas.SaveWorldCoroutine(__instance));
      return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNet.LoadWorld))]
    static void LoadWorldPostfix(ref ZNet __instance) {
      ZLog.Log($"Finished loading world file. m_netTime is: {__instance.m_netTime}");
    }
  }
}
