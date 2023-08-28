using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static SearsCatalog.PluginConfig;

namespace SearsCatalog {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.UpdateBuildGuiInput))]
    static IEnumerable<CodeInstruction> UpdateBuildGuiInputTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldstr, "Mouse ScrollWheel"),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Input), nameof(Input.GetAxis))),
              new CodeMatch(OpCodes.Ldc_R4))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, float>>(GetAxisDelegate))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldstr, "Mouse ScrollWheel"),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetAxis))),
              new CodeMatch(OpCodes.Ldc_R4))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, float>>(GetAxisDelegate))
          .InstructionEnumeration();
    }

    static float GetAxisDelegate(float result) {
      if (result != 0f && IsModEnabled.Value) {
        return 0f;
      }

      return result;
    }
  }
}