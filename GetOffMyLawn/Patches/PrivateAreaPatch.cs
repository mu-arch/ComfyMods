using HarmonyLib;

using System.Collections.Generic;

using UnityEngine;

using static GetOffMyLawn.GetOffMyLawn;
using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(PrivateArea))]
  static class PrivateAreaPatch {
    static readonly List<Piece> _pieceCache = new();
    static int _pieceCount = 0;

    static void GetAllPiecesInRadius(Vector3 origin, float radius, List<Piece> pieces) {
      foreach (Piece piece in Piece.s_allPieces) {
        if (piece.gameObject.layer == Piece.s_ghostLayer
            || Vector3.Distance(origin, piece.transform.position) >= radius) {
          continue;
        }

        pieces.Add(piece);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.Interact))]
    static void InteractPostfix(ref PrivateArea __instance) {
      if (!IsModEnabled.Value || !__instance || !__instance.IsEnabled() || !__instance.m_piece.IsCreator()) {
        return;
      }

      _pieceCache.Clear();
      _pieceCount = 0;

      GetAllPiecesInRadius(__instance.transform.position, __instance.m_radius, _pieceCache);

      foreach (Piece piece in _pieceCache) {
        if (!piece || !piece.m_nview || !piece.m_nview.IsValid()) {
          PluginLogger.LogWarning(
              $"Skipping piece with invalid ZNetView: {Localization.m_instance.Localize(piece.Ref()?.m_name)}.");

          continue;
        }

        if (piece.GetComponent<Plant>()) {
          continue;
        }

        piece.m_nview.GetZDO().Set(HealthHashCode, TargetPieceHealth.Value);

        if (ShowRepairEffectOnWardActivation.Value) {
          piece.m_placeEffect?.Create(piece.transform.position, piece.transform.rotation);
        }

        _pieceCount++;
      }

      PluginLogger.LogInfo($"Repaired {_pieceCount} pieces to health: {TargetPieceHealth.Value}");

      if (ShowTopLeftMessageOnPieceRepair.Value) {
        Player.m_localPlayer.Message(
            MessageHud.MessageType.TopLeft, $"Repaired {_pieceCount} pieces to health: {TargetPieceHealth.Value}");
      }

      _pieceCache.Clear();
    }
  }
}
