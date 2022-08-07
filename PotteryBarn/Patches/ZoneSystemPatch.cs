using HarmonyLib;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn.Patches {
  [HarmonyPatch(typeof(ZoneSystem))]
  public class ZoneSystemPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.Start))]
    static void StartPostfix(ref ZoneSystem __instance) {
      if (IsModEnabled.Value) {
        __instance.StartCoroutine(PotteryBarn.AddPieces());
      }
    }
  }
}
