using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace Intermission {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    static void AwakePostfix(ref FejdStartup __instance) {
      Image _loadingImage = __instance.m_loading.transform.Find("Bkg").Ref()?.GetComponent<Image>();
      Text _loadingText = __instance.m_loading.transform.Find("Text").Ref()?.GetComponent<Text>();

      Transform _panelSeparator =
          UnityEngine.Object.Instantiate(
              __instance.m_menuList.transform.Find("ornament"), __instance.m_loading.transform);

      Intermission.SetupTipText(_loadingText);
      Intermission.SetupPanelSeparator(_panelSeparator);

      Intermission.SetLoadingImage(_loadingImage);
      Intermission.SetLoadingTip(_loadingText);
    }
  }
}
