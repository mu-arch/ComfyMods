using HarmonyLib;

using static ComfySigns.PluginConfig;

namespace ComfySigns {
  [HarmonyPatch(typeof(TextInput))]
  static class TextInputPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(TextInput.Awake))]
    static void AwakePostfix(ref TextInput __instance) {
      if (IsModEnabled.Value && __instance.m_textFieldTMP) {
        __instance.gameObject.AddComponent<TextInputPanelDragger>();
        __instance.m_textFieldTMP.richText = false;
      }
    }
  }
}
