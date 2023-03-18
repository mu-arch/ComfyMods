using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static Keysential.PluginConfig;

namespace Keysential {
  public class VendorKeyManager : MonoBehaviour {
    static readonly float _vendorNearbyDistance = 8f;
    static readonly string _vendorNearbyGlobalKey = "defeated_goblinking";

    void Awake() {
      if (ZNet.m_isServer && VendorKeyManagerPosition.Value != Vector3.zero) {
        StartCoroutine(VendorPlayerProximityCoroutine(VendorKeyManagerPosition.Value));
      }
    }

    IEnumerator VendorPlayerProximityCoroutine(Vector3 vendorPosition) {
      ZLog.Log($"Starting VendorPlayProximity coroutine at position: {vendorPosition}");

      List<string> originalKeys = new(ZoneSystem.m_instance.m_globalKeys);
      List<string> nearbyKeys = new(originalKeys);
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

    static readonly string[] _nearbyChatMessages = new string[] {
      "I'm egg-traordinary!",
      "I'm feeling egg-cellent today!",
      "I'm egg-ceptional!",
      "I'm egg-cited to see you!",
      "You're so eggs-tra!",
      "My wares are in-eggs-pensive!",
      "I'm egg-static!",
      "Egg-cuse me?",
      "I have egg-axctly the thing you're after.",
      "Buy my eggs-clusive wares!",
    };

    void SendChatMessage(ZNetPeer netPeer, Vector3 position, string name, string message) {
      ZRoutedRpc.m_instance.InvokeRoutedRPC(
          netPeer.m_uid,
          "ChatMessage",
          position,
          (int) Talker.Type.Normal,
          new UserInfo() {
            Name = name,
            Gamertag = name,
            NetworkUserId = PrivilegeManager.GetNetworkUserId(),
          },
          message,
          PrivilegeManager.GetNetworkUserId());
    }
  }
}
