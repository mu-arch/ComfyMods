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
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
          .InstructionEnumeration();
    }

    static bool TakeInputDelegate(bool takeInputResult) {
      if (IsModEnabled.Value && Player.m_localPlayer && Player.m_localPlayer.m_hovering) {
        if (ChangePieceColorShortcut.Value.IsDown()) {
          if (Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear changeTarget)) {
            Player.m_localPlayer.StartCoroutine(ChangePieceColorCoroutine(changeTarget));
            return false;
          }
        }

        if (ClearPieceColorShortcut.Value.IsDown()
            && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear clearTarget)) {
          Player.m_localPlayer.StartCoroutine(ClearPieceColorCoroutine(clearTarget));
          return false;
        }

        if (CopyPieceColorShortcut.Value.IsDown()
            && Player.m_localPlayer.m_hovering.TryGetComponentInParent(out WearNTear copyTarget)) {
          Player.m_localPlayer.StartCoroutine(CopyPieceColorCoroutine(copyTarget));
          return false;
        }
      }

      return takeInputResult;
    }
  }
}
