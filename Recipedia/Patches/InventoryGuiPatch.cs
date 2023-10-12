using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [HarmonyPatch(typeof(InventoryGui))]
  static class InventoryGuiPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InventoryGui.SetupCrafting))]
    static void SetupCraftingPrefix(InventoryGui __instance) {
      if (IsModEnabled.Value) {
        RecipeFilterController.AddRecipeFilter(__instance);
      }
    }
  }
}
