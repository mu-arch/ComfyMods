using HarmonyLib;

namespace ComfyLoadingScreens {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      ComfyLoadingScreens.SetupTipText(__instance.m_loadingTip);
      ComfyLoadingScreens.SetCustomLoadingTip(__instance.m_loadingTip);
      ComfyLoadingScreens.SetCustomLoadingImage(__instance.m_loadingImage);

      __instance.m_loadingProgress.transform.Find("TopFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("BottomFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("text_darken").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("panel_separator").Ref()?.gameObject.SetActive(false);

      __instance.m_teleportingProgress = __instance.m_loadingProgress;
      __instance.m_useRandomImages = ComfyLoadingScreens.CustomLoadingImageFiles.Count <= 0;
    }

    static bool _teleportingProgressState;
    static bool _haveSetupLoadScreenState;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPrefix(ref Hud __instance) {
      _teleportingProgressState = __instance.m_teleportingProgress.activeInHierarchy;
      _haveSetupLoadScreenState = __instance.m_haveSetupLoadScreen;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPostfix(ref Hud __instance) {
      if ((!_haveSetupLoadScreenState && __instance.m_haveSetupLoadScreen)
          || (!_teleportingProgressState && __instance.m_teleportingProgress.activeInHierarchy)) {
        ComfyLoadingScreens.SetCustomLoadingImage(__instance.m_loadingImage);
        ComfyLoadingScreens.SetCustomLoadingTip(__instance.m_loadingTip);
      }
    }
  }
}
