using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Steamworks;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace BetterZeeNet {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeNet : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeenet";
    public const string PluginName = "BetterZeeNet";
    public const string PluginVersion = "1.6.0";

    static ConfigEntry<bool> _isModEnabled;

    static ConfigEntry<bool> _useCustomPrecomputedPings;
    static ConfigEntry<float> _customPingInterval;
    static ConfigEntry<float> _customPingTimeout;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      _useCustomPrecomputedPings =
          Config.Bind(
              "ZRpc.Ping",
              "useCustomPrecomputedPings",
              true,
              "Override ReceivePing/UpdatePing methods use custom ping interval/timeout and precomputed data.");

      _customPingInterval =
          Config.Bind(
              "ZRpc.Ping",
              "customPingInterval",
              ZRpc.m_pingInterval,
              new ConfigDescription(
                  "Override the minimum interval (in seconds) before a ping is sent per connection.",
                  new AcceptableValueRange<float>(0.1f, 3f)));

      _customPingTimeout =
          Config.Bind(
              "ZRpc.Ping",
              "customPingTimeout",
              ZRpc.m_timeout,
              new ConfigDescription(
                  "Override the time (in seconds) since last ping received for timeout and to close the connection.",
                  new AcceptableValueRange<float>(15f, 120f)));

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public delegate TResult OutFunc<in T1, T2, out TResult>(T1 arg1, out T2 arg2);

    public delegate TResult OutFunc<in T1, in T2, in T3, in T4, T5, out TResult>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, out T5 arg5);

    class SocketWrapper {
      static OutFunc<HSteamNetConnection, IntPtr, uint, int, long, EResult> _sendFunc;
      static Func<HSteamNetConnection, IntPtr[], int, int> _receiveFunc;
      static OutFunc<HSteamNetConnection, SteamNetworkingQuickConnectionStatus, bool> _connectionStatusFunc;

      public static SocketWrapper Create(ZSteamSocket socket) {
        _receiveFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection
                : SteamNetworkingSockets.ReceiveMessagesOnConnection;

        _sendFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.SendMessageToConnection
                : SteamNetworkingSockets.SendMessageToConnection;

        _connectionStatusFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.GetQuickConnectionStatus
                : SteamNetworkingSockets.GetQuickConnectionStatus;

        return new SocketWrapper(socket);
      }

      public readonly IntPtr[] MessagePtr = new IntPtr[1];
      public readonly ZSteamSocket Socket;

      SocketWrapper(ZSteamSocket socket) {
        Socket = socket;
      }

      public bool TryReceiveMessage(ref ZSteamSocket socket, out ZPackage package) {
        if (socket.m_con == HSteamNetConnection.Invalid || _receiveFunc(socket.m_con, MessagePtr, 1) != 1) {
          package = null;
          return false;
        }

        SteamNetworkingMessage_t messageT = Marshal.PtrToStructure<SteamNetworkingMessage_t>(MessagePtr[0]);

        package = new();
        package.m_stream.SetLength(messageT.m_cbSize);

        Marshal.Copy(messageT.m_pData, package.m_stream.GetBuffer(), 0, messageT.m_cbSize);

        socket.m_totalRecv += messageT.m_cbSize;
        socket.m_gotData = true;

        messageT.m_pfnRelease = MessagePtr[0];
        messageT.Release();

        return true;
      }

      public readonly ConcurrentQueue<byte[]> SendQueue = new();
      public int SendQueueSize = 0;

      public bool SendPackage(byte[] data, int length) {
        if (!Socket.IsConnected()) {
          return false;
        }

        IntPtr messageOutPtr = Marshal.AllocHGlobal(length);
        Marshal.Copy(data, 0, messageOutPtr, length);

        EResult result = _sendFunc(Socket.m_con, messageOutPtr, (uint)length, 8, out _);
        Marshal.FreeHGlobal(messageOutPtr);

        if (result != EResult.k_EResultOK) {
          return false;
        }

        Socket.m_totalSent += length;
        return true;
      }

      public void Enqueue(byte[] data) {
        lock (SendQueue) {
          SendQueue.Enqueue(data);
          SendQueueSize += data.Length;
        }
      }

      public void Dequeue() {
        lock (SendQueue) {
          if (SendQueue.TryDequeue(out byte[] data)) {
            SendQueueSize -= data.Length;
          }
        }
      }

      public int GetSendQueueSize() {
        if (!Socket.IsConnected()) {
          return 0;
        }

        int queueSize = SendQueueSize;

        lock (Socket.m_sendQueue) {
          foreach (byte[] data in Socket.m_sendQueue) {
            queueSize += data.Length;
          }
        }

        if (_connectionStatusFunc(Socket.m_con, out SteamNetworkingQuickConnectionStatus status)) {
          queueSize += status.m_cbPendingReliable + status.m_cbPendingUnreliable + status.m_cbSentUnackedReliable;
        }

        return queueSize;
      }
    }

    static CancellationTokenSource _cancellationTokenSource;

    static void SendQueuedPackagesLoop(CancellationToken cancellationToken) {
      ZLog.Log("Starting SendQueuedPackagesLoop...");

      while (!_cancellationTokenSource.IsCancellationRequested) {
        if (TryGetNextSocketWrapper(out SocketWrapper wrapper)) {
          while (wrapper.SendQueue.TryPeek(out byte[] data) && wrapper.SendPackage(data, data.Length)) {
            wrapper.Dequeue();
          }
        }

        Thread.Yield();
      }

      ZLog.Log("Stopping SendQueuedPackagesLoop...");
    }

    [HarmonyPatch(typeof(ZNet))]
    class ZNetPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZNet.Start))]
      static void StartPostfix(ref ZNet __instance) {
        _cancellationTokenSource = new();
        _cacheEnumerator = _socketWrapperCache.GetEnumerator();

        new Thread(() => SendQueuedPackagesLoop(_cancellationTokenSource.Token)).Start();
        new Thread(() => SendQueuedPackagesLoop(_cancellationTokenSource.Token)).Start();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZNet.StopAll))]
      static void StopAllPostfix(ref ZNet __instance) {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
      }
    }

    static readonly ConcurrentDictionary<ZSteamSocket, SocketWrapper> _socketWrapperCache = new();
    static IEnumerator<KeyValuePair<ZSteamSocket, SocketWrapper>> _cacheEnumerator;

    static bool TryGetNextSocketWrapper(out SocketWrapper wrapper) {
      lock (_cacheEnumerator) {
        if (_cacheEnumerator.MoveNext()) {
          wrapper = _cacheEnumerator.Current.Value;
          return true;
        }

        _cacheEnumerator = _socketWrapperCache.GetEnumerator();

        wrapper = default;
        return false;
      }
    }

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Recv))]
      static bool RecvPrefix(ref ZSteamSocket __instance, ref ZPackage __result) {
        _socketWrapperCache
            .GetOrAdd(__instance, SocketWrapper.Create)
            .TryReceiveMessage(ref __instance, out __result);

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Send))]
      static bool SendPrefix(ref ZSteamSocket __instance, ref ZPackage pkg) {
        if (pkg.Size() == 0 || !__instance.IsConnected()) {
          return false;
        }

        if (_socketWrapperCache.TryGetValue(__instance, out SocketWrapper socket)) {
          socket.Enqueue(pkg.GetArray());
        } else {
          lock (__instance.m_sendQueue) {
            __instance.m_sendQueue.Enqueue(pkg.GetArray());
          }
        }

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.SendQueuedPackages))]
      static bool SendQueuedPackagesPrefix(ref ZSteamSocket __instance) {
        SocketWrapper socket = _socketWrapperCache.GetOrAdd(__instance, SocketWrapper.Create);

        lock (__instance.m_sendQueue) {
          for (int i = 0, count = __instance.m_sendQueue.Count; i < count; i++) {
            socket.Enqueue(__instance.m_sendQueue.Dequeue());
          }
        }

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.GetSendQueueSize))]
      static bool GetSendQueueSizePrefix(ref ZSteamSocket __instance, ref int __result) {
        if (_socketWrapperCache.TryGetValue(__instance, out SocketWrapper wrapper)) {
          __result = wrapper.GetSendQueueSize();
          return false;
        }

        return true;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZSteamSocket.OnNewConnection))]
      static void OnNewConnectionPostfix(ref ZSteamSocket __instance) {
        _socketWrapperCache.AddOrUpdate(
            __instance, SocketWrapper.Create, (socket, wrapper) => SocketWrapper.Create(socket));
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Close))]
      static void ClosePrefix(ref ZSteamSocket __instance) {
        if (_socketWrapperCache.TryRemove(__instance, out SocketWrapper wrapper)) {
          ZLog.Log($"Closing socket: {__instance.GetEndPointString()}, SendQueueSize: {wrapper.GetSendQueueSize()} B");
        }
      }
    }

    static readonly byte[] _pingRequest = { 0x00, 0x00, 0x00, 0x00, 0x01 };
    static readonly byte[] _pingResponse = { 0x00, 0x00, 0x00, 0x00, 0x00 };

    [HarmonyPatch(typeof(ZRpc))]
    class ZRpcPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRpc.UpdatePing))]
      static bool UpdatePingPrefix(ref ZRpc __instance, ref float dt) {
        if (!_useCustomPrecomputedPings.Value) {
          return true;
        }

        __instance.m_pingTimer += dt;

        if (__instance.m_pingTimer > _customPingInterval.Value) {
          __instance.m_pingTimer = 0f;

          __instance.m_sentPackages++;
          __instance.m_sentData += _pingRequest.Length;

          SocketWrapper wrapper = _socketWrapperCache.GetOrAdd((ZSteamSocket)__instance.m_socket, SocketWrapper.Create);
          wrapper.Enqueue(_pingRequest);
        }

        __instance.m_timeSinceLastPing += dt;

        if (__instance.m_timeSinceLastPing > _customPingTimeout.Value) {
          ZLog.LogWarning("ZRpc timeout detected!");
          __instance.m_socket.Close();
        }

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRpc.ReceivePing))]
      static bool ReceivePingPrefix(ref ZRpc __instance, ref ZPackage package) {
        if (!_useCustomPrecomputedPings.Value) {
          return true;
        }

        if (package.ReadBool()) {
          __instance.m_sentPackages++;
          __instance.m_sentData += _pingResponse.Length;

          SocketWrapper wrapper = _socketWrapperCache.GetOrAdd((ZSteamSocket)__instance.m_socket, SocketWrapper.Create);
          wrapper.Enqueue(_pingResponse);
        } else {
          __instance.m_timeSinceLastPing = 0f;
        }

        return false;
      }
    }
  }
}