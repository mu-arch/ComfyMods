using HarmonyLib;

using UnityEngine;

using static ColorfulLights.ColorfulLights;
using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [HarmonyPatch(typeof(ZNetScene))]
  static class ZNetScenePatch {
    static readonly int _vfxFireWorkTestHashCode = "vfx_FireWorkTest".GetStableHashCode();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZNetScene.RPC_SpawnObject))]
    static bool RPC_SpawnObjectPrefix(
        ref ZNetScene __instance, long spawner, Vector3 pos, Quaternion rot, int prefabHash) {
      if (!IsModEnabled.Value || prefabHash != _vfxFireWorkTestHashCode || rot == Quaternion.identity) {
        return true;
      }

      Color fireworksColor = Utils.Vec3ToColor(new Vector3(rot.x, rot.y, rot.z));

      PluginLogger.LogInfo($"Spawning fireworks with color: {fireworksColor}, rotation: {rot}");
      GameObject fireworksClone = Object.Instantiate(__instance.GetPrefab(prefabHash), pos, rot);

      FireplaceColor.SetParticleColors(
          fireworksClone.GetComponentsInChildren<Light>(includeInactive: true),
          fireworksClone.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
          fireworksClone.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true),
          fireworksColor);

      return false;
    }
  }
}
