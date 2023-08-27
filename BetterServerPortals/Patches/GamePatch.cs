using HarmonyLib;

using UnityEngine;

namespace BetterServerPortals {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.Start))]
    static void StartPostfix(Game __instance) {
      if (ZNet.m_isServer) {
        __instance.StopCoroutine(nameof(Game.ConnectPortalsCoroutine));
        __instance.StartCoroutine(BetterServerPortals.ConnectPortalsCoroutine(ZDOMan.s_instance));
      }

      foreach (GameObject prefab in __instance.m_portalPrefabs) {
        ZLog.Log($"PortalPrefab is: {(prefab ? prefab.name : default)})");
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Game.ConnectPortals))]
    static bool ConnectPortalsPrefix() {
      BetterServerPortals.ConnectPortals(ZDOMan.s_instance);
      return false;
    }
  }
}
