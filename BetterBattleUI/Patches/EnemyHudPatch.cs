using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static BetterBattleUI.PluginConfig;

namespace BetterBattleUI {
  [HarmonyPatch(typeof(EnemyHud))]
  static class EnemyHudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyHud.Awake))]
    static void AwakePostfix(ref EnemyHud __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      // TODO: temporary, just for testing.
      GameObject bossName = __instance.m_baseHudBoss.transform.Find("Name").gameObject;
      bossName.AddComponent<VerticalGradient>();
      bossName.AddComponent<WobblyText>();
    }
  }
}
