using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace Shortcuts {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Shortcuts : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.shortcuts";
    public const string PluginName = "Shortcuts";
    public const string PluginVersion = "0.9.0";

    static ConfigEntry<bool> _isModEnabled;

    static ConfigEntry<KeyboardShortcut> _toggleHudShortcut;
    static ConfigEntry<KeyboardShortcut> _toggleDebugFlyShortcut;
    static ConfigEntry<KeyboardShortcut> _toggleDebugNoCostShortcut;
    static ConfigEntry<KeyboardShortcut> _toggleDebugKillAllShortcut;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      _toggleHudShortcut =
          Config.Bind(
              "Hud",
              "toggleHudShortcut",
              new KeyboardShortcut(KeyCode.F3, KeyCode.LeftControl),
              "Shortcut to toggle the Hud on/off.");

      _toggleDebugFlyShortcut =
          Config.Bind(
              "Debugmode",
              "toggleDebugFlyShortcut",
              new KeyboardShortcut(KeyCode.Z),
              "Shortcut to toggle flying when in debugmode.");

      _toggleDebugNoCostShortcut =
          Config.Bind(
              "Debugmode",
              "toggleDebugNoCostShortcut",
              new KeyboardShortcut(KeyCode.B),
              "Shortcut to toggle no-cost building when in debugmode.");

      _toggleDebugKillAllShortcut =
          Config.Bind(
              "Debugmode",
              "toggleDebugKillAllShortcut",
              new KeyboardShortcut(KeyCode.None),
              "Shortcut to kill/damage all mobs around player. Unbound by default.");

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

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      static readonly CodeMatch _inputGetKeyMatch =
          new(OpCodes.Call, AccessTools.Method(typeof(Input), nameof(Input.GetKey), new Type[] { typeof(KeyCode) }));

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
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(122)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleDebugFlyShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(98)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleDebugNoCostShortcut.Value.IsDown()).operand)
            .MatchForward(useEnd: false, new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(107)), _inputGetKeyDownMatch)
            .Advance(offset: 1)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _toggleDebugKillAllShortcut.Value.IsDown()).operand)
            .InstructionEnumeration();
      }
    }
  }
}