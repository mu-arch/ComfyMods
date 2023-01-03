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
    public const string PluginVersion = "1.9.0";

    public static readonly int PieceColorHashCode = "PieceColor".GetStableHashCode();
    public static readonly int PieceEmissionColorFactorHashCode = "PieceEmissionColorFactor".GetStableHashCode();
    public static readonly int PieceLastColoredByHashCode = "PieceLastColoredBy".GetStableHashCode();
    public static readonly int PieceLastColoredByHostHashCode = "PieceLastColoredByHost".GetStableHashCode();

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
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

    public static IEnumerator ChangePieceColorCoroutine(WearNTear target) {
      yield return null;
      ChangePieceColorAction(target);
    }

    static void ChangePieceColorAction(WearNTear wearNTear) {
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
          ZNetScene.m_instance.GetPrefab("vfx_boar_love"),
          pieceColor.transform.position,
          pieceColor.transform.rotation);
    }

    static void ChangePieceColorZdo(ZNetView netView) {
      netView.m_zdo.Set(PieceColorHashCode, TargetPieceColorAsVec3);
      netView.m_zdo.Set(PieceEmissionColorFactorHashCode, TargetPieceEmissionColorFactor.Value);
      netView.m_zdo.Set(PieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());
      netView.m_zdo.Set(PieceLastColoredByHostHashCode, PrivilegeManager.GetNetworkUserId());
    }

    static readonly List<Piece> _piecesCache = new();

    public static IEnumerator ChangeColorsInRadiusCoroutine(Vector3 position, float radius) {
      yield return null;

      _piecesCache.Clear();
      Piece.GetAllPiecesInRadius(Player.m_localPlayer.transform.position, radius, _piecesCache);

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

      _logger.LogInfo($"Changed color of {changeColorCount} pieces.");
      _piecesCache.Clear();
    }

    public static IEnumerator ClearPieceColorCoroutine(WearNTear target) {
      yield return null;
      ClearPieceColorAction(target);
    }

    static void ClearPieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      if (wearNTear.m_nview.m_zdo.RemoveVec3(PieceColorHashCode)
          || wearNTear.m_nview.m_zdo.RemoveFloat(PieceEmissionColorFactorHashCode)) {
        wearNTear.m_nview.m_zdo.Set(PieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());
        wearNTear.m_nview.m_zdo.Set(PieceLastColoredByHostHashCode, PrivilegeManager.GetNetworkUserId());
        wearNTear.m_nview.m_zdo.IncreseDataRevision();
      }

      if (wearNTear.TryGetComponent(out PieceColor pieceColor)) {
        pieceColor.UpdateColors();
      }

      wearNTear.m_piece.Ref()?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
    }

    public static IEnumerator ClearColorsInRadiusCoroutine(Vector3 position, float radius) {
      yield return null;

      _piecesCache.Clear();
      Piece.GetAllPiecesInRadius(Player.m_localPlayer.transform.position, radius, _piecesCache);

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

      _logger.LogInfo($"Cleared colors from {clearColorCount} pieces.");
    }

    public static IEnumerator CopyPieceColorCoroutine(WearNTear target) {
      yield return null;
      CopyPieceColorAction(target);
    }

    static void CopyPieceColorAction(WearNTear wearNTear) {
      if (!wearNTear
          || !wearNTear.m_nview
          || !wearNTear.m_nview.IsValid()
          || !wearNTear.m_nview.m_zdo.TryGetVec3(PieceColorHashCode, out Vector3 colorAsVector)) {
        return;
      }

      Color color = Utils.Vec3ToColor(colorAsVector);
      TargetPieceColor.SetValue(color);

      if (wearNTear.m_nview.m_zdo.TryGetFloat(PieceEmissionColorFactorHashCode, out float factor)) {
        TargetPieceEmissionColorFactor.Value = factor;
      }

      MessageHud.m_instance.ShowMessage(
          MessageHud.MessageType.TopLeft,
          $"Copied piece color: #{ColorUtility.ToHtmlStringRGB(color)} (f: {TargetPieceEmissionColorFactor.Value})");
    }
  }
}
