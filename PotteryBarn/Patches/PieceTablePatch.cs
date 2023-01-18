using HarmonyLib;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn {
  [HarmonyPatch(typeof(PieceTable))]
  static class PieceTablePatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PieceTable.UpdateAvailable))]
    static void UpdateAvailablePrefix(ref PieceTable __instance) {
      if (IsModEnabled.Value) {
        while (__instance.m_availablePieces.Count < __instance.m_selectedPiece.Length) {
          __instance.m_availablePieces.Add(new());
        }
      }
    }
  }
}
