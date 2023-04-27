using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static Effectual.PluginConfig;

namespace Effectual {
  [HarmonyPatch(typeof(EffectArea))]
  static class EffectAreaPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EffectArea.IsPointInsideArea))]
    static IEnumerable<CodeInstruction> IsPointInsideAreaTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldsfld),
              new CodeMatch(OpCodes.Call),
              new CodeMatch(OpCodes.Stloc_0))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<int, int>>(OverlapSphereDelegate))
          .InstructionEnumeration();
    }

    static int OverlapSphereDelegate(int count) {
      if (IsModEnabled.Value && count == EffectArea.m_tempColliders.Length) {
        ZLog.Log($"EffectArea.m_tempColliders buffer full at size {count}, increasing by 128.");
        Array.Resize(ref EffectArea.m_tempColliders, count + 128);
      }

      return count;
    }
  }
}
