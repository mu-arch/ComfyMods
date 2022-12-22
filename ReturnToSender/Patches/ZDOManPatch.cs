using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

namespace ReturnToSender {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZDOMan), nameof(ZDOMan.SendZDOToPeers2))))
          .SetOperandAndAdvance(AccessTools.Method(typeof(ZDOMan), nameof(ZDOMan.SendZDOToPeers)))
          .InstructionEnumeration();
    }
  }
}
