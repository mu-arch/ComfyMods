using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace ComfyLoadingScreens {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    static Image _loadingImage;
    static Text _loadingText;
    static Transform _panelSeparator;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    static void AwakePostfix(ref FejdStartup __instance) {
      _loadingImage = __instance.m_loading.transform.Find("Bkg").Ref()?.GetComponent<Image>();
      _loadingText = __instance.m_loading.transform.Find("Text").Ref()?.GetComponent<Text>();

      _panelSeparator =
          UnityEngine.Object.Instantiate(
              __instance.m_menuList.transform.Find("ornament"), __instance.m_loading.transform);

      ComfyLoadingScreens.SetupTipText(_loadingText);
      ComfyLoadingScreens.SetupPanelSeparator(_panelSeparator);

      ComfyLoadingScreens.SetCustomLoadingImage(_loadingImage);
      ComfyLoadingScreens.SetCustomLoadingTip(_loadingText);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.LoadMainScene))]
    static void LoadMainScene(ref FejdStartup __instance) {
      ComfyLoadingScreens.SetupTipText(_loadingText);
      ComfyLoadingScreens.SetupPanelSeparator(_panelSeparator);
    }
  }
}
