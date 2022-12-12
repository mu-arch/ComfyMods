using HarmonyLib;

using static SearsCatalog.PluginConfig;

namespace SearsCatalog {
  [HarmonyPatch(typeof(PieceTable))]
  static class PieceTablePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.SetCategory))]
    static void SetCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.PrevCategory))]
    static void PrevCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PieceTable.NextCategory))]
    static void NextCategoryPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.BuildHudNeedRefresh = true;
      }
    }
  }
}
