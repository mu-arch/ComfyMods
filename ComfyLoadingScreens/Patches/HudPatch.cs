using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace ComfyLoadingScreens {
  [HarmonyPatch(typeof(Hud))]
  static class HudPatch {
    static readonly List<string> _customTips = new();
    static readonly List<string> _customImageFiles = new();

    static Text _teleportLoadingTipText;
    static Image _teleportLoadingImage;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      _customImageFiles.Clear();
      _customImageFiles.AddRange(ComfyLoadingScreens.GetCustomLoadingImageFiles());

      _teleportLoadingImage = CreateLoadingImage(__instance);
      _teleportLoadingImage.gameObject.SetActive(_customImageFiles.Count > 0);

      if (_customImageFiles.Count > 0) {
        __instance.m_useRandomImages = false;
      }

      _customTips.Clear();
      _customTips.AddRange(ComfyLoadingScreens.GetCustomLoadingTips());

      __instance.m_loadingTip.horizontalOverflow = HorizontalWrapMode.Overflow;
      __instance.m_loadingTip.fontSize = 20;
      __instance.m_loadingTip.color = Color.white;
      __instance.m_loadingTip.GetComponent<Outline>().enabled = true;

      _teleportLoadingTipText = CreateLoadingTipText(__instance);
      _teleportLoadingTipText.gameObject.SetActive(true);
    }

    static Text CreateLoadingTipText(Hud hud) {
      Text tipText = UnityEngine.Object.Instantiate(hud.m_loadingTip, hud.m_teleportingProgress.transform);
      tipText.name = "LoadingTip";

      return tipText;
    }

    static Image CreateLoadingImage(Hud hud) {
      Image loadingImage = UnityEngine.Object.Instantiate(hud.m_loadingImage, hud.m_teleportingProgress.transform);
      loadingImage.name = "LoadingImage";

      return loadingImage;
    }

    static bool _teleportingProgressState;
    static bool _haveSetupLoadingScreenState;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPrefix(ref Hud __instance) {
      _teleportingProgressState = __instance.m_teleportingProgress.activeInHierarchy;
      _haveSetupLoadingScreenState = __instance.m_haveSetupLoadScreen;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.UpdateBlackScreen))]
    static void UpdateBlackScreenPostfix(ref Hud __instance) {
      if (!_teleportingProgressState && __instance.m_teleportingProgress.activeInHierarchy) {
        SetupCustomLoading(_teleportLoadingImage, _teleportLoadingTipText);
      }

      if (!_haveSetupLoadingScreenState && __instance.m_haveSetupLoadScreen) {
        SetupCustomLoading(__instance.m_loadingImage, __instance.m_loadingTip);
      }
    }

    static void SetupCustomLoading(Image loadingImage, Text loadingTip) {
      if (_customTips.Count > 0) {
        string customTip = _customTips.RandomElement();
        ZLog.Log($"Using custom tip: {customTip}");
        loadingTip.text = customTip;
      }

      if (_customImageFiles.Count > 0) {
        Sprite customImageSprite = ComfyLoadingScreens.GetCustomLoadingImage(_customImageFiles.RandomElement());

        if (customImageSprite) {
          ZLog.Log($"Using custom image sprite: {customImageSprite.name}");
          loadingImage.sprite = customImageSprite;
          loadingImage.type = Image.Type.Simple;
          loadingImage.preserveAspect = true;
        }
      }
    }
  }
}
