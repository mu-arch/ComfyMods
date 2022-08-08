using HarmonyLib;

using System.Collections.Generic;
using System.Reflection.Emit;
using System;

using static Insightful.Insightful;
using static Insightful.PluginConfig;

namespace Insightful.Patches {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
          .InstructionEnumeration();
    }

    static bool TakeInputDelegate(bool takeInputResult) {
      if (!IsModEnabled.Value
          || !ReadHiddenTextShortcut.Value.IsDown()
          || !Player.m_localPlayer
          || !Player.m_localPlayer.m_hovering) {
        return takeInputResult;
      }

      Player.m_localPlayer.StartCoroutine(ReadHiddenTextCoroutine(Player.m_localPlayer.m_hovering));
      return false;
    }
  }
}
