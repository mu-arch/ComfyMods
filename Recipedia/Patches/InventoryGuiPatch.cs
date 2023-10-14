using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [HarmonyPatch(typeof(InventoryGui))]
  static class InventoryGuiPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.SetupCrafting))]
    static void SetupCraftingPostfix(InventoryGui __instance) {
      if (IsModEnabled.Value) {
        RecipeFilterController.SetupRecipeFilter(__instance);
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(InventoryGui.UpdateContainer))]
    static IEnumerable<CodeInstruction> UpdateContainerTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldstr, "Use"),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetButton))),
              new CodeMatch(OpCodes.Brtrue))
          .ThrowIfInvalid("Could not find GetButton(\"Use\").")
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(GetButtonDelegate))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldstr, "JoyUse"),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetButton))),
              new CodeMatch(OpCodes.Brfalse))
          .ThrowIfInvalid("Could not find GetButton(\"JoyUse\").")
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(GetButtonDelegate))
          .InstructionEnumeration();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(InventoryGui.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldstr, "Use"),
              new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(ZInput), nameof(ZInput.GetButtonDown))),
              new CodeMatch(OpCodes.Brfalse))
          .ThrowIfInvalid("Could not find GetButtonDown(\"Use\").")
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(GetButtonDelegate))
          .InstructionEnumeration();
    }


    static bool GetButtonDelegate(bool result) {
      if (result && RecipeFilterController.IsFocused()) {
        return false;
      }

      return result;
    }
  }
}
