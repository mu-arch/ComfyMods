using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulPieces : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfulpieces";
    public const string PluginName = "ColorfulPieces";
    public const string PluginVersion = "1.7.0";

    public static readonly int PieceColorHashCode = "PieceColor".GetStableHashCode();
    public static readonly int PieceEmissionColorFactorHashCode = "PieceEmissionColorFactor".GetStableHashCode();
    public static readonly int PieceLastColoredByHashCode = "PieceLastColoredBy".GetStableHashCode();

    static readonly Dictionary<WearNTear, WearNTearData> _wearNTearDataCache = new();
    static readonly ConcurrentDictionary<Vector3, Color> _vectorToColorCache = new();

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
      if (!wearNTear?.m_nview
          || !wearNTear.m_nview.IsValid()
          || !PrivateArea.CheckAccess(wearNTear.transform.position, flash: true)) {
        _logger.LogWarning("Piece does not have a valid ZNetView or is in a PrivateArea.");
        return false;
      }

      if (!wearNTear.m_nview.IsOwner()) {
        wearNTear.m_nview.ClaimOwnership();
      }

      return true;
    }

    public static IEnumerator ChangePieceColorCoroutine(GameObject target) {
      yield return null;
      ChangePieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void ChangePieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      wearNTear.m_nview.m_zdo.Set(PieceColorHashCode, TargetPieceColorAsVec3);
      wearNTear.m_nview.m_zdo.Set(PieceEmissionColorFactorHashCode, TargetPieceEmissionColorFactor.Value);
      wearNTear.m_nview.m_zdo.Set(PieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());

      if (_wearNTearDataCache.TryGetValue(wearNTear, out WearNTearData wearNTearData)) {
        wearNTearData.TargetColor = TargetPieceColor.Value;
        wearNTearData.TargetEmissionColorFactor = TargetPieceEmissionColorFactor.Value;

        SetWearNTearColors(wearNTearData);
      }

      wearNTear.m_piece?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
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

    public static IEnumerator ClearPieceColorCoroutine(GameObject target) {
      yield return null;
      ClearPieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void ClearPieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      if (wearNTear.m_nview.m_zdo.RemoveVec3(PieceColorHashCode)
          || wearNTear.m_nview.m_zdo.RemoveFloat(PieceEmissionColorFactorHashCode)) {
        wearNTear.m_nview.m_zdo.Set(PieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());
        wearNTear.m_nview.m_zdo.IncreseDataRevision();
      }

      if (_wearNTearDataCache.TryGetValue(wearNTear, out WearNTearData wearNTearData)) {
        wearNTearData.TargetColor = Color.clear;
        wearNTearData.TargetEmissionColorFactor = 0f;
        wearNTearData.ClearMaterialColors();
      }

      wearNTear.m_piece?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
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

    public static IEnumerator CopyPieceColorCoroutine(GameObject target) {
      yield return null;
      CopyPieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void CopyPieceColorAction(WearNTear wearNTear) {
      if (!wearNTear?.m_nview
          || !wearNTear.m_nview.IsValid()
          || !wearNTear.m_nview.m_zdo.TryGetVec3(PieceColorHashCode, out Vector3 colorAsVector)) {
        return;
      }

      TargetPieceColor.Value = _vectorToColorCache.GetOrAdd(colorAsVector, Utils.Vec3ToColor);
      TargetPieceColorHex.Value = $"#{ColorUtility.ToHtmlStringRGB(TargetPieceColor.Value)}";

      if (wearNTear.m_nview.m_zdo.TryGetFloat(PieceEmissionColorFactorHashCode, out float factor)) {
        TargetPieceEmissionColorFactor.Value = factor;
      }

      MessageHud.instance?.ShowMessage(
          MessageHud.MessageType.TopLeft,
          $"Copied piece color: {TargetPieceColorHex.Value} (f: {TargetPieceEmissionColorFactor.Value})");
    }

    [HarmonyPatch(typeof(WearNTear))]
    class WearNTearPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(WearNTear.Awake))]
      static void WearNTearAwakePostfix(ref WearNTear __instance) {
        if (!IsModEnabled.Value || !__instance?.m_nview || !__instance.m_nview.IsValid()) {
          return;
        }

        if (!_wearNTearDataCache.TryGetValue(__instance, out WearNTearData wearNTearData)) {
          wearNTearData = new(__instance);
          _wearNTearDataCache[__instance] = wearNTearData;
        }

        wearNTearData.ClearMaterialColors();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(WearNTear.OnDestroy))]
      static void WearNTearOnDestroyPrefix(ref WearNTear __instance) {
        _wearNTearDataCache.Remove(__instance);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(WearNTear.UpdateWear))]
      static void WearNTearUpdateWearPostfix(ref WearNTear __instance) {
        if (!IsModEnabled.Value
            || !__instance?.m_nview
            || __instance.m_nview.m_zdo == null
            || __instance.m_nview.m_zdo.m_zdoMan == null
            || __instance.m_nview.m_zdo.m_vec3 == null
            || !_wearNTearDataCache.TryGetValue(__instance, out WearNTearData wearNTearData)
            || wearNTearData.LastDataRevision >= __instance.m_nview.m_zdo.m_dataRevision) {
          return;
        }

        if (__instance.m_nview.m_zdo.m_vec3.TryGetValue(PieceColorHashCode, out Vector3 colorAsVector)) {
          wearNTearData.TargetColor = _vectorToColorCache.GetOrAdd(colorAsVector, Utils.Vec3ToColor);

          if (__instance.m_nview.m_zdo.m_floats != null
              && __instance.m_nview.m_zdo.m_floats.TryGetValue(PieceEmissionColorFactorHashCode, out float factor)) {
            wearNTearData.TargetEmissionColorFactor = factor;
          }

          SetWearNTearColors(wearNTearData);
        } else if (wearNTearData.TargetColor != Color.clear) {
          wearNTearData.TargetColor = Color.clear;
          wearNTearData.TargetEmissionColorFactor = 0f;
          wearNTearData.ClearMaterialColors();
        }

        wearNTearData.LastDataRevision = __instance.m_nview.m_zdo.m_dataRevision;
      }
    }

    [HarmonyPatch(typeof(WearNTearUpdater))]
    class WearNTearUpdaterPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(WearNTearUpdater.Awake))]
      static void AwakePostfix(ref WearNTearUpdater __instance) {
        if (IsModEnabled.Value) {
          __instance.StartCoroutine(LogCacheInfoCoroutine());
        }
      }

      static IEnumerator LogCacheInfoCoroutine() {
        WaitForSeconds waitInterval = new(seconds: 60f);

        while (true) {
          yield return waitInterval;
          _logger.LogInfo($"WearNTearData cache size: {_wearNTearDataCache.Count}");
        }
      }
    }



    static void SetWearNTearColors(WearNTearData wearNTearData) {
      foreach (Material material in wearNTearData.Materials) {
        if (material.HasProperty("_EmissionColor")) {
          material.SetColor("_EmissionColor", wearNTearData.TargetColor * wearNTearData.TargetEmissionColorFactor);
        }

        material.color = wearNTearData.TargetColor;
      }
    }
  }
}
