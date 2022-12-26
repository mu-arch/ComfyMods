using HarmonyLib;

using UnityEngine;

using static HeyListen.PluginConfig;

namespace HeyListen {
  [HarmonyPatch(typeof(Demister))]
  static class DemisterPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Demister.Awake))]
    static void AwakePostfix(ref Demister __instance) {
      if (!IsModEnabled.Value || !DemisterBallUseCustomSettings.Value) {
        return;
      }

      GameObject prefab = __instance.transform.root.gameObject;

      if (prefab.name.StartsWith("demister_ball", System.StringComparison.Ordinal)
          && !prefab.TryGetComponent(out DemisterBallControl _)) {
        ZLog.Log($"Adding DemisterBallControl to demister_ball.");
        prefab.AddComponent<DemisterBallControl>();
      }
    }
  }
}
