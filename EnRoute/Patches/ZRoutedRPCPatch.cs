using System.Collections.Generic;

using HarmonyLib;

namespace EnRoute {
  [HarmonyPatch(typeof(ZRoutedRpc))]
  static class ZRoutedRPCPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZRoutedRpc.RouteRPC))]
    static bool RouteRPCPrefix(ZRoutedRpc __instance, ZRoutedRpc.RoutedRPCData rpcData) {
      if (__instance.m_server
          || rpcData.m_targetPeerID != ZRoutedRpc.Everybody
          || !EnRoute.NearbyMethodHashCodes.Contains(rpcData.m_methodHash)) {
        return true;
      }

      foreach (ZNetPeer netPeer in __instance.m_peers) {
        if (netPeer.IsReady()) {
          RouteRPCToPeer(netPeer, EnRoute.NearbyUserIds, rpcData);
        }
      }

      return false;
    }

    static readonly ZPackage _package = new();

    static void RouteRPCToPeer(ZNetPeer netPeer, HashSet<long> targetPeerIds, ZRoutedRpc.RoutedRPCData rpcData) {
      if (targetPeerIds.Count > 0) {
        foreach (long targetPeerId in targetPeerIds) {
          rpcData.m_targetPeerID = targetPeerId;
          RouteRPCToPeer(netPeer, rpcData);
          EnRoute.RouteToNearbyCount++;
        }
      } else {
        rpcData.m_targetPeerID = netPeer.m_uid;
        RouteRPCToPeer(netPeer, rpcData);
        EnRoute.RouteToServerCount++;
      }
    }

    static void RouteRPCToPeer(ZNetPeer netPeer, ZRoutedRpc.RoutedRPCData rpcData) {
      _package.Clear();
      rpcData.Serialize(_package);
      netPeer.m_rpc.Invoke("RoutedRPC", _package);
    }
  }
}
