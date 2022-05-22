using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Shortcuts : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.shortcuts";
    public const string PluginName = "Shortcuts";
    public const string PluginVersion = "1.2.0";

    Harmony _harmony;

    public void Awake() {
      CreateConfig(Config);

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly CodeMatch _inputGetKeyDownMatch =
        new(
            OpCodes.Call,
            AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new Type[] { typeof(KeyCode) }));

    static readonly CodeMatch _inputGetKeyMatch =
        new(OpCodes.Call, AccessTools.Method(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode) }));

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Hud.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11C), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => _toggleHudShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x132), _inputGetKeyMatch)
            .Advance(offset: 1)
            .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => true).operand)
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x7A)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleDebugFlyShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x62)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleDebugNoCostShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6B)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _debugKillAllShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6C)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _debugRemoveDropsShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x31)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem1Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x32)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem2Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x33)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem3Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x34)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem4Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x35)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem5Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x36)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem6Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x37)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem7Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x38)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => IsDownIgnored(_hotbarItem8Shortcut.Value)))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(Console))]
    class ConsolePatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Console.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11E), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleConsoleShortcut.Value.IsDown()).operand)
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(FejdStartup))]
    class FejdStartupPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(FejdStartup.LateUpdate))]
      static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x124), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _takeScreenshotShortcut.Value.IsDown()).operand)
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(GameCamera))]
    class GameCameraPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(GameCamera.LateUpdate))]
      static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x124), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _takeScreenshotShortcut.Value.IsDown()).operand)
            .InstructionEnumeration();
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(GameCamera.UpdateMouseCapture))]
      static IEnumerable<CodeInstruction> UpdateMouseCaptureTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x132), _inputGetKeyMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleMouseCaptureShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11A), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(OpCodes.Call, Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => true).operand)
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(ConnectPanel))]
    class ConnectPanelPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ConnectPanel.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11B), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleConnectPanelShortcut.Value.IsDown()).operand)
            .InstructionEnumeration();
      }
    }

    static readonly string[] InputKeyNames = {
      "Forward", "Backward", "Left", "Right", "Jump", "Crouch", "Run", "Hide", "Sit" };

    static readonly KeyCode[] IgnoredKeyCodes = {
      KeyCode.Mouse0,
      KeyCode.Mouse1,
      KeyCode.Mouse2,
      KeyCode.Mouse3,
      KeyCode.Mouse4,
      KeyCode.Mouse5,
      KeyCode.Mouse6,
      KeyCode.None
    };

    static readonly IEnumerable<KeyCode> AllKeyCodes = (KeyCode[]) Enum.GetValues(typeof(KeyCode));
    static readonly Dictionary<KeyboardShortcut, KeyCode[]> AllKeysCache = new();

    static KeyCode[] BlockingKeyCodes = { };
    static readonly HashSet<KeyCode> KeysCache = new();

    static bool IsDownIgnored(KeyboardShortcut shortcut) {
      if (!Input.GetKeyDown(shortcut.MainKey) || !shortcut.Modifiers.All(c => Input.GetKey(c))) {
        return false;
      }

      if (!AllKeysCache.TryGetValue(shortcut, out KeyCode[] keys)) {
        keys = new[] { shortcut.MainKey }.Concat(shortcut.Modifiers).ToArray();
        AllKeysCache[shortcut] = keys;
      }

      return BlockingKeyCodes.All(c => !Input.GetKey(c) || keys.Contains(c));
    }

    static void UpdateBlockingKeyCodes(ZInput zInputInstance) {
      KeyCode[] CombinedIgnoredKeyCodes =
          IgnoredKeyCodes.Concat(
                  InputKeyNames
                      .Select(name => zInputInstance.m_buttons[name])
                      .Where(def => def != null && def.m_key != KeyCode.None)
                      .Select(def => def.m_key))
              .ToArray();

      ZLog.Log($"Combined IgnoredKeyCodes are: {string.Join(", ", CombinedIgnoredKeyCodes)}");

      BlockingKeyCodes = AllKeyCodes.Except(CombinedIgnoredKeyCodes).ToArray();
    }

    [HarmonyPatch(typeof(ZInput))]
    static class ZInputPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZInput.Load))]
      static void LoadPostfix(ZInput __instance) {
        UpdateBlockingKeyCodes(__instance);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZInput.Reset))]
      static void ResetPostfix(ZInput __instance) {
        UpdateBlockingKeyCodes(__instance);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZInput.EndBindKey))]
      static void EndBindKeyPostfix(ref ZInput __instance) {
        UpdateBlockingKeyCodes(__instance);
      }
    }
  }
}