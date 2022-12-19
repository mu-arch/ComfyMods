using HarmonyLib;

using UnityEngine;

using static Dramamist.PluginConfig;

namespace Dramamist {
  [HarmonyPatch(typeof(Demister))]
  static class DemisterPatch {
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
