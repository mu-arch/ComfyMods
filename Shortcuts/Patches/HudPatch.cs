using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Hud.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x11C),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleHudShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x132),
              Shortcuts.InputGetKeyMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => true))
          .InstructionEnumeration();
    }
  }
}