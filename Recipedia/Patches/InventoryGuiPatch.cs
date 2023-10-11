using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [HarmonyPatch(typeof(InventoryGui))]
  static class InventoryGuiPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(InventoryGui.Awake))]
    static void AwakePostfix(ref InventoryGui __instance) {
      if (IsModEnabled.Value) {
        RecipeFilterController.AddRecipeFilter(__instance.m_recipeListRoot);
      }
    }
  }
}
