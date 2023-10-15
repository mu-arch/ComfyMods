using HarmonyLib;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Intermission {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    static void AwakePostfix(ref FejdStartup __instance) {
      Image loadingImage = __instance.m_loading.transform.Find("Bkg").GetComponent<Image>();

      // FejdStartup.m_menuAnimator locks the vanilla TMP_Text UI state, so work-around is to clone it to a new object.
      TMP_Text sourceLoadingText = __instance.m_loading.transform.Find("Text").GetComponent<TMP_Text>();
      TMP_Text loadingText = UnityEngine.Object.Instantiate(sourceLoadingText, __instance.m_loading.transform);
      loadingText.name = sourceLoadingText.name;
      UnityEngine.Object.Destroy(sourceLoadingText.gameObject);

      Transform panelSeparator =
          UnityEngine.Object.Instantiate(
              __instance.m_menuList.transform.Find("ornament"), __instance.m_loading.transform);

      Intermission.SetupTipText(loadingText);
      Intermission.SetupLoadingImage(loadingImage);
      Intermission.SetupPanelSeparator(panelSeparator);

      Intermission.SetLoadingTip(loadingText);
      Intermission.SetLoadingImage(loadingImage);
    }
  }
}
