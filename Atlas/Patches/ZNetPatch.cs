using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZNet))]
  static class ZNetPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNet.LoadWorld))]
    static void LoadWorldPostfix(ref ZNet __instance) {
      ZLog.Log($"Finished loading world file. ZNet.m_netTime is: {__instance.m_netTime}");
    }
  }
}
