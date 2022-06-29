using HarmonyLib;

using System.Collections.Generic;

using static GetOffMyLawn.GetOffMyLawn;
using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(PrivateArea))]
  public class PrivateAreaPatch {
    static readonly List<Piece> PieceCache = new();
    static int PieceCount = 0;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PrivateArea.Interact))]
    static void InteractPostfix(ref PrivateArea __instance) {
      if (!IsModEnabled.Value || !__instance || !__instance.IsEnabled() || !__instance.m_piece.IsCreator()) {
        return;
      }

      PieceCache.Clear();
      PieceCount = 0;

      Piece.GetAllPiecesInRadius(__instance.transform.position, __instance.m_radius, PieceCache);

      foreach (Piece piece in PieceCache) {
        if (!piece || !piece.m_nview || !piece.m_nview.IsValid()) {
          PluginLogger.LogWarning(
              $"Skipping piece with invalid ZNetView: {Localization.instance.Localize(piece?.m_name ?? "null")}.");

          continue;
        }

        if (piece.GetComponent<Plant>()) {
          continue;
        }

        piece.m_nview.GetZDO().Set(HealthHashCode, PieceHealth.Value);

        if (ShowRepairEffectOnWardActivation.Value) {
          piece.m_placeEffect?.Create(piece.transform.position, piece.transform.rotation);
        }

        PieceCount++;
      }

      PluginLogger.LogInfo($"Repaired {PieceCount} pieces to health: {PieceHealth.Value}");

      if (ShowTopLeftMessageOnPieceRepair.Value) {
        Player.m_localPlayer.Message(
            MessageHud.MessageType.TopLeft, $"Repaired {PieceCount} pieces to health: {PieceHealth.Value}");
      }

      PieceCache.Clear();
    }
  }
}
