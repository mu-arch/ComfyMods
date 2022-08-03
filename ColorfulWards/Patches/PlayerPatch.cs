using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using static ColorfulWards.PluginConfig;

namespace ColorfulWards.Patches {
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
      if (IsModEnabled.Value
          && ChangeWardColorShortcut.Value.IsDown()
          && Player.m_localPlayer
          && Player.m_localPlayer.m_hovering) {
        Player.m_localPlayer.StartCoroutine(
            ColorfulWards.ChangeWardColorCoroutine(
                Player.m_localPlayer.m_hovering.GetComponentInParent<PrivateArea>()));

        return false;
      }

      return takeInputResult;
    }
  }
}
