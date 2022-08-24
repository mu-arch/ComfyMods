using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using static Recipedia.PluginConfig;

namespace Recipedia {
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
      if (IsModEnabled.Value && RecipeListPanelToggleShortcut.Value.IsDown()) {
        Recipedia.ToggleRecipeListPanel();
        return false;
      }

      return takeInputResult;
    }
  }
}
