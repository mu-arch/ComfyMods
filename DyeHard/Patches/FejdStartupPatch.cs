using HarmonyLib;

using static DyeHard.DyeHard;
using static DyeHard.PluginConfig;

namespace DyeHard.Patches {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.SetupObjectDB))]
    static void SetupObjectDbPostfix(ref FejdStartup __instance) {
      BindCustomizationConfig(
          __instance.GetComponent<ObjectDB>(),
          __instance.m_newCharacterPanel.GetComponent<PlayerCustomizaton>());
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.SetupCharacterPreview))]
    static void SetupCharacterPreviewPostfix(ref FejdStartup __instance) {
      LocalPlayerCache = __instance.m_playerInstance.GetComponent<Player>();

      if (IsModEnabled.Value) {
        SetPlayerZdoHairColor();
        SetPlayerHairItem();
        SetPlayerBeardItem();

        __instance.m_playerInstance.transform.Translate(OffsetCharacterPreviewPosition.Value);
      }
    }
  }
}
