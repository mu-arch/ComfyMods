using HarmonyLib;

using static SearsCatalog.PluginConfig;

namespace SearsCatalog {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      if (IsModEnabled.Value) {
        SearsCatalog.TogglePieceListPanel(toggleOn: false);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.TogglePieceSelection))]
    static void TogglePieceSelectionPostfix(ref Hud __instance) {
      if (IsModEnabled.Value) {
        SearsCatalog.TogglePieceListPanel(toggleOn: __instance.m_pieceSelectionWindow.activeSelf);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.HidePieceSelection))]
    static void HidePieceSelectionPostfix() {
      if (IsModEnabled.Value) {
        SearsCatalog.TogglePieceListPanel(toggleOn: false);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hud.UpdatePieceList))]
    static void UpdatePieceListPrefix(ref Hud __instance, ref Piece.PieceCategory category, ref bool __state) {
      if (IsModEnabled.Value) {
        __state = __instance.m_lastPieceCategory == category;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdatePieceList))]
    static void UpdatePieceListPostfix(ref Hud __instance, ref bool __state) {
      if (IsModEnabled.Value && __state) {
        SearsCatalog.RefreshPieceListPanel();
      }
    }
  }
}
