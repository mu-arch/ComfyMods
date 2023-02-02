using ComfyLib;

using HarmonyLib;

using UnityEngine;

using static Intermission.PluginConfig;

namespace Intermission {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    static Transform _panelTransform;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      Intermission.SetupTipText(__instance.m_loadingTip);

      Intermission.SetLoadingTip(__instance.m_loadingTip);
      Intermission.SetLoadingImage(__instance.m_loadingImage);

      __instance.m_loadingProgress.transform.Find("TopFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("BottomFade").Ref()?.gameObject.SetActive(false);
      __instance.m_loadingProgress.transform.Find("text_darken").Ref()?.gameObject.SetActive(false);

      __instance.m_teleportingProgress = __instance.m_loadingProgress;
      __instance.m_useRandomImages = CustomAssets.LoadingImageFiles.Count <= 0;

      Transform loadingBlack = __instance.transform.Find("LoadingBlack");
      __instance.m_loadingImage.transform.SetParent(loadingBlack, false);
      __instance.m_loadingTip.transform.SetParent(loadingBlack, false);

      _panelTransform = __instance.m_loadingProgress.transform.Find("panel_separator");
      _panelTransform.SetParent(loadingBlack, false);

      Intermission.SetupPanelSeparator(_panelTransform);
    }

    static bool _teleportingProgressState;
    static bool _haveSetupLoadScreenState;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPrefix(ref Hud __instance) {
      _teleportingProgressState = __instance.m_teleportingProgress.activeInHierarchy;
      _haveSetupLoadScreenState = __instance.m_haveSetupLoadScreen;
    }

    static Coroutine _scaleLerpCoroutine;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPostfix(ref Hud __instance) {
      if ((!_haveSetupLoadScreenState && __instance.m_haveSetupLoadScreen)
          || (!_teleportingProgressState && __instance.m_teleportingProgress.activeInHierarchy)) {
        if (Player.m_localPlayer) {
          __instance.m_loadingImage.transform.SetParent(__instance.m_loadingProgress.transform, false);
          __instance.m_loadingTip.transform.SetParent(__instance.m_loadingProgress.transform, false);
          _panelTransform.SetParent(__instance.m_loadingProgress.transform, false);
        }

        Intermission.SetLoadingImage(__instance.m_loadingImage);
        Intermission.SetLoadingTip(__instance.m_loadingTip);

        if (_scaleLerpCoroutine != null) {
          __instance.StopCoroutine(_scaleLerpCoroutine);
        }

        if (LoadingImageUseScaleLerp.Value) {
          _scaleLerpCoroutine =
              __instance.StartCoroutine(
                  Intermission.ScaleLerp(
                      __instance.m_loadingImage.transform,
                      Vector3.one,
                      Vector3.one * LoadingImageScaleLerpEndScale.Value,
                      LoadingImageScaleLerpDuration.Value));
        }
      }
    }
  }
}