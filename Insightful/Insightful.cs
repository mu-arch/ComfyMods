using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace Insightful {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Insightful : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.insightful";
    public const string PluginName = "Insightful";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;
    static ConfigEntry<KeyboardShortcut> _readHiddenTextShortcut;

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _readHiddenTextShortcut =
          Config.Bind(
              "Hotkeys",
              "readHiddenTextShortcut",
              new KeyboardShortcut(KeyCode.R, KeyCode.LeftShift),
              "Shortcut to read hidden text embedded within RuneStones.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
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

      static bool TakeInputDelegate(bool takeInputResult) {
        if (!_isModEnabled.Value || !_readHiddenTextShortcut.Value.IsDown()) {
          return takeInputResult;
        }

        RuneStone runeStone = Player.m_localPlayer?.m_hovering?.GetComponentInParent<RuneStone>();

        if (!runeStone) {
          return takeInputResult;
        }

        runeStone.StartCoroutine(ReadHiddenTextCoroutine(runeStone));
        return false;
      }
    }

    static IEnumerator ReadHiddenTextCoroutine(RuneStone runeStone) {
      yield return null;

      ZDO zdo = runeStone?.GetComponentInParent<ZNetView>()?.GetZDO();

      if (zdo == null) {
        _logger.LogInfo("No ZNetView/ZDO found for RuneStone.");
        yield break;
      }

      if (!zdo.TryGetString(_inscriptionTopicHashCode, out string inscriptionTopic)
          || !zdo.TryGetString(_inscriptionTextHashCode, out string inscriptionText)) {
        _logger.LogInfo("RuneStone does not have custom Inscription Topic or Text.");
        yield break;
      }

      _logger.LogInfo($"Found hidden Inscription on RuneStone: {zdo.m_uid}");

      if (!zdo.TryGetEnum(_inscriptionStyleHashCode, out TextViewer.Style style)) {
        style = TextViewer.Style.Rune;
      }

      TextViewer.instance.ShowText(style, inscriptionTopic, inscriptionText, autoHide: true);
    }

    static readonly int _inscriptionTopicHashCode = "InscriptionTopic".GetStableHashCode();
    static readonly int _inscriptionTextHashCode = "InscriptionText".GetStableHashCode();
    static readonly int _inscriptionStyleHashCode = "InscriptionStyle".GetStableHashCode();

    [HarmonyPatch(typeof(RuneStone))]
    class RuneStonePatch {
      static readonly string _shortcutTemplate =
          "{0}\n[<color=yellow><b>{1}</b></color>] Read Inscription";

      [HarmonyPostfix]
      [HarmonyPatch(nameof(RuneStone.GetHoverText))]
      static void GetHoverTextPostfix(ref RuneStone __instance, ref string __result) {
        if (!_isModEnabled.Value) {
          return;
        }

        ZDO zdo = __instance?.GetComponentInParent<ZNetView>()?.GetZDO();

        if (zdo?.m_strings != null
            && zdo.m_strings.ContainsKey(_inscriptionTopicHashCode)
            && zdo.m_strings.ContainsKey(_inscriptionTextHashCode)) {
          __result = string.Format(_shortcutTemplate, __result, _readHiddenTextShortcut.Value);
        }
      }
    }
  }

  static class Extensions {
    public static bool TryGetString(this ZDO zdo, int keyHashCode, out string result) {
      if (zdo.m_strings == null) {
        result = default;
        return false;
      }

      return zdo.m_strings.TryGetValue(keyHashCode, out result);
    }

    public static bool TryGetEnum<T>(this ZDO zdo, int keyHashCode, out T result) {
      if (zdo.m_ints != null && zdo.m_ints.TryGetValue(keyHashCode, out int value)) {
        result = (T) Enum.ToObject(typeof(T), value);
        return true;
      }

      result = default;
      return false;
    }

    public static RuneStone.RandomRuneText Clear(this RuneStone.RandomRuneText runeText) {
      runeText.m_label = string.Empty;
      runeText.m_topic = string.Empty;
      runeText.m_text = string.Empty;

      return runeText;
    }
  }
}