using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Keysential {
  public static class GlobalKeysManager {
    public static Dictionary<string, Coroutine> CurrentKeyManagers { get; private set; } = new();

    public static bool StartKeyManager(string managerId, IEnumerator managerMethod) {
      if (CurrentKeyManagers.ContainsKey(managerId)) {
        Keysential.LogError($"Cannot start due to existing KeyManager coroutine found with id: {managerId}");
        return false;
      }

      Keysential.LogInfo($"Starting KeyManager coroutine with id: {managerId}");

      Coroutine managerCoroutine = ZNet.instance.StartCoroutine(managerMethod);
      CurrentKeyManagers[managerId] = managerCoroutine;

      return true;
    }

    public static bool StopKeyManager(string managerId) {
      if (!CurrentKeyManagers.TryGetValue(managerId, out Coroutine managerCoroutine)) {
        Keysential.LogError($"Could not find KeyManager coroutine with id: {managerId}");
        return false;
      }

      if (managerCoroutine != null) {
        Keysential.LogInfo($"Stopping KeyManager coroutine with id: {managerId}");
        ZNet.instance.StopCoroutine(managerCoroutine);
      }

      CurrentKeyManagers.Remove(managerId);
      return true;
    }

    public static void SendChatMessage(ZNetPeer netPeer, Vector3 position, string name, string message) {
      ZRoutedRpc.s_instance.InvokeRoutedRPC(
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
