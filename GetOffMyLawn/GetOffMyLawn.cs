using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace GetOffMyLawn {
  [BepInPlugin(Package, ModName, Version)]
  public class GetOffMyLawn : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.getoffmylawn";
    public const string Version = "0.1.1";
    public const string ModName = "Get Off My Lawn";

    private static ConfigEntry<bool> _isModEnabled;
    private static ConfigEntry<float> _pieceHealth;

    private static ConfigEntry<bool> _showTopLeftMessageOnPieceRepair;
    private static ConfigEntry<bool> _showRepairEffectOnWardActivation;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind("Global", "isModEnabled", true, "Whether the mod should be enabled.");

      _pieceHealth =
          Config.Bind(
              "PieceValues",
              "pieceHealth",
              100000f,
              "Target value to set piece health to when creating and repairing.");

      _showTopLeftMessageOnPieceRepair =
          Config.Bind(
              "Indicators",
              "showTopLeftMessageOnPieceRepair",
              false,
              "Shows a message in the top-left message area on piece repair.");

      _showRepairEffectOnWardActivation =
          Config.Bind(
              "Indicators",
              "showRepairEffectOnWardActivation",
              false,
              "Shows the repair effect on affected pieces when activating a ward.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchSelf();
      }
    }

    [HarmonyPatch(typeof(BaseAI))]
    private class BaseAiPatch {
      private static readonly string[] _targetRayMask = new string[] {
        "Default", "static_solid", "Default_small", "vehicle",
      };

      [HarmonyPostfix]
      [HarmonyPatch(nameof(BaseAI.Awake))]
      private static void AwakePostfix(ref BaseAI __instance) {
        if (_isModEnabled.Value) {
          __instance.m_monsterTargetRayMask = LayerMask.GetMask(_targetRayMask);
        }
      }
    }

    [HarmonyPatch(typeof(MonsterAI))]
    private class MonsterAiPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(MonsterAI.UpdateTarget))]
      private static void UpdateTargetPrefix(ref MonsterAI __instance) {
        if (_isModEnabled.Value) {
          __instance.m_attackPlayerObjects = false;
        }
      }
    }

    [HarmonyPatch(typeof(Piece))]
    private class PiecePatch {
      private static readonly int _healthHashcode = "health".GetStableHashCode();

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Piece.SetCreator))]
      private static void SetCreatorPostfix(ref Piece __instance) {
        if (!_isModEnabled.Value || !__instance || !__instance.m_nview || __instance.GetComponent<Plant>()) {
          return;
        }

        if (__instance.m_category == Piece.PieceCategory.Misc
            || __instance.m_category == Piece.PieceCategory.Building
            || __instance.m_category == Piece.PieceCategory.Crafting
            || __instance.m_category == Piece.PieceCategory.Furniture) {
          ZLog.Log(string.Format(
              "Creating piece '{0}' with health: {1}",
              Localization.instance.Localize(__instance.m_name),
              _pieceHealth.Value));

          __instance.m_nview.GetZDO().Set(_healthHashcode, _pieceHealth.Value);
        }
      }
    }

    [HarmonyPatch(typeof(PrivateArea))]
    private class PrivateAreaPatch {
      private static readonly int _healthHashcode = "health".GetStableHashCode();
      private static readonly List<Piece> _pieces = new List<Piece>();
      private static int _pieceCount = 0;

      [HarmonyPostfix]
      [HarmonyPatch(nameof(PrivateArea.Interact))]
      private static void InteractPostfix(ref PrivateArea __instance) {
        if (!_isModEnabled.Value || !__instance || !__instance.IsEnabled() || !__instance.m_piece.IsCreator()) {
          return;
        }

        _pieces.Clear();
        _pieceCount = 0;

        Piece.GetAllPiecesInRadius(__instance.transform.position, __instance.m_radius, _pieces);

        foreach (var piece in _pieces) {
          if (piece.GetComponent<Plant>()) {
            continue;
          }

          if (piece.m_category == Piece.PieceCategory.Misc
              || piece.m_category == Piece.PieceCategory.Building
              || piece.m_category == Piece.PieceCategory.Crafting
              || piece.m_category == Piece.PieceCategory.Furniture) {
            piece.m_nview.GetZDO().Set(_healthHashcode, _pieceHealth.Value);

            if (_showRepairEffectOnWardActivation.Value) {
              piece.m_placeEffect.Create(piece.transform.position, piece.transform.rotation);
            }

            _pieceCount++;
          }
        }

        ZLog.Log(string.Format("Repaired {0} pieces to health: {1}", _pieceCount, _pieceHealth.Value));

        if (_showTopLeftMessageOnPieceRepair.Value) {
          Player.m_localPlayer.Message(
              MessageHud.MessageType.TopLeft,
              string.Format("Repaired {0} pieces to health: {1}", _pieceCount, _pieceHealth.Value));
        }
      }
    }

    [HarmonyPatch(typeof(Player))]
    private class PlayerPatch {
      private static readonly int _healthHashcode = "health".GetStableHashCode();

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Player.Repair))]
      private static void RepairPrefix(ref Player __instance, ItemDrop.ItemData toolItem, Piece repairPiece) {
        if (!_isModEnabled.Value || !__instance || !__instance.InPlaceMode()) {
          return;
        }

        Piece hoveringPiece = __instance.m_hoveringPiece;

        if (!hoveringPiece || !__instance.CheckCanRemovePiece(hoveringPiece) || hoveringPiece.GetComponent<Plant>()) {
          return;
        }

        string pieceName = Localization.instance.Localize(hoveringPiece.m_name);

        if (!PrivateArea.CheckAccess(hoveringPiece.transform.position, radius: 0f, flash: false, wardCheck: false)) {
          ZLog.Log(string.Format("Unable to repair piece '{0}' due to ward in range.", pieceName));
          return;
        }

        hoveringPiece.m_nview.GetZDO().Set(_healthHashcode, _pieceHealth.Value);
        ZLog.Log(string.Format("Repaired piece '{0}' to health: {1}", pieceName, _pieceHealth.Value));

        if (_showTopLeftMessageOnPieceRepair.Value) {
          __instance.Message(
              MessageHud.MessageType.TopLeft,
              string.Format("Repaired piece '{0}' to health: {1}", pieceName, _pieceHealth.Value));
        }
      }
    }
  }
}
