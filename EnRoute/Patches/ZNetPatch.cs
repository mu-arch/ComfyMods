using System.Collections;
using System.Diagnostics;

using HarmonyLib;

using UnityEngine;

namespace EnRoute {
  [HarmonyPatch(typeof(ZNet))]
  static class ZNetPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNet.RPC_PlayerList))]
    static void RPC_PlayerListPostfix() {
      RouteManager.RefreshNearbyPlayers();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNet.UpdatePlayerList))]
    static void UpdatePlayerListPostfix(ZNet __instance) {
      
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ZNet.Awake))]
    static void AwakePostfix(ZNet __instance) {
      __instance.StartCoroutine(LogStatsCoroutine());
    }

    static IEnumerator LogStatsCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 60f);
      Stopwatch stopwatch = Stopwatch.StartNew();

      while (true) {
        yield return waitInterval;
        RouteManager.LogStats(stopwatch.Elapsed);
      }
    }
  }
}
