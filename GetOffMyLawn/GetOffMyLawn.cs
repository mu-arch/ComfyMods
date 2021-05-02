using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace GetOffMyLawn {
  [BepInPlugin(GetOffMyLawn.Package, GetOffMyLawn.ModName, GetOffMyLawn.Version)]
  public class GetOffMyLawn : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.getoffmylawn";
    public const string Version = "0.0.2";
    public const string ModName = "Get Off My Lawn";

    private static ConfigEntry<bool> isModEnabled;
    private static ConfigEntry<float> pieceHealth;
    private const float _wardRadius = 30f;

    private readonly Harmony harmony = new Harmony("redseiko.valheim.getoffmylawn");

    public void Awake() {
      isModEnabled = Config.Bind("Global", "isModEnabled", true, "Whether the mod should be enabled.");

      pieceHealth =
          Config.Bind(
              "PieceValues",
              "PieceHealth",
              100000f,
              "Target value to set piece health to when creating and repairing.");

      harmony.PatchAll();
    }

    public void OnDestroy() {
      harmony.UnpatchSelf();
    }

    [HarmonyPatch(typeof(BaseAI))]
    class BaseAiPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(BaseAI.Awake))]
      private static void AwakePostfix(ref BaseAI __instance) {
        if (!isModEnabled.Value) {
          return;
        }

        __instance.m_monsterTargetRayMask = LayerMask.GetMask(
            new string[] {
                "Default",
                "static_solid",
                "Default_small",
                "vehicle"
            });
      }
    }

    [HarmonyPatch(typeof(Piece))]
    class PiecePatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Piece.IsValidMonsterTarget))]
      private static bool IsValidMonsterTargetPrefix(ref bool __result) {
        if (!isModEnabled.Value) {
          return true;
        }

        __result = false;
        return false;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Piece.SetCreator))]
      private static void SetCreatorPostfix(ref Piece __instance) {
        if (!isModEnabled.Value) {
          return;
        }

        if (__instance.m_category == Piece.PieceCategory.Building
            || __instance.m_category == Piece.PieceCategory.Crafting
            || __instance.m_category == Piece.PieceCategory.Furniture) {

          ZLog.Log("Creating piece '" + __instance.m_name + "' with health: " + pieceHealth.Value);
          __instance.m_nview.GetZDO().Set("health", pieceHealth.Value);
        }
      }
    }

    [HarmonyPatch(typeof(PrivateArea))]
    class PrivateAreaPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(PrivateArea.Interact))]
      private static void InteractPostfix(ref PrivateArea __instance) {
        if (!isModEnabled.Value) {
          return;
        }

        if (!__instance.IsEnabled()) {
          return;
        }

        Vector3 position = __instance == null
            ? Player.m_localPlayer.transform.position
            : __instance.transform.position;

        List<Piece> pieces = new List<Piece>();
        Piece.GetAllPiecesInRadius(position, _wardRadius, pieces);

        int pieceCount = 0;

        foreach (var piece in pieces) {
          if (piece.m_category == Piece.PieceCategory.Building
              || piece.m_category == Piece.PieceCategory.Crafting
              || piece.m_category == Piece.PieceCategory.Furniture) {
            piece.m_nview.GetZDO().Set("health", pieceHealth.Value);
            pieceCount++;
          }
        }

        ZLog.Log("Repairing " + pieceCount + " pieces to health: " + pieceHealth.Value);
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Player.Repair))]
      private static void RepairPrefix(ref Player __instance, ItemDrop.ItemData toolItem, Piece repairPiece) {
        if (!isModEnabled.Value) {
          return;
        }

        if (!__instance.InPlaceMode()) {
          return;
        }

        Piece hoveringPiece = __instance.GetHoveringPiece();

        if (hoveringPiece == null) {
          return;
        }

        if (!__instance.CheckCanRemovePiece(hoveringPiece)) {
          return;
        }

        if (!PrivateArea.CheckAccess(hoveringPiece.transform.position, 0f, /*flash=*/ true, /*wardCheck=*/ false)) {
          return;
        }

        ZLog.Log("Repairing piece '" + hoveringPiece.m_name + "' to health: " + pieceHealth.Value);
        hoveringPiece.m_nview.GetZDO().Set("health", pieceHealth.Value);
      }
    }
  }
}
