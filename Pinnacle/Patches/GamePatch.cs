using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Game.UpdateNoMap))]
    static IEnumerable<CodeInstruction> UpdateNoMapTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldc_I4_1),
              new CodeMatch(OpCodes.Br),
              new CodeMatch(OpCodes.Ldc_I4_0),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Minimap), nameof(Minimap.SetMapMode))))
          .ThrowIfInvalid("Could not patch Game.UpdateNoMap call to SetMapMode.")
          .Advance(offset: 1)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<Minimap.MapMode, Minimap.MapMode>>(SetMapModeDelegate))
          .InstructionEnumeration();
    }

    static Minimap.MapMode SetMapModeDelegate(Minimap.MapMode mapMode) {
      if (IsModEnabled.Value && !Game.m_noMap && Minimap.m_instance.m_mode == Minimap.MapMode.Large) {
        mapMode = Minimap.MapMode.Large;
      }

      return mapMode;
    }
  }
}
