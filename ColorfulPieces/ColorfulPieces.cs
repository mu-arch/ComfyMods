using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulPieces : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfulpieces";
    public const string PluginName = "ColorfulPieces";
    public const string PluginVersion = "1.11.0";

    public static readonly int PieceColorHashCode = "PieceColor".GetStableHashCode();
    public static readonly int PieceEmissionColorFactorHashCode = "PieceEmissionColorFactor".GetStableHashCode();
    public static readonly int PieceLastColoredByHashCode = "PieceLastColoredBy".GetStableHashCode();
    public static readonly int PieceLastColoredByHostHashCode = "PieceLastColoredByHost".GetStableHashCode();

    static ManualLogSource _logger;
    Harmony _harmony;

    void Awake() {
      _logger = Logger;
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static bool ClaimOwnership(WearNTear wearNTear) {
      if (!wearNTear
          || !wearNTear.m_nview
          || !wearNTear.m_nview.IsValid()
          || !PrivateArea.CheckAccess(wearNTear.transform.position, flash: true)) {
        _logger.LogWarning("Piece does not have a valid ZNetView or is in a PrivateArea.");
        return false;
      }

      wearNTear.m_nview.ClaimOwnership();

      return true;
    }

    public static void ChangePieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      ChangePieceColorZdo(wearNTear.m_nview);

      if (wearNTear.TryGetComponent(out PieceColor pieceColor)) {
        pieceColor.UpdateColors();
      }

      wearNTear.m_piece.Ref()?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
    }

    // TODO(redseiko@): this is for applying PieceColor to other components than Piece.
    public static void ChangePieceColorAction(PieceColor pieceColor) {
      if (!pieceColor.TryGetComponent(out ZNetView netView)
          || !netView
          || !netView.IsValid()
          || !PrivateArea.CheckAccess(pieceColor.transform.position, flash: true)) {
        return;
      }

      netView.ClaimOwnership();
      ChangePieceColorZdo(netView);

      pieceColor.UpdateColors();

      Instantiate(
          ZNetScene.s_instance.GetPrefab("vfx_boar_love"),
          pieceColor.transform.position,
          pieceColor.transform.rotation);
    }

    static void ChangePieceColorZdo(ZNetView netView) {
      SetPieceColorZdoValues(
          netView.m_zdo, Utils.ColorToVec3(TargetPieceColor.Value), TargetPieceEmissionColorFactor.Value);
    }

    static readonly List<Piece> _piecesCache = new();

    public static IEnumerator ChangeColorsInRadiusCoroutine(Vector3 position, float radius) {
      yield return null;

      _piecesCache.Clear();
      GetAllPiecesInRadius(Player.m_localPlayer.transform.position, radius, _piecesCache);

      long changeColorCount = 0L;

      foreach (Piece piece in _piecesCache) {
        if (changeColorCount % 5 == 0) {
          yield return null;
        }

        if (piece && piece.TryGetComponent(out WearNTear wearNTear)) {
          ChangePieceColorAction(wearNTear);
          changeColorCount++;
        }
      }

      LogMessage($"Changed color of {changeColorCount} pieces.");
      _piecesCache.Clear();
    }

    public static void ClearPieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      SetPieceColorZdoValues(
          wearNTear.m_nview.m_zdo, PluginConstants.NoColorVector3, PluginConstants.NoEmissionColorFactor);

      if (wearNTear.TryGetComponent(out PieceColor pieceColor)) {
        pieceColor.UpdateColors();
      }

      wearNTear.m_piece.Ref()?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
    }

    public static IEnumerator ClearColorsInRadiusCoroutine(Vector3 position, float radius) {
      yield return null;

      _piecesCache.Clear();
      GetAllPiecesInRadius(Player.m_localPlayer.transform.position, radius, _piecesCache);

      long clearColorCount = 0L;

      foreach (Piece piece in _piecesCache) {
        if (clearColorCount % 5 == 0) {
          yield return null;
        }

        if (piece && piece.TryGetComponent(out WearNTear wearNTear)) {
          ClearPieceColorAction(wearNTear);
          clearColorCount++;
        }
      }

      LogMessage($"Cleared colors from {clearColorCount} pieces.");
    }

    public static bool CopyPieceColorAction(ZNetView netView) {
      if (!netView || !netView.IsValid() || !netView.m_zdo.TryGetVector3(PieceColorHashCode, out Vector3 colorAsVector)) {
        return false;
      }

      Color color = Utils.Vec3ToColor(colorAsVector);
      TargetPieceColor.SetValue(color);

      if (netView.m_zdo.TryGetFloat(PieceEmissionColorFactorHashCode, out float factor)) {
        TargetPieceEmissionColorFactor.Value = factor;
      }

      MessageHud.m_instance.Ref()?.ShowMessage(
          MessageHud.MessageType.TopLeft,
          $"Copied piece color: #{ColorUtility.ToHtmlStringRGB(color)} (f: {TargetPieceEmissionColorFactor.Value})");

      return true;
    }

    public static void SetPieceColorZdoValues(ZDO zdo, Vector3 colorVector3, float emissionColorFactor) {
      zdo.Set(PieceColorHashCode, colorVector3);
      zdo.Set(PieceEmissionColorFactorHashCode, emissionColorFactor);
      zdo.Set(PieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());
      zdo.Set(PieceLastColoredByHostHashCode, PrivilegeManager.GetNetworkUserId());
    }

    public static void GetAllPiecesInRadius(Vector3 position, float radius, List<Piece> pieces) {
      foreach (Piece piece in Piece.s_allPieces) {
        if (piece.gameObject.layer == Piece.s_ghostLayer
            || Vector3.Distance(position, piece.transform.position) >= radius) {
          continue;
        }

        pieces.Add(piece);
      }
    }

    public static void LogMessage(string message) {
      _logger.LogInfo(message);
      Chat.m_instance.Ref()?.AddString(message);
    }
  }
}
