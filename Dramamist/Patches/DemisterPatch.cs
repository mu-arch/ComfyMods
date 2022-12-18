using HarmonyLib;

using UnityEngine;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(Demister))]
  static class DemisterPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.Awake))]
    static void AwakePostfix(ref Demister __instance) {
      if (IsModEnabled.Value && ParticleMist.m_instance && Dramamist.IsLocalPlayerDemisterBall(__instance)) {
        Dramamist.UpdateDemisterSettings(__instance);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.OnEnable))]
    static void OnEnablePostfix(ref Demister __instance) {
      if (IsModEnabled.Value
          && ParticleMist.m_instance
          && __instance.m_forceField.gameObject.TryGetComponent(out SphereCollider collider)) {
        ParticleMist.m_instance.m_ps.trigger.AddCollider(collider);
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.OnDisable))]
    static void OnDisablePostfix(ref Demister __instance) {
      if (IsModEnabled.Value
          && ParticleMist.m_instance
          && __instance.m_forceField.gameObject.TryGetComponent(out SphereCollider collider)) {
        ParticleMist.m_instance.m_ps.trigger.RemoveCollider(collider);
      }
    }
  }
}
