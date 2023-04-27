using HarmonyLib;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn {
  [HarmonyPatch(typeof(ZoneSystem))]
  static class ZoneSystemPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.Start))]
    static void StartPostfix(ref ZoneSystem __instance) {
      if (IsModEnabled.Value) {
        PotteryBarn.AddPieces();
      }
    }
  }
}
