using HarmonyLib;

using static PotteryBarn.PluginConfig;
using static PotteryBarn.PotteryBarn;

namespace PotteryBarn.Patches {
  [HarmonyPatch(typeof(Piece))]
  public static class PiecePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Piece.CanBeRemoved))]
    static void CanBeRemovedPostfix(ref Piece __instance, ref bool __result) {
      if (!IsModEnabled.Value) {
        return;
      }

      if (CustomPiecePrefabNames.Contains(Utils.GetPrefabName(__instance.gameObject))) {
        __result = __instance.m_creator != 0L;
        ZLog.Log($"Can piece {__instance.name} be removed (CreatorId: {__instance.m_creator}): {__result} ");
      }
    }
  }
}
