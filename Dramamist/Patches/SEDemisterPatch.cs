using HarmonyLib;

using UnityEngine;

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
          && __instance.m_character == Player.m_localPlayer
          && __instance.m_ballInstance.TryGetComponentInChildren(out Demister demister)) {
        ZLog.Log($"Adding ForceField to local Player Demister.");

        Collider collider =
            demister.m_forceField.GetOrAddComponent<SphereCollider>()
                .SetRadius(demister.m_forceField.endRange)
                .SetIsTrigger(true);

        ParticleSystem.TriggerModule trigger = ParticleMist.m_instance.m_ps.trigger;
        trigger.AddCollider(collider);

        Dramamist.UpdateDemisterSettings(demister);
      }
    }
  }
}