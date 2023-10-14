using System;
using System.Collections.Generic;

namespace EnRoute {
  public static class RouteManager {
    public static readonly Dictionary<long, ZNetPeer> NetPeers = new();
    public static readonly Dictionary<ZNetPeer, RouteRecord> NetPeerRouting = new();

    public static void OnAddPeer(ZNetPeer netPeer) {
      NetPeers[netPeer.m_uid] = netPeer;
      NetPeerRouting[netPeer] = new(netPeer);
    }

    public static void OnRemovePeer(ZNetPeer netPeer) {
      NetPeers.Remove(netPeer.m_uid);
      NetPeerRouting.Remove(netPeer);
    }

    public static readonly HashSet<long> NearbyUserIds = new();

    public static long RouteToNearbyCount = 0L;
    public static long RouteToServerCount = 0L;

    public static void LogStats(TimeSpan timeElapsed) {
      ZLog.Log($"RouteToNearby: {RouteToNearbyCount}, RouteToServer: {RouteToServerCount}, Elapsed: {timeElapsed}");
    }

    public static void RefreshNearbyPlayers() {
      NearbyUserIds.Clear();

      ZoneSystem zoneSystem = ZoneSystem.m_instance;
      ZDOID playerCharacterId = ZNet.m_instance.m_characterID;
      Vector2i playerZone = zoneSystem.GetZone(ZNet.m_instance.m_referencePosition);

      foreach (ZNet.PlayerInfo playerInfo in ZNet.m_instance.m_players) {
        ZDOID characterId = playerInfo.m_characterID;

        if (characterId.IsNone() || characterId == playerCharacterId) {
          continue;
        }

        if (!playerInfo.m_publicPosition
            || Vector2i.Distance(playerZone, zoneSystem.GetZone(playerInfo.m_position)) <= 2) {
          NearbyUserIds.Add(characterId.UserID);
        }
      }
    }

    static readonly ZPackage _package = new();

    public static void RouteRPCToNearbyPeers(ZRoutedRpc routedRpc, ZRoutedRpc.RoutedRPCData rpcData) {
      foreach (ZNetPeer netPeer in routedRpc.m_peers) {
        if (netPeer.IsReady()) {
          RouteRPCToPeer(netPeer, NearbyUserIds, rpcData);
        }
      }
    }

    public static void RouteRPCToPeer(ZNetPeer netPeer, HashSet<long> targetPeerIds, ZRoutedRpc.RoutedRPCData rpcData) {
      if (netPeer.m_server) {
        rpcData.m_targetPeerID = netPeer.m_uid;
        RouteRPCToPeer(netPeer, rpcData);
        RouteToServerCount++;
      }

      if (targetPeerIds.Count > 0) {
        foreach (long targetPeerId in targetPeerIds) {
          rpcData.m_targetPeerID = targetPeerId;
          RouteRPCToPeer(netPeer, rpcData);
          RouteToNearbyCount++;
        }
      }
    }

    static void RouteRPCToPeer(ZNetPeer netPeer, ZRoutedRpc.RoutedRPCData rpcData) {
      _package.Clear();
      rpcData.Serialize(_package);
      netPeer.m_rpc.Invoke("RoutedRPC", _package);
    }
  }
}
