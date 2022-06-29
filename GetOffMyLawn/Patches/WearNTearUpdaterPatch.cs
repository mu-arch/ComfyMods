using HarmonyLib;

using System.Collections;

using UnityEngine;

using static GetOffMyLawn.GetOffMyLawn;
using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(WearNTearUpdater))]
  public class WearNTearUpdaterPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(WearNTearUpdater.Awake))]
    static void AwakePostfix(ref WearNTearUpdater __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      __instance.StartCoroutine(LogCountersCoroutine());
    }

    static IEnumerator LogCountersCoroutine() {
      WaitForSeconds _waitInterval = new(seconds: 60f);

      while (true) {
        yield return _waitInterval;

        if (IsModEnabled.Value && EnablePieceHealthDamageThreshold.Value) {
          PluginLogger.LogInfo(
              $"WearNTear.ApplyDamage() ignored... 60s: {ApplyDamageCountLastMin} (Total: {ApplyDamageCount})");

          ApplyDamageCountLastMin = 0L;
        }
      }
    }
  }
}
