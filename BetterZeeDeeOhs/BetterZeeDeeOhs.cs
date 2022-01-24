using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

using UnityEngine;

namespace BetterZeeDeeOhs {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterZeeDeeOhs : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeedeeohs";
    public const string PluginName = "BetterZeeDeeOhs";
    public const string PluginVersion = "1.2.0";

    static ConfigEntry<float> _sendZdosWaitInterval;

    Harmony _harmony;

    public void Awake() {
      _sendZdosWaitInterval = Config.Bind(
          "SendZdos",
          "sendZdosWaitInterval",
          0.05f,
          new ConfigDescription(
              "Minimum interval (in seconds) to wait between sending ZDOs to all peers.",
              new AcceptableValueRange<float>(0.01f, 1f)));

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    class ZdoPeerWrapper {
      public ZDOMan.ZDOPeer ZdoPeer { get; private set; }
      public long PeerUid { get; private set; }

      public List<ZDO> TempToSync { get; } = new();
      public List<ZDO> TempToSyncDistant { get; } = new();
      public HashSet<ZDO> TempToAdd { get; } = new();
      public HashSet<ZDOID> TempToRemove { get; } = new();

      public ZPackage SendPackage { get; } = new();
      public ZPackage SerializePackage { get; } = new();

      public Comparison<ZDO> ServerSendComparison { get; private set; }

      ZdoPeerWrapper(ZDOMan.ZDOPeer zdoPeer) {
        ZdoPeer = zdoPeer;
        PeerUid = zdoPeer.m_peer.m_uid;
        ServerSendComparison = new(ServerSendCompare);
      }

      public static ZdoPeerWrapper Create(ZDOMan.ZDOPeer zdoPeer) {
        return new(zdoPeer);
      }

      public static ZdoPeerWrapper Create(ZDOMan.ZDOPeer zdoPeer, ZdoPeerWrapper wrapper) {
        wrapper.ZdoPeer = zdoPeer;
        wrapper.PeerUid = zdoPeer.m_peer.m_uid;
        return wrapper;
      }

      int ServerSendCompare(ZDO x, ZDO y) {
        bool xIsPrioritized =
            x.m_type == ZDO.ObjectType.Prioritized && x.m_owner != 0L && x.m_owner != PeerUid;

        bool yIsPrioritized =
            y.m_type == ZDO.ObjectType.Prioritized && y.m_owner != 0L && y.m_owner != PeerUid;

        if (xIsPrioritized && yIsPrioritized) {
          return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
        }

        if (xIsPrioritized != yIsPrioritized) {
          return xIsPrioritized ? -1 : 1;
        }

        return x.m_type == y.m_type
            ? x.m_tempSortValue.CompareTo(y.m_tempSortValue)
            : y.m_type.CompareTo(x.m_type);
      }
    }

    static readonly ConcurrentDictionary<ZDOMan.ZDOPeer, ZdoPeerWrapper> _zdoPeerWrapperCache = new();

    static readonly ConcurrentDictionary<ZNetPeer, ZDOMan.ZDOPeer> _zdoPeerByNetPeerCache = new();
    static readonly ConcurrentDictionary<ZRpc, ZDOMan.ZDOPeer> _zdoPeerByRpcCache = new();
    static readonly ConcurrentDictionary<long, ZDOMan.ZDOPeer> _zdoPeerByUidCache = new();

    [HarmonyPatch(typeof(ZDOMan))]
    class ZDOManPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZDOMan.AddPeer))]
      static IEnumerable<CodeInstruction> AddPeerTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldfld, typeof(ZDOMan.ZDOPeer).GetField(nameof(ZDOMan.ZDOPeer.m_peer))),
                new CodeMatch(OpCodes.Ldfld, typeof(ZNetPeer).GetField(nameof(ZNetPeer.m_rpc))),
                new CodeMatch(OpCodes.Ldstr, "ZDOData"))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate<Action<ZDOMan.ZDOPeer>>(AddPeerDelegate))
            .InstructionEnumeration();
      }

      static void AddPeerDelegate(ZDOMan.ZDOPeer zdoPeer) {
        _zdoPeerWrapperCache.AddOrUpdate(zdoPeer, ZdoPeerWrapper.Create, ZdoPeerWrapper.Create);
        _zdoPeerByNetPeerCache.AddOrUpdate(zdoPeer.m_peer, zdoPeer, (_, _) => zdoPeer);
        _zdoPeerByRpcCache.AddOrUpdate(zdoPeer.m_peer.m_rpc, zdoPeer, (_, _) => zdoPeer);
        _zdoPeerByUidCache.AddOrUpdate(zdoPeer.m_peer.m_uid, zdoPeer, (_, _) => zdoPeer);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.FindPeer), typeof(ZNetPeer))]
      static bool FindPeerByZNetPeerPrefix(ref ZDOMan.ZDOPeer __result, ref ZNetPeer netPeer) {
        return !_zdoPeerByNetPeerCache.TryGetValue(netPeer, out __result);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.FindPeer), typeof(ZRpc))]
      static bool FindPeerByZRpcPrefix(ref ZDOMan.ZDOPeer __result, ref ZRpc rpc) {
        return !_zdoPeerByRpcCache.TryGetValue(rpc, out __result);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.GetPeer))]
      static bool GetPeer(ref ZDOMan.ZDOPeer __result, ref long uid) {
        return !_zdoPeerByUidCache.TryGetValue(uid, out __result);
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZDOMan.RemovePeer))]
      static IEnumerable<CodeInstruction> RemovePeerTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, typeof(ZDOMan).GetField(nameof(ZDOMan.m_peers))),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Callvirt))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
            .InsertAndAdvance(Transpilers.EmitDelegate<Action<ZNetPeer>>(RemovePeerDelegate))
            .InstructionEnumeration();
      }

      static void RemovePeerDelegate(ZNetPeer netPeer) {
        if (_zdoPeerByNetPeerCache.TryRemove(netPeer, out ZDOMan.ZDOPeer zdoPeer)) {
          _zdoPeerWrapperCache.TryRemove(zdoPeer, out _);
        }

        _zdoPeerByRpcCache.TryRemove(netPeer.m_rpc, out _);
        _zdoPeerByUidCache.TryRemove(netPeer.m_uid, out _);
      }

      static readonly ParallelOptions _parallelOptions = new() {
        MaxDegreeOfParallelism = 3,
      };

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.SendZDOToPeers))]
      static bool SendZdosToPeersPrefix(ref ZDOMan __instance, ref float dt) {
        __instance.m_sendTimer += dt;

        if (__instance.m_sendTimer <= _sendZdosWaitInterval.Value) {
          return false;
        }

        __instance.m_sendTimer = 0f;
        Parallel.ForEach(__instance.m_peers, _parallelOptions, peer => SendZdos(ZDOMan.m_instance, peer, false));

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.SendZDOs))]
      static bool SendZdosPrefix(ref ZDOMan __instance, ref bool __result, ref ZDOMan.ZDOPeer peer, ref bool flush) {
        __result = SendZdos(__instance, peer, flush);
        return false;
      }

      static bool SendZdos(ZDOMan zdoMan, ZDOMan.ZDOPeer peer, bool flush) {
        if (!peer.m_peer.m_socket.IsConnected()) {
          return false;
        }

        int sendQueueSize = ((ZSteamSocket) peer.m_peer.m_socket).GetSendQueueSize();

        if (!flush && sendQueueSize > 10240) {
          return false;
        }

        int sendQueueFreeSize = 10240 - sendQueueSize;

        if (sendQueueFreeSize < 2048) {
          return false;
        }

        ZdoPeerWrapper wrapper = _zdoPeerWrapperCache.GetOrAdd(peer, ZdoPeerWrapper.Create);
        wrapper.TempToSync.Clear();

        if (ZNet.m_isServer) {
          CreateServerSyncList(zdoMan, ZoneSystem.m_instance, peer, wrapper);
        } else {
          CreateClientSyncList(zdoMan, peer, wrapper);
        }

        if (wrapper.TempToSync.Count == 0 && peer.m_invalidSector.Count == 0) {
          return false;
        }

        wrapper.SendPackage.Clear();
        bool hasInvalidSector = peer.m_invalidSector.Count > 0;

        if (hasInvalidSector) {
          wrapper.SendPackage.Write(peer.m_invalidSector.Count);

          foreach (ZDOID zdoid in peer.m_invalidSector) {
            wrapper.SendPackage.Write(zdoid);
          }

          peer.m_invalidSector.Clear();
        } else {
          wrapper.SendPackage.Write(0);
        }

        float time = Time.time;
        bool isSendingZdos = false;

        for (int i = 0, count = wrapper.TempToSync.Count; i < count; i++) {
          if (wrapper.SendPackage.Size() > sendQueueFreeSize) {
            break;
          }

          ZDO zdo = wrapper.TempToSync[i];
          peer.m_forceSend.Remove(zdo.m_uid);

          if (!ZNet.m_isServer) {
            zdoMan.m_clientChangeQueue.Remove(zdo.m_uid);
          }

          if (!peer.ShouldSend(zdo)) {
            continue;
          }

          wrapper.SendPackage.Write(zdo.m_uid);
          wrapper.SendPackage.Write(zdo.m_ownerRevision);
          wrapper.SendPackage.Write(zdo.m_dataRevision);
          wrapper.SendPackage.Write(zdo.m_owner);
          wrapper.SendPackage.Write(zdo.m_position);

          wrapper.SerializePackage.Clear();
          zdo.Serialize(wrapper.SerializePackage);
          wrapper.SendPackage.Write(wrapper.SerializePackage);

          peer.m_zdos[zdo.m_uid] = new(zdo.m_dataRevision, zdo.m_ownerRevision, time);

          isSendingZdos = true;
          zdoMan.m_zdosSent++;
        }

        wrapper.SendPackage.Write(ZDOID.None);

        if (hasInvalidSector || isSendingZdos) {
          SendZdoData(peer.m_peer.m_rpc, wrapper.SendPackage);
        }

        return hasInvalidSector || isSendingZdos;
      }

      static readonly int _rpcZdoDataHashCode = "ZDOData".GetStableHashCode();

      static void SendZdoData(ZRpc rpc, ZPackage package) {
        if (!rpc.IsConnected()) {
          return;
        }

        rpc.m_pkg.Clear();
        rpc.m_pkg.Write(_rpcZdoDataHashCode);
        rpc.m_pkg.Write(package);

        rpc.m_sentPackages++;
        rpc.m_sentData += package.Size();

        ZSteamSocket socket = (ZSteamSocket) rpc.m_socket;
        socket.Send(rpc.m_pkg);
      }

      static void CreateServerSyncList(
          ZDOMan zdoMan, ZoneSystem zoneSystem, ZDOMan.ZDOPeer zdoPeer, ZdoPeerWrapper wrapper) {
        wrapper.TempToSyncDistant.Clear();

        zdoMan.FindSectorObjects(
            zoneSystem.GetZone(zdoPeer.m_peer.m_refPos),
            zoneSystem.m_activeArea,
            zoneSystem.m_activeDistantArea,
            wrapper.TempToSync,
            wrapper.TempToSyncDistant);

        ServerSortSendZdos(zdoPeer, zdoPeer.m_peer.m_refPos.sqrMagnitude, wrapper.TempToSync, wrapper);

        wrapper.TempToSync.AddRange(wrapper.TempToSyncDistant);
        AddForceSendZdos(zdoMan, zdoPeer, wrapper);
      }

      static void ServerSortSendZdos(
          ZDOMan.ZDOPeer zdoPeer, float refPosSqrMagnitude, List<ZDO> zdos, ZdoPeerWrapper wrapper) {
        float time = Time.time;

        for (int i = 0, count = zdos.Count; i < count; i++) {
          ZDO zdo = zdos[i];

          if (zdoPeer.m_zdos.TryGetValue(zdo.m_uid, out ZDOMan.ZDOPeer.PeerZDOInfo zdoInfo)) {
            zdo.m_tempHaveRevision = true;
            zdo.m_tempSortValue = Mathf.Clamp(time - zdoInfo.m_syncTime, 0f, 100f) * 1.5f;
            zdo.m_tempSortValue =
                zdo.m_position.sqrMagnitude - refPosSqrMagnitude - (zdo.m_tempSortValue * zdo.m_tempSortValue);
          } else {
            zdo.m_tempHaveRevision = false;
            zdo.m_tempSortValue = zdo.m_position.sqrMagnitude - refPosSqrMagnitude - 22500f;
          }
        }

        zdos.Sort(wrapper.ServerSendComparison);
      }

      static void CreateClientSyncList(ZDOMan zdoMan, ZDOMan.ZDOPeer zdoPeer, ZdoPeerWrapper wrapper) {
        wrapper.TempToRemove.Clear();

        foreach (ZDOID zdoid in zdoMan.m_clientChangeQueue) {
          if (zdoid != ZDOID.None && zdoMan.m_objectsByID.TryGetValue(zdoid, out ZDO zdo) && zdo != null) {
            wrapper.TempToSync.Add(zdo);
          } else {
            wrapper.TempToRemove.Add(zdoid);
          }
        }

        foreach (ZDOID zdoid in wrapper.TempToRemove) {
          zdoMan.m_clientChangeQueue.Remove(zdoid);
        }

        ClientSortSendZdos(zdoPeer, wrapper.TempToSync);
        AddForceSendZdos(zdoMan, zdoPeer, wrapper);
      }

      static void ClientSortSendZdos(ZDOMan.ZDOPeer zdoPeer, List<ZDO> zdos) {
        float time = Time.time;

        for (int i = 0, count = zdos.Count; i < count; i++) {
          ZDO zdo = zdos[i];
          zdo.m_tempSortValue =
              zdoPeer.m_zdos.TryGetValue(zdo.m_uid, out ZDOMan.ZDOPeer.PeerZDOInfo zdoInfo)
                  ? Mathf.Clamp(time - zdoInfo.m_syncTime, 0f, 100f) * -1.5f
                  : -150f;
        }

        zdos.Sort(_clientSendComparison);
      }

      static readonly Comparison<ZDO> _clientSendComparison = new(ClientSendCompare);

      static int ClientSendCompare(ZDO x, ZDO y) {
        if (x.m_type == ZDO.ObjectType.Prioritized && y.m_type == ZDO.ObjectType.Prioritized) {
          return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
        }

        if (x.m_type == ZDO.ObjectType.Prioritized) {
          return -1;
        }

        if (y.m_type == ZDO.ObjectType.Prioritized) {
          return 1;
        }

        return x.m_tempSortValue.CompareTo(y.m_tempSortValue);
      }

      static void AddForceSendZdos(ZDOMan zdoMan, ZDOMan.ZDOPeer zdoPeer, ZdoPeerWrapper wrapper) {
        if (zdoPeer.m_forceSend.Count <= 0) {
          return;
        }

        wrapper.TempToAdd.Clear();
        wrapper.TempToRemove.Clear();

        foreach (ZDOID zdoid in zdoPeer.m_forceSend) {
          if (zdoid != ZDOID.None && zdoMan.m_objectsByID.TryGetValue(zdoid, out ZDO zdo) && zdo != null) {
            wrapper.TempToAdd.Add(zdo);
          } else {
            wrapper.TempToRemove.Add(zdoid);
          }
        }

        zdoPeer.m_forceSend.ExceptWith(wrapper.TempToRemove);
        wrapper.TempToSync.InsertRange(index: 0, wrapper.TempToAdd);

        wrapper.TempToAdd.Clear();
        wrapper.TempToRemove.Clear();
      }
    }
  }
}