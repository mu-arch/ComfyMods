using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using static Silence.Silence;

namespace Silence {
  [HarmonyPatch(typeof(Chat))]
  static class ChatPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Chat.Awake))]
    static void AwakePostfix(Chat __instance) {
      ChatInstance = __instance;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Chat.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(
                  OpCodes.Call,
                  AccessTools.Method(typeof(ZInput), nameof(ZInput.GetKeyDown), new Type[] { typeof(KeyCode) })))
          .Advance(offset: 1)
          .InsertAndAdvance(
              new CodeInstruction(Transpilers.EmitDelegate<Func<bool, bool>>(result => result && !IsSilenced)))
          .InstructionEnumeration();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Chat.AddInworldText))]
    static bool AddInworldTextPrefix() {
      return !IsSilenced;
    }
  }
}
