using System.Collections.Generic;

using HarmonyLib;

using UnityEngine;

using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [HarmonyPatch(typeof(ZNetScene))]
  static class ZNetScenePatch {
    static readonly int _vfxFireWorkTestHashCode = "vfx_FireWorkTest".GetStableHashCode();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZNetScene.RPC_SpawnObject))]
    static bool RPC_SpawnObjectPrefix(
        ref ZNetScene __instance, Vector3 pos, Quaternion rot, int prefabHash) {
      if (!IsModEnabled.Value || prefabHash != _vfxFireWorkTestHashCode || rot == Quaternion.identity) {
        return true;
      }

      Color fireworksColor = Utils.Vec3ToColor(new Vector3(rot.x, rot.y, rot.z));

      ZLog.Log($"Spawning fireworks with color: {fireworksColor}");
      GameObject fireworksClone = Object.Instantiate(__instance.GetPrefab(prefabHash), pos, rot);

      SetFireworkColors(
          fireworksClone.GetComponentsInChildren<Light>(includeInactive: true),
          fireworksClone.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
          fireworksClone.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true),
          fireworksColor);

      return false;
    }

    static void SetFireworkColors(
        IEnumerable<Light> lights,
        IEnumerable<ParticleSystem> systems,
        IEnumerable<ParticleSystemRenderer> renderers,
        Color color) {
      ParticleSystem.MinMaxGradient gradient = new(color);

      foreach (ParticleSystem system in systems) {
        ParticleSystem.ColorOverLifetimeModule colorOverLiftime = system.colorOverLifetime;

        if (colorOverLiftime.enabled) {
          colorOverLiftime.color = gradient;
        }

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;

        if (sizeOverLifetime.enabled) {
          ParticleSystem.MainModule main = system.main;
          main.startColor = color;
        }
      }

      foreach (ParticleSystemRenderer renderer in renderers) {
        renderer.material.color = color;
      }

      foreach (Light light in lights) {
        light.color = color;
      }
    }
  }
}
