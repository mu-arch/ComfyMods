using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static ColorfulLights.ColorfulLights;
using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))),
              new CodeMatch(OpCodes.Stloc_0))
          .Advance(offset: 2)
          .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
          .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0))
          .InstructionEnumeration();
    }

    static bool TakeInputDelegate(bool takeInputResult) {
      if (IsModEnabled.Value
          && ChangeColorActionShortcut.Value.IsDown()
          && Player.m_localPlayer
          && Player.m_localPlayer.m_hovering) {
        Fireplace targetFireplace = Player.m_localPlayer.m_hovering.GetComponentInParent<Fireplace>();

        if (targetFireplace) {
          Player.m_localPlayer.StartCoroutine(ChangeFireplaceColorCoroutine(targetFireplace));
          return false;
        }
      }

      return takeInputResult;
    }
  }
}
