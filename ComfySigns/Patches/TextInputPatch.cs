using ComfyLib;

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
        __instance.m_panel.GetOrAddComponent<TextInputPanelDragger>();

        __instance.m_textFieldTMP
            .SetRichText(false)
            .SetTextViewport(__instance.m_textFieldTMP.textComponent.GetComponent<RectTransform>());

        __instance.m_textFieldTMP.gameObject.GetOrAddComponent<RectMask2D>()
            .SetPadding(left: 2f, top: 0f, right: 2f, bottom: 2f)
            .SetSoftness(horizontal: 2, vertical: 0);
      }
    }
  }
}
