using HarmonyLib;

namespace BetterServerPortals {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.Start))]
    static void StartPostfix(Game __instance) {
      if (ZNet.m_isServer) {
        __instance.StopCoroutine(nameof(Game.ConnectPortals));
        __instance.StartCoroutine(BetterServerPortals.ConnectPortalsCoroutine(__instance));
      }
    }
  }
}
