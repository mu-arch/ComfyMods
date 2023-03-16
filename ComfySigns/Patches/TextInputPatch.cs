using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

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
        __instance.m_textFieldTMP.textViewport = __instance.m_textFieldTMP.textComponent.GetComponent<RectTransform>();

        RectMask2D rectMask = __instance.m_textFieldTMP.gameObject.AddComponent<RectMask2D>();
        rectMask.padding = new(2f, 0f, 2f, 0f);
        rectMask.softness = new(2, 2);
      }
    }
  }
}
