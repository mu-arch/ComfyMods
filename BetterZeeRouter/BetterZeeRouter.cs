using BepInEx;

using HarmonyLib;

using System.Collections.Concurrent;
using System.Reflection;

using UnityEngine;

namespace BetterZeeRouter {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterZeeRouter : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeerouter";
    public const string PluginName = "BetterZeeRouter";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly int _damageTextHashCode = "DamageText".GetStableHashCode();
    static readonly int _sayHashCode = "Say".GetStableHashCode();

    static readonly ConcurrentDictionary<long, ZNetPeer> _netPeerByUidCache = new();
    static readonly ZPackage _rpcDataPackage = new();

    static bool ShouldUsePeerSqrDistance(
        ref ZRoutedRpc.RoutedRPCData rpcData, out Vector3 origin, out float rangeSqr) {
      if (_netPeerByUidCache.TryGetValue(rpcData.m_senderPeerID, out ZNetPeer netPeer)) {
        origin = netPeer.m_refPos;

        if (rpcData.m_methodHash == _damageTextHashCode) {
          rangeSqr = 5000f;
          return true;
        }

        if (rpcData.m_methodHash == _sayHashCode) {
          rangeSqr = 5000f;
          return true;
        }
      }

      origin = Vector3.zero;
      rangeSqr = 0f;

      return false;
    }

    [HarmonyPatch(typeof(ZRoutedRpc))]
    class ZRoutedRpcPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.AddPeer))]
      static void AddPeerPostfix(ref ZNetPeer peer) {
        _netPeerByUidCache[peer.m_uid] = peer;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.RemovePeer))]
      static void RemovePeerPostfix(ref ZNetPeer peer) {
        _netPeerByUidCache.TryRemove(peer.m_uid, out _);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRoutedRpc.GetPeer))]
      static bool GetPeerPrefix(ref ZNetPeer __result, ref long uid) {
        _netPeerByUidCache.TryGetValue(uid, out __result);
        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRoutedRpc.RouteRPC))]
      static bool RouteRPCPrefix(ref ZRoutedRpc __instance, ref ZRoutedRpc.RoutedRPCData rpcData) {
        if (!__instance.m_server) {
          return true;
        }

        _rpcDataPackage.Clear();
        rpcData.Serialize(_rpcDataPackage);

        if (rpcData.m_targetPeerID == 0L) {
          bool usePeerSqrDistance = ShouldUsePeerSqrDistance(ref rpcData, out Vector3 origin, out float rangeSqr);

          foreach (ZNetPeer netPeer in __instance.m_peers) {
            if (netPeer.m_uid == rpcData.m_senderPeerID
                || !netPeer.IsReady()
                || (usePeerSqrDistance && (netPeer.m_refPos - origin).sqrMagnitude > rangeSqr)) {
              continue;
            }

            netPeer.m_rpc.Invoke("RoutedRPC", _rpcDataPackage);
          }

          return false;
        }

        if (_netPeerByUidCache.TryGetValue(rpcData.m_targetPeerID, out ZNetPeer targetPeer)
            && targetPeer != null
            && targetPeer.IsReady()) {
          targetPeer.m_rpc.Invoke("RoutedRPC", _rpcDataPackage);
        }

        return false;
      }
    }
  }
}