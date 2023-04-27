using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Shortcuts.PluginConfig;

namespace Shortcuts {
  [HarmonyPatch(typeof(GameCamera))]
  static class GameCameraPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(GameCamera.LateUpdate))]
    static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x124),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => TakeScreenshotShortcut.Value.IsKeyDown()))
          .InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(GameCamera.UpdateMouseCapture))]
    static IEnumerable<CodeInstruction> UpdateMouseCaptureTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x132),
              Shortcuts.InputGetKeyMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => ToggleMouseCaptureShortcut.Value.IsKeyDown()))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4, 0x11A),
              Shortcuts.InputGetKeyDownMatch)
          .Advance(offset: 1)
          .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<KeyCode, bool>>(_ => true))
          .InstructionEnumeration();
    }
  }
}