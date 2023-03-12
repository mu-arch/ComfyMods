using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x7A)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(keyCode => ToggleDebugFlyShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x62)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleDebugNoCostShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6B)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => DebugKillAllShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x6C)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => DebugRemoveDropsShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x31)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem1Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x32)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem2Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x33)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem3Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x34)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem4Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x35)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem5Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x36)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem6Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x37)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem7Shortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_S, Convert.ToSByte(0x38)),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => HotbarItem8Shortcut.Value.IsKeyDown()))
          .InstructionEnumeration();
    }
  }
}