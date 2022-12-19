using HarmonyLib;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(SE_Demister))]
  static class SEDemisterPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(SE_Demister.UpdateStatusEffect))]
    static void UpdateStatusEffectPrefix(ref SE_Demister __instance, ref bool __state) {
      if (IsModEnabled.Value) {
        __state = __instance.m_ballInstance;
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SE_Demister.UpdateStatusEffect))]
    static void UpdateStatusEffectPostfix(ref SE_Demister __instance, ref bool __state) {
      if (IsModEnabled.Value
          && __instance.m_ballInstance
          && !__state
          && __instance.m_ballInstance.TryGetComponentInChildren(out Demister demister)) {
        ZLog.Log($"Adding FadeOutParticleMist to player Demister.");

        demister.GetOrAddComponent<FadeOutParticleMist>();
        Dramamist.UpdateDemisterSettings(demister);
      }
    }
  }
}