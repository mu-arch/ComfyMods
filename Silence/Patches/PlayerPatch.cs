using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static Silence.PluginConfig;
using static Silence.Silence;

namespace Silence {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))))
          .Advance(offset: 1)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
          .InstructionEnumeration();
    }

    static bool TakeInputDelegate(bool takeInput) {
      if (takeInput && IsModEnabled.Value && ToggleSilenceShortcut.Value.IsDown()) {
        Player.m_localPlayer.StartCoroutine(ToggleSilenceCoroutine());
        return false;
      }

      return takeInput;
    }
  }
}
