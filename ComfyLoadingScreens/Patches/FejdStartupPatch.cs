using HarmonyLib;

using UnityEngine.UI;

namespace ComfyLoadingScreens {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.LoadMainScene))]
    static void LoadMainScene(ref FejdStartup __instance) {
      ComfyLoadingScreens.SetCustomLoadingImage(
      __instance.m_loading.transform.Find("Bkg").Ref()?.GetComponent<Image>());

      Text loadingText = __instance.m_loading.transform.Find("Text").Ref()?.GetComponent<Text>();

      ComfyLoadingScreens.SetupTipText(loadingText);
      ComfyLoadingScreens.SetCustomLoadingTip(loadingText);
    }
  }
}
