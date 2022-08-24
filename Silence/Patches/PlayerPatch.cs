using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using static Silence.PluginConfig;
using static Silence.Silence;

namespace Silence.Patches {
  [HarmonyPatch(typeof(Player))]
  public class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))))
          .Advance(offset: 2)
          .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, Character, bool>>(TakeInputDelegate))
          .InstructionEnumeration();
    }

    static bool TakeInputDelegate(bool takeInputResult, Character character) {
      if (!IsModEnabled.Value || character != Player.m_localPlayer) {
        return takeInputResult;
      }

      if (ToggleSilenceShortcut.Value.IsDown()) {
        Player.m_localPlayer.StartCoroutine(ToggleSilenceCoroutine());
        return false;
      }

      return takeInputResult;
    }
  }
}
