using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [HarmonyPatch(typeof(Console))]
  static class ConsolePatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Console.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ret),
              new CodeMatch(OpCodes.Ldc_I4, 0x11E),
              new CodeMatch(OpCodes.Call))
          .Advance(offset: 2)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleConsoleShortcut.Value.IsKeyDown()))
          .InstructionEnumeration();
    }
  }
}