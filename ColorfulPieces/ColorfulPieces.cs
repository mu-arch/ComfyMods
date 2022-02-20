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
    public const string PluginVersion = "1.6.0";

    static readonly int _pieceColorHashCode = "PieceColor".GetStableHashCode();
    static readonly int _pieceEmissionColorFactorHashCode = "PieceEmissionColorFactor".GetStableHashCode();
    static readonly int _pieceLastColoredByHashCode = "PieceLastColoredBy".GetStableHashCode();

    static readonly Dictionary<WearNTear, WearNTearData> _wearNTearDataCache = new();
    static readonly ConcurrentDictionary<Vector3, Color> _vectorToColorCache = new();

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      CreateConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Terminal))]
    class TerminalPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Terminal.InitTerminal))]
      static void InitTerminalPostfix() {
        new Terminal.ConsoleCommand(
            "clearcolor",
            "ColorfulPieces: Clears all colors applied to all pieces within radius of player.",
            args => {
              if (args.Length < 2 || !float.TryParse(args.Args[1], out float radius) || !Player.m_localPlayer) {
                return;
              }

              args.Context.StartCoroutine(
                  ClearColorsInRadiusCoroutine(Player.m_localPlayer.transform.position, radius));
            });

        new Terminal.ConsoleCommand(
            "changecolor",
            "ColorfulPieces: Changes the color of all pieces within radius of player to the currently set color.",
            args => {
              if (args.Length < 2 || !float.TryParse(args.Args[1], out float radius) || !Player.m_localPlayer) {
                return;
              }

              args.Context.StartCoroutine(
                  ChangeColorsInRadiusCoroutine(Player.m_localPlayer.transform.position, radius));
            });
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))),
                new CodeMatch(OpCodes.Stloc_0))
            .Advance(offset: 2)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0))
            .InstructionEnumeration();
      }

      static GameObject _hoverObject;

      static bool TakeInputDelegate(bool takeInputResult) {
        if (!_isModEnabled.Value) {
          return takeInputResult;
        }

        _hoverObject = Player.m_localPlayer?.m_hovering;

        if (!_hoverObject) {
          return takeInputResult;
        }

        if (_changePieceColorShortcut.Value.IsDown()) {
          Player.m_localPlayer.StartCoroutine(ChangePieceColorCoroutine(_hoverObject));
          return false;
        }

        if (_clearPieceColorShortcut.Value.IsDown()) {
          Player.m_localPlayer.StartCoroutine(ClearPieceColorCoroutine(_hoverObject));
          return false;
        }

        if (_copyPieceColorShortcut.Value.IsDown()) {
          Player.m_localPlayer.StartCoroutine(CopyPieceColorCoroutine(_hoverObject));
          return false;
        }

        return takeInputResult;
      }
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

    static IEnumerator ChangePieceColorCoroutine(GameObject target) {
      yield return null;
      ChangePieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void ChangePieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      wearNTear.m_nview.m_zdo.Set(_pieceColorHashCode, _targetPieceColorAsVec3);
      wearNTear.m_nview.m_zdo.Set(_pieceEmissionColorFactorHashCode, _targetPieceEmissionColorFactor.Value);
      wearNTear.m_nview.m_zdo.Set(_pieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());

      if (_wearNTearDataCache.TryGetValue(wearNTear, out WearNTearData wearNTearData)) {
        wearNTearData.TargetColor = _targetPieceColor.Value;
        wearNTearData.TargetEmissionColorFactor = _targetPieceEmissionColorFactor.Value;

        SetWearNTearColors(wearNTearData);
      }

      wearNTear.m_piece?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
    }

    static readonly List<Piece> _piecesCache = new();

    static IEnumerator ChangeColorsInRadiusCoroutine(Vector3 position, float radius) {
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

    static IEnumerator ClearPieceColorCoroutine(GameObject target) {
      yield return null;
      ClearPieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void ClearPieceColorAction(WearNTear wearNTear) {
      if (!ClaimOwnership(wearNTear)) {
        return;
      }

      if (wearNTear.m_nview.m_zdo.RemoveVec3(_pieceColorHashCode)
          || wearNTear.m_nview.m_zdo.RemoveFloat(_pieceEmissionColorFactorHashCode)) {
        wearNTear.m_nview.m_zdo.Set(_pieceLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());
        wearNTear.m_nview.m_zdo.IncreseDataRevision();
      }

      if (_wearNTearDataCache.TryGetValue(wearNTear, out WearNTearData wearNTearData)) {
        wearNTearData.TargetColor = Color.clear;
        wearNTearData.TargetEmissionColorFactor = 0f;
        wearNTearData.ClearMaterialColors();
      }

      wearNTear.m_piece?.m_placeEffect?.Create(wearNTear.transform.position, wearNTear.transform.rotation);
    }

    static IEnumerator ClearColorsInRadiusCoroutine(Vector3 position, float radius) {
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

    static IEnumerator CopyPieceColorCoroutine(GameObject target) {
      yield return null;
      CopyPieceColorAction(target?.GetComponentInParent<WearNTear>());
    }

    static void CopyPieceColorAction(WearNTear wearNTear) {
      if (!wearNTear?.m_nview
          || !wearNTear.m_nview.IsValid()
          || !wearNTear.m_nview.m_zdo.TryGetVec3(_pieceColorHashCode, out Vector3 colorAsVector)) {
        return;
      }

      _targetPieceColor.Value = _vectorToColorCache.GetOrAdd(colorAsVector, Utils.Vec3ToColor);
      _targetPieceColorHex.Value = $"#{ColorUtility.ToHtmlStringRGB(_targetPieceColor.Value)}";

      if (wearNTear.m_nview.m_zdo.TryGetFloat(_pieceEmissionColorFactorHashCode, out float factor)) {
        _targetPieceEmissionColorFactor.Value = factor;
      }

      MessageHud.instance?.ShowMessage(
          MessageHud.MessageType.TopLeft,
          $"Copied piece color: {_targetPieceColorHex.Value} (f: {_targetPieceEmissionColorFactor.Value})");
    }

    [HarmonyPatch(typeof(WearNTear))]
    class WearNTearPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(WearNTear.Awake))]
      static void WearNTearAwakePostfix(ref WearNTear __instance) {
        if (!_isModEnabled.Value || !__instance?.m_nview || !__instance.m_nview.IsValid()) {
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
        if (!_isModEnabled.Value
            || !__instance?.m_nview
            || __instance.m_nview.m_zdo == null
            || __instance.m_nview.m_zdo.m_zdoMan == null
            || __instance.m_nview.m_zdo.m_vec3 == null
            || !_wearNTearDataCache.TryGetValue(__instance, out WearNTearData wearNTearData)
            || wearNTearData.LastDataRevision >= __instance.m_nview.m_zdo.m_dataRevision) {
          return;
        }

        if (__instance.m_nview.m_zdo.m_vec3.TryGetValue(_pieceColorHashCode, out Vector3 colorAsVector)) {
          wearNTearData.TargetColor = _vectorToColorCache.GetOrAdd(colorAsVector, Utils.Vec3ToColor);

          if (__instance.m_nview.m_zdo.m_floats != null
              && __instance.m_nview.m_zdo.m_floats.TryGetValue(_pieceEmissionColorFactorHashCode, out float factor)) {
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
        if (_isModEnabled.Value) {
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

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      static readonly string _hoverNameTextTemplate =
        "{0}{1}"
            + "<size={9}>"
            + "[<color={2}>{3}</color>] Set piece color: <color=#{4}>#{4}</color> (<color=#{4}>{5}</color>)\n"
            + "[<color={6}>{7}</color>] Clear piece color\n"
            + "[<color={6}>{8}</color>] Copy piece color\n"
            + "</size>";

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.UpdateCrosshair))]
      static void HudUpdateCrosshairPostfix(ref Hud __instance, ref Player player) {
        if (!_isModEnabled.Value || !_showChangeRemoveColorPrompt.Value || !Player.m_localPlayer?.m_hovering) {
          return;
        }

        WearNTear wearNTear = player.m_hovering.GetComponentInParent<WearNTear>();

        if (!wearNTear?.m_nview || !wearNTear.m_nview.IsValid()) {
          return;
        }

        __instance.m_hoverName.text =
            string.Format(
                _hoverNameTextTemplate,
                __instance.m_hoverName.text,
                __instance.m_hoverName.text.Length > 0 ? "\n" : string.Empty,
                "#FFA726",
                _changePieceColorShortcut.Value,
                ColorUtility.ToHtmlStringRGB(_targetPieceColor.Value),
                _targetPieceEmissionColorFactor.Value.ToString("N2"),
                "#EF5350",
                _clearPieceColorShortcut.Value,
                _copyPieceColorShortcut.Value,
                _colorPromptFontSize.Value);
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
