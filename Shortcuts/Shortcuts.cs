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
      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly CodeMatch InputGetKeyDownMatch =
        new(
            OpCodes.Call,
            AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new Type[] { typeof(KeyCode) }));

    static readonly CodeMatch InputGetKeyMatch =
        new(OpCodes.Call, AccessTools.Method(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode) }));

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Hud.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11C), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleHudShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x132), InputGetKeyMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => true))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x7A)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => ToggleDebugFlyShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x62)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleDebugNoCostShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6B)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => DebugKillAllShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6C)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => DebugRemoveDropsShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x31)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem1Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x32)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem2Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x33)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem3Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x34)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem4Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x35)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem5Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x36)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem6Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x37)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem7Shortcut.Value)))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x38)), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => IsKeyDown(HotbarItem8Shortcut.Value)))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(Console))]
    class ConsolePatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Console.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11E), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleConsoleShortcut.Value.IsDown()))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(FejdStartup))]
    class FejdStartupPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(FejdStartup.LateUpdate))]
      static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x124), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => TakeScreenshotShortcut.Value.IsDown()))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(GameCamera))]
    class GameCameraPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(GameCamera.LateUpdate))]
      static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x124), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => TakeScreenshotShortcut.Value.IsDown()))
            .InstructionEnumeration();
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(GameCamera.UpdateMouseCapture))]
      static IEnumerable<CodeInstruction> UpdateMouseCaptureTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x132), InputGetKeyMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleMouseCaptureShortcut.Value.IsDown()))
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11A), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => true))
            .InstructionEnumeration();
      }
    }

    [HarmonyPatch(typeof(ConnectPanel))]
    class ConnectPanelPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ConnectPanel.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4, 0x11B), InputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetInstructionAndAdvance(
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleConnectPanelShortcut.Value.IsDown()))
            .InstructionEnumeration();
      }
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

    /**
     * BepInEx.KeyboardShortcut documentation states "If any other keys are pressed, the shortcut will not trigger."
     * However we want the Hotbar shortcuts to be able to trigger during player movement and so need to add special
     * logic to exclude movement KeyCodes from the `IsDown()` validation.
     *
     * BepInEx hardcodes this in a private `KeyboardShortcut._modifierBlockKeyCodes` array so we need to mimic the
     * `IsDown()` implementation and support a configurable set of KeyCodes.
     *
     * Modified from: https://github.com/BepInEx/BepInEx/blob/master/BepInEx.Unity/Configuration/KeyboardShortcut.cs/
     */

    static readonly string[] ZInputKeyNamesToIgnore = {
      "Forward", "Backward", "Left", "Right", "Jump", "Crouch", "Run", "Hide", "Sit" };

    static readonly KeyCode[] IgnoredKeyCodes = {
      KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6,
      KeyCode.None
    };

    static readonly IEnumerable<KeyCode> AllKeyCodes = (KeyCode[]) Enum.GetValues(typeof(KeyCode));
    static KeyCode[] BlockingKeyCodes = AllKeyCodes.Except(IgnoredKeyCodes).ToArray();

    static bool IsKeyDown(KeyboardShortcut shortcut) {
      if (ShortcutKeysCache.TryGetValue(shortcut, out KeyCode[] keys)) {
        return Input.GetKeyDown(shortcut.MainKey)
            && shortcut.Modifiers.All(Input.GetKey)
            && BlockingKeyCodes.All(c => !Input.GetKey(c) || keys.Contains(c));
      }

      return shortcut.IsDown();
    }

    static void UpdateBlockingKeyCodes(ZInput zInputInstance) {
      BlockingKeyCodes =
          AllKeyCodes.Except(
                  IgnoredKeyCodes.Concat(
                      ZInputKeyNamesToIgnore
                          .Select(name => zInputInstance.m_buttons[name])
                          .Where(def => def != null && def.m_key != KeyCode.None)
                          .Select(def => def.m_key)))
              .ToArray();
    }
  }
}