using HarmonyLib;

using static SearsCatalog.PluginConfig;
using static UnityEngine.Random;

namespace SearsCatalog {
  [HarmonyPatch(typeof(PieceTable))]
  static class PieceTablePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.SetCategory))]
    static void SetCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.RefreshPieceListPanel();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.PrevCategory))]
    static void PrevCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.RefreshPieceListPanel();
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.NextCategory))]
    static void NextCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.RefreshPieceListPanel();
      }
    }
  }
}
