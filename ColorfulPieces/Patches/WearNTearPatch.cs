using HarmonyLib;

using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces {
  [HarmonyPatch(typeof(WearNTear))]
  static class WearNTearPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(WearNTear.Awake))]
    static void WearNTearAwakePostfix(ref WearNTear __instance) {
      if (IsModEnabled.Value) {
        __instance.gameObject.AddComponent<PieceColor>();
      }
    }
  }
}
