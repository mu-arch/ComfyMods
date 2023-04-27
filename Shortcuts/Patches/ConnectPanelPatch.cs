using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [HarmonyPatch(typeof(ConnectPanel))]
  static class ConnectPanelPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ConnectPanel.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x11B),
              new CodeMatch(OpCodes.Call))
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleConnectPanelShortcut.Value.IsKeyDown()))
          .InstructionEnumeration();
    }
  }
}