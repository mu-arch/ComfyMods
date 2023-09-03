using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using ComfyLib;

using HarmonyLib;

using static ColorfulWards.PluginConfig;

namespace ColorfulWards.Patches {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyEmitIL] // TODO REMOVE ME
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
          .Advance(offset: 1)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldloc_1),
              Transpilers.EmitDelegate<Func<bool, bool>>(UpdateHoverPostDelegate),
              new CodeInstruction(OpCodes.Stloc_1))
          .InstructionEnumeration();
    }

    static bool UpdateHoverPostDelegate(bool takeInput) {
      if (takeInput
          && IsModEnabled.Value
          && ChangeWardColorShortcut.Value.IsDown()
          && Player.m_localPlayer
          && Player.m_localPlayer.m_hovering
          && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out PrivateArea privateArea)) {
        ColorfulWards.ChangeWardColor(privateArea);
        return false;
      }

      return takeInput;
    }
  }
}
