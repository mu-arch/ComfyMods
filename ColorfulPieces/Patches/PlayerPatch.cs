using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using UnityEngine;

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
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))),
              new CodeMatch(OpCodes.Stloc_0))
          .Advance(offset: 2)
          .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
          .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0))
          .InstructionEnumeration();
    }

    static GameObject _hoverObject;

    static bool TakeInputDelegate(bool takeInputResult) {
      if (!IsModEnabled.Value) {
        return takeInputResult;
      }

      _hoverObject = Player.m_localPlayer.Ref()?.m_hovering;

      if (!_hoverObject) {
        return takeInputResult;
      }

      if (ChangePieceColorShortcut.Value.IsDown()) {
        Player.m_localPlayer.StartCoroutine(ChangePieceColorCoroutine(_hoverObject));
        return false;
      }

      if (ClearPieceColorShortcut.Value.IsDown()) {
        Player.m_localPlayer.StartCoroutine(ClearPieceColorCoroutine(_hoverObject));
        return false;
      }

      if (CopyPieceColorShortcut.Value.IsDown()) {
        Player.m_localPlayer.StartCoroutine(CopyPieceColorCoroutine(_hoverObject));
        return false;
      }

      return takeInputResult;
    }
  }
}
