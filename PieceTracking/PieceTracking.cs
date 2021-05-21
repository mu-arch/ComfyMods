using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace PieceTracking {
  [BepInPlugin(PieceTracking.Package, PieceTracking.ModName, PieceTracking.Version)]
  public class PieceTracking : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.piecetracking";
    public const string Version = "0.0.1";
    public const string ModName = "Piece Tracking";

    private Harmony _harmony;
    public static ManualLogSource Log;

    private void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchAll(null);
      }
    }

    [HarmonyPatch(typeof(Hud))]
    private class HudPatch {
      private static Transform _pieceInfoText;

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      private static void HudAwakePostfix(Hud __instance) {
        _pieceInfoText = Instantiate(__instance.m_healthText, __instance.m_pieceHealthRoot.transform).transform;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Hud.OnDestroy))]
      private static void HudOnDestroyPrefix(Hud __instance) {
        Destroy(_pieceInfoText);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
      private static void HudUpdateCrossHairPostfix(Hud __instance, Player player, float bowDrawPercentage) {
        Piece piece = player.GetHoveringPiece();

        if (piece == null) {
          return;
        }

        __instance.m_pieceHealthRoot.Find("_PieceInfoText");

        __instance.m_hoverName.text = "(CreatorId: " + piece.GetCreator() + " )" + __instance.m_hoverName.text;
      }
    }
  }
}
