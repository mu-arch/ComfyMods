using HarmonyLib;

using static HeyListen.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(SE_Demister))]
  static class SEDemisterPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SE_Demister.Setup))]
    static void SetupPrefix(ref SE_Demister __instance) {
      if (IsModEnabled.Value) {

      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SE_Demister.UpdateStatusEffect))]
    static void UpdateStatusEffectPostfix(ref SE_Demister __instance) {
      if (IsModEnabled.Value
          && DemisterBallLockPosition.Value
          && __instance.m_ballInstance
          && __instance.m_character) {
        __instance.m_ballInstance.transform.position =
            __instance.m_character.m_head.position + DemisterBallLockOffset.Value;
      }
    }
  }
}
