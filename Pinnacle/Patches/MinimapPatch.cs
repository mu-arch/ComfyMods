using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using UnityEngine;

using static Pinnacle.PluginConfig;

namespace Pinnacle.Patches {
  [HarmonyPatch(typeof(Minimap))]
  public class MinimapPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Minimap.OnMapLeftClick))]
    static IEnumerable<CodeInstruction> OnMapLeftClickTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Minimap), nameof(Minimap.GetClosestPin))),
              new CodeMatch(OpCodes.Stloc_1))
          .Advance(offset: 1)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<Minimap.PinData, Minimap.PinData>>(GetClosestPinDelegate))
          .InstructionEnumeration();
    }

    static Minimap.PinData GetClosestPinDelegate(Minimap.PinData closestPin) {
      if (IsModEnabled.Value && Input.GetKey(KeyCode.RightShift)) {
        Pinnacle.TogglePinEditPanel(closestPin);
        return null;
      }

      return closestPin;
    }
  }
}
