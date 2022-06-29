using HarmonyLib;

using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(MonsterAI))]
  public class MonsterAIPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MonsterAI.UpdateTarget))]
    static void UpdateTargetPrefix(ref MonsterAI __instance) {
      if (IsModEnabled.Value) {
        __instance.m_attackPlayerObjects = false;
      }
    }
  }
}
