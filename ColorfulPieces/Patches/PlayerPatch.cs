using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static ColorfulPieces.ColorfulPieces;
using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces {
  [HarmonyPatch(typeof(Player))]
  class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
          .Advance(offset: 2)
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldloc_1),
              Transpilers.EmitDelegate<Action<bool>>(UpdateHoverPostDelegate))
          .InstructionEnumeration();
    }

    static void UpdateHoverPostDelegate(bool takeInput) {
      if (takeInput && IsModEnabled.Value && Player.m_localPlayer && Player.m_localPlayer.m_hovering) {
        if (ChangePieceColorShortcut.Value.IsDown()
            && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear changeTarget)) {
          ChangePieceColorAction(changeTarget);
        }

        if (ClearPieceColorShortcut.Value.IsDown()
            && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear clearTarget)) {
          ClearPieceColorAction(clearTarget);
        }

        if (CopyPieceColorShortcut.Value.IsDown()
            && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear copyTarget)
            && copyTarget) {
          CopyPieceColorAction(copyTarget.m_nview);
        }
      }
    }
  }
}
