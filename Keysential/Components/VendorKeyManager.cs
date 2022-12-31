using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Keysential {
  public class VendorKeyManager : MonoBehaviour {
    static readonly string _vendorPrefabName = "Vendor_BlackForest";
    static readonly float _vendorNearbyDistance = 5f;
    static readonly string _vendorNearbyGlobalKey = "defeated_goblinking";

    void Awake() {
      if (ZNet.m_isServer && ZoneSystem.m_instance.GetLocationIcon(_vendorPrefabName, out Vector3 vendorPosition)) {
        StartCoroutine(VendorPlayerProximityCoroutine(vendorPosition));
      }
    }

    IEnumerator VendorPlayerProximityCoroutine(Vector3 vendorPosition) {
      ZLog.Log($"Starting VendorPlayProximity coroutine at position: {vendorPosition}");

      if (TryGetNearbyVendorZdo(vendorPosition, 10f, out ZDO vendorZdo)) {
        ZLog.Log($"Using nearby Vendor ZDO position: {vendorZdo.m_position}");
        vendorPosition = vendorZdo.m_position;
      }

      List<string> originalKeys = new(ZoneSystem.m_instance.m_globalKeys);
      List<string> nearbyKeys = new(capacity: originalKeys.Count);
      nearbyKeys.AddRange(originalKeys);
      nearbyKeys.Add(_vendorNearbyGlobalKey);

      HashSet<long> nearbyPeers = new(capacity: 256);
      WaitForSeconds waitInterval = new(seconds: 3f);

      while (ZNet.m_instance) {
        foreach (ZNetPeer netPeer in ZNet.m_instance.m_peers) {
          bool isNearby = Vector3.Distance(netPeer.m_refPos, vendorPosition) <= _vendorNearbyDistance;

          if (isNearby) {
            if (nearbyPeers.Contains(netPeer.m_uid)) {
              // Do nothing.
            } else {
              ZLog.Log($"Sending nearby global keys to peer: {netPeer.m_uid}");
              ZRoutedRpc.m_instance.InvokeRoutedRPC(netPeer.m_uid, "GlobalKeys", nearbyKeys);
              nearbyPeers.Add(netPeer.m_uid);

              SendChatMessage(
                  netPeer,
                  vendorPosition,
                  "<color=green>Haldor</color>",
                  _nearbyChatMessages[Random.Range(0, _nearbyChatMessages.Length)]);
            }
          } else {
            if (nearbyPeers.Contains(netPeer.m_uid)) {
              ZLog.Log($"Sending original global keys to peer: {netPeer.m_uid}");
              ZRoutedRpc.m_instance.InvokeRoutedRPC(netPeer.m_uid, "GlobalKeys", originalKeys);
              nearbyPeers.Remove(netPeer.m_uid);
            } else {
              // Do nothing.
            }
          }
        }

        yield return waitInterval;
      }
    }

    static readonly int _haldorPrefabHashCode = "Haldor".GetStableHashCode();

    static bool TryGetNearbyVendorZdo(Vector3 vendorPosition, float radius, out ZDO vendorZdo) {
      vendorZdo = null;
      float closestDistance = radius;

      foreach (ZDO zdo in ZDOMan.m_instance.m_objectsByID.Values) {
        if (zdo.m_prefab == _haldorPrefabHashCode) {
          float distance = Vector3.Distance(zdo.m_position, vendorPosition);

          if (distance < closestDistance) {
            closestDistance = distance;
            vendorZdo = zdo;
          }
        }
      }

      return vendorZdo != null;
    }

    static readonly string[] _nearbyChatMessages = new string[] {
      "I'm egg-traordinary!",
      "I'm feeling egg-cellent today!",
      "I'm egg-traordinary, egg-ceptional, and egg-cellent!",
      "I'm egg-cited to see you!",
    };

    void SendChatMessage(ZNetPeer netPeer, Vector3 position, string name, string message) {
      ZRoutedRpc.m_instance.InvokeRoutedRPC(
          netPeer.m_uid,
          "ChatMessage",
          position,
          (int) Talker.Type.Normal,
          name,
          message,
          PrivilegeManager.GetNetworkUserId());
    }
  }
}
