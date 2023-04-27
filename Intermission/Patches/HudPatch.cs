using ComfyLib;

using HarmonyLib;

using UnityEngine;

namespace Intermission {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      Transform _panelSeparator = __instance.m_loadingProgress.transform.Find("panel_separator");

      Intermission.SetupTipText(__instance.m_loadingTip);
      Intermission.SetupLoadingImage(__instance.m_loadingImage);
      Intermission.SetupPanelSeparator(_panelSeparator);

      Intermission.SetLoadingTip(__instance.m_loadingTip);
      Intermission.SetLoadingImage(__instance.m_loadingImage);

      __instance.m_loadingProgress.transform.Find("TopFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("BottomFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("text_darken").Ref()?.gameObject.SetActive(false);

      __instance.m_teleportingProgress = __instance.m_loadingProgress;
      __instance.m_useRandomImages = CustomAssets.LoadingImageFiles.Count <= 0;

      Transform _loadingBlack = __instance.transform.Find("LoadingBlack");
      __instance.m_loadingImage.transform.SetParent(_loadingBlack, false);
      __instance.m_loadingTip.transform.SetParent(_loadingBlack, false);
      _panelSeparator.SetParent(_loadingBlack, false);
    }

    static bool _loadingScreenState;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPrefix(ref Hud __instance) {
      _loadingScreenState = __instance.m_loadingImage.gameObject.activeInHierarchy;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPostfix(ref Hud __instance) {
      if (!_loadingScreenState && __instance.m_loadingScreen.gameObject.activeInHierarchy) {
        Intermission.SetLoadingImage(__instance.m_loadingImage);
        Intermission.SetLoadingTip(__instance.m_loadingTip);
        Intermission.ScaleLerpLoadingImage(__instance.m_loadingImage);
      }
    }
  }
}