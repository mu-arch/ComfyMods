using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [HarmonyPatch(typeof(StoreGui))]
  static class StoreGuiPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(StoreGui.IsVisible))]
    static void IsVisiblePostfix(ref bool __result) {
      if (!__result && IsModEnabled.Value && RecipeFilterController.IsFocused()) {
        __result = true;
      }
    }
  }
}
