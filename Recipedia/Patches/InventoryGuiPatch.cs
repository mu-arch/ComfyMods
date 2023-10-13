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
  }
}
