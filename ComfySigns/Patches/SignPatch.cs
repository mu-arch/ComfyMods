using ComfyLib;

using HarmonyLib;

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
  }
}
