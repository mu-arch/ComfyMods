using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using ComfyLib;

using HarmonyLib;

using static Insightful.Insightful;
using static Insightful.PluginConfig;

namespace Insightful {
  [HarmonyPatch(typeof(Player))]
  static class PlayerPatch {
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Player.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Player), nameof(Player.UpdateHover))))
          .Advance(offset: 1)
          .InsertAndAdvance(Transpilers.EmitDelegate<Action>(UpdateHoverPostDelegate))
          .InstructionEnumeration();
    }

    static void UpdateHoverPostDelegate() {
      if (IsModEnabled.Value
          && ReadHiddenTextShortcut.Value.IsKeyDown()
          && Player.m_localPlayer
          && Player.m_localPlayer.m_hovering) {
        Player.m_localPlayer.StartCoroutine(ReadHiddenTextCoroutine(Player.m_localPlayer.m_hovering));
      }
    }
  }
}
