using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [HarmonyPatch(typeof(TextInput))]
  static class TextInputPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TextInput.IsVisible))]
    static void IsVisiblePostfix(ref bool __result) {
      if (!__result && IsModEnabled.Value && RecipeFilterController.IsFocused()) {
        __result = true;
      }
    }
  }
}
