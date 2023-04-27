using HarmonyLib;

using static PotteryBarn.PluginConfig;

namespace PotteryBarn {
  [HarmonyPatch(typeof(EffectList))]
  static class EffectListPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EffectList.Create))]
    static void CreatePrefix(ref EffectList __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      foreach (EffectList.EffectData effectData in __instance.m_effectPrefabs) {
        if (effectData != null && effectData.m_enabled && !effectData.m_prefab) {
          effectData.m_enabled = false;
        }
      }
    }
  }
}
