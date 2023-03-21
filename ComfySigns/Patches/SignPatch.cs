using ComfyLib;

using HarmonyLib;

using TMPro;

using static ComfySigns.PluginConfig;

namespace ComfySigns {
  [HarmonyPatch(typeof(Sign))]
  static class SignPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Sign.Awake))]
    static void AwakePostfix(ref Sign __instance) {
      if (IsModEnabled.Value) {
        __instance.m_textWidget
            .SetFont(UIFonts.GetFontAsset(SignDefaultTextFont.Value))
            .SetColor(SignDefaultTextColor.Value);

        __instance.m_characterLimit = 999;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Sign.SetText))]
    static void SetTextPostfix(ref Sign __instance) {
      if (IsModEnabled.Value && SignEffectEnablePartyEffect.Value) {
        ProcessSignEffect(__instance);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Sign.UpdateText))]
    static void UpdateTextPostfix(ref Sign __instance) {
      if (IsModEnabled.Value && SignEffectEnablePartyEffect.Value) {
        ProcessSignEffect(__instance);
      }
    }

    static void ProcessSignEffect(Sign sign) {
      if (HasSignEffect(sign.m_textWidget, "party")) {
        if (!sign.m_textWidget.gameObject.TryGetComponent(out VertexColorCycler _)) {
          sign.m_textWidget.gameObject.AddComponent<VertexColorCycler>();
        }
      } else {
        if (sign.m_textWidget.gameObject.TryGetComponent(out VertexColorCycler colorCycler)) {
          UnityEngine.Object.Destroy(colorCycler);
          sign.m_textWidget.ForceMeshUpdate(ignoreActiveState: true);
        }
      }
    }

    static bool HasSignEffect(TMP_Text textComponent, string effectId) {
      if (textComponent.text.Length <= 0 || !textComponent.text.StartsWith("<link")) {
        return false;
      }

      foreach (TMP_LinkInfo linkInfo in textComponent.textInfo.linkInfo) {
        if (linkInfo.linkTextfirstCharacterIndex == 0
            && linkInfo.linkTextLength == textComponent.textInfo.characterCount
            && linkInfo.GetLinkID() == effectId) {
          return true;
        }
      }

      return false;
    }
  }
}
