using HarmonyLib;

using UnityEngine;

using static SkyTree.PluginConfig;

namespace SkyTree {
  [HarmonyPatch(typeof(ZoneSystem))]
  static class ZoneSystemPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZoneSystem.SpawnZone))]
    static void SpawnZonePrefix() {
      if (IsModEnabled.Value) {
        foreach (GameObject branch in SkyTree.YggdrasilBranches) {
          branch.layer = SkyTree.SkyboxLayer;
        }
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZoneSystem.SpawnZone))]
    static void SpawnZonePostfix() {
      if (IsModEnabled.Value) {
        foreach (GameObject branch in SkyTree.YggdrasilBranches) {
          branch.layer = SkyTree.StaticSolidLayer;
        }
      }
    }
  }
}
