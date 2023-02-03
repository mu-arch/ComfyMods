using ComfyLib;

using HarmonyLib;

using UnityEngine;

namespace Intermission {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    static Transform _panelSeparator;
    static Transform _loadingBlack;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      Intermission.SetupTipText(__instance.m_loadingTip);
      Intermission.SetupLoadingImage(__instance.m_loadingImage);

      _panelSeparator = __instance.m_loadingProgress.transform.Find("panel_separator");
      Intermission.SetupPanelSeparator(_panelSeparator);

      Intermission.SetLoadingTip(__instance.m_loadingTip);
      Intermission.SetLoadingImage(__instance.m_loadingImage);

      __instance.m_loadingProgress.transform.Find("TopFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("BottomFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("text_darken").Ref()?.gameObject.SetActive(false);

      __instance.m_teleportingProgress = __instance.m_loadingProgress;
      __instance.m_useRandomImages = CustomAssets.LoadingImageFiles.Count <= 0;

      _loadingBlack = __instance.transform.Find("LoadingBlack");
      __instance.m_loadingImage.transform.SetParent(_loadingBlack, false);
      __instance.m_loadingTip.transform.SetParent(_loadingBlack, false);
      _panelSeparator.SetParent(_loadingBlack, false);
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
        Transform parentTransform = Player.m_localPlayer ? __instance.m_loadingProgress.transform : _loadingBlack;

        __instance.m_loadingImage.transform.SetParent(parentTransform, false);
        __instance.m_loadingTip.transform.SetParent(parentTransform, false);
        _panelSeparator.SetParent(parentTransform, false);

        Intermission.SetLoadingImage(__instance.m_loadingImage);
        Intermission.SetLoadingTip(__instance.m_loadingTip);

        Intermission.ScaleLerpLoadingImage(__instance.m_loadingImage);
      }
    }
  }
}