using HarmonyLib;

using UnityEngine;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(SE_Demister))]
  static class SEDemisterPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SE_Demister.Setup))]
    static void SetupPrefix(ref SE_Demister __instance) {
      if (IsModEnabled.Value && DemisterBallPrefab.Value.Length > 0) {
        GameObject prefab = ZNetScene.m_instance.GetPrefab(DemisterBallPrefab.Value);

        if (prefab && prefab.name != __instance.m_ballPrefab.name) {
          ZLog.Log($"Overriding ballPrefab from: {__instance.m_ballPrefab} to: {DemisterBallPrefab.Value}");
          __instance.m_ballPrefab = prefab;
        }
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
