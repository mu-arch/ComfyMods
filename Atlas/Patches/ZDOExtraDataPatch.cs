using System.Collections.Generic;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZDOExtraData))]
  static class ZDOExtraDataPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZDOExtraData.PrepareSave))]
    static void PrepareSavePostfix() {
      int count = 0;

      foreach (KeyValuePair<ZDOID, long> pair in ZDOExtraData.s_tempTimeCreated) {
        if (ZDOExtraData.s_saveLongs.InitAndSet(pair.Key, Atlas.TimeCreatedHashCode, pair.Value)) {
          count++;
        }
      }

      ZLog.Log($"Saving new/modified ZDO.timeCreated for {count}/{ZDOExtraData.s_tempTimeCreated.Count} ZDOs.");
    }
  }
}
