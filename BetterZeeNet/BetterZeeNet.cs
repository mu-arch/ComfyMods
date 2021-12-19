using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Steamworks;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace BetterZeeNet {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeNet : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeenet";
    public const string PluginName = "BetterZeeNet";
    public const string PluginVersion = "1.1.0";

    static ConfigEntry<bool> _isModEnabled;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public delegate TResult OutFunc<in T1, in T2, in T3, in T4, T5, out TResult>(
        T1 arg1, T2 arg2, T3 arg3, T4 arg4, out T5 arg5);

    class SocketWrapper {
      static OutFunc<HSteamNetConnection, IntPtr, uint, int, long, EResult> _sendFunc;
      static Func<HSteamNetConnection, IntPtr[], int, int> _receiveFunc;

      public static SocketWrapper Create(ZSteamSocket socket) {
        _receiveFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection
                : SteamNetworkingSockets.ReceiveMessagesOnConnection;

        _sendFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.SendMessageToConnection
                : SteamNetworkingSockets.SendMessageToConnection;

        return new SocketWrapper(socket);
      }

      public readonly IntPtr[] MessagePtr = new IntPtr[1];
      public readonly ZPackage Package = new();
      public readonly ZSteamSocket Socket;

      SocketWrapper(ZSteamSocket socket) {
        Socket = socket;
        new Thread(() => SendMessageLoop(Cancellation.Token)).Start();
      }

      public bool TryReceiveMessage(ref ZSteamSocket socket) {
        if (socket.m_con == HSteamNetConnection.Invalid || _receiveFunc(socket.m_con, MessagePtr, 1) != 1) {
          return false;
        }

        SteamNetworkingMessage_t messageT = Marshal.PtrToStructure<SteamNetworkingMessage_t>(MessagePtr[0]);

        Package.m_writer.Flush();
        Package.m_stream.SetLength(messageT.m_cbSize);
        Package.m_stream.Position = 0L;

        Marshal.Copy(messageT.m_pData, Package.m_stream.GetBuffer(), 0, messageT.m_cbSize);

        socket.m_totalRecv += messageT.m_cbSize;
        socket.m_gotData = true;

        messageT.m_pfnRelease = MessagePtr[0];
        messageT.Release();

        return true;
      }

      public readonly CancellationTokenSource Cancellation = new();
      public readonly ConcurrentQueue<byte[]> SendQueue = new();

      void SendMessageLoop(CancellationToken token) {
        while (!token.IsCancellationRequested) {
          if (SendQueue.TryPeek(out byte[] data) && SendPackage(data, data.Length)) {
            SendQueue.TryDequeue(out _);
          } else {
            Thread.Yield();
          }
        }
      }

      bool SendPackage(byte[] data, int length) {
        if (!Socket.IsConnected()) {
          return false;
        }

        IntPtr messageOutPtr = Marshal.AllocHGlobal(length);
        Marshal.Copy(data, 0, messageOutPtr, length);

        EResult result = _sendFunc(Socket.m_con, messageOutPtr, (uint)length, 8, out _);
        Marshal.FreeHGlobal(messageOutPtr);

        if (result != EResult.k_EResultOK) {
          ZLog.LogError($"Failed to send data: {result}");
          return false;
        }

        Socket.m_totalSent += length;
        return true;
      }
    }

    static readonly ConcurrentDictionary<ZSteamSocket, SocketWrapper> _socketWrapperCache = new();

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Recv))]
      static bool RecvPrefix(ref ZSteamSocket __instance, ref ZPackage __result) {
        SocketWrapper socket = _socketWrapperCache.GetOrAdd(__instance, SocketWrapper.Create);
        __result = socket.TryReceiveMessage(ref __instance) ? socket.Package : null;

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Send))]
      static bool SendPrefix(ref ZSteamSocket __instance, ref ZPackage pkg) {
        if (pkg.Size() == 0 || !__instance.IsConnected()) {
          return false;
        }

        __instance.m_sendQueue.Enqueue(pkg.GetArray());
        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.SendQueuedPackages))]
      static bool SendQueuedPackagesPrefix(ref ZSteamSocket __instance) {
        SocketWrapper socket = _socketWrapperCache.GetOrAdd(__instance, SocketWrapper.Create);

        for (int i = 0, count = __instance.m_sendQueue.Count; i < count;  i++) {
          socket.SendQueue.Enqueue(__instance.m_sendQueue.Dequeue());
        }

        return false;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZSteamSocket.OnNewConnection))]
      static void OnNewConnectionPostfix(ref ZSteamSocket __instance) {
        _socketWrapperCache.GetOrAdd(__instance, SocketWrapper.Create);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Close))]
      static void ClosePrefix(ref ZSteamSocket __instance) {
        if (_socketWrapperCache.TryRemove(__instance, out SocketWrapper socket)) {
          socket.Cancellation.Cancel();
          socket.Cancellation.Dispose();
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
        __instance.m_pingTimer += dt;

        if (__instance.m_pingTimer > ZRpc.m_pingInterval) {
          __instance.m_pingTimer = 0f;

          __instance.m_sentPackages++;
          __instance.m_sentData += _pingRequest.Length;
          ((ZSteamSocket)__instance.m_socket).m_sendQueue.Enqueue(_pingRequest);
        }

        __instance.m_timeSinceLastPing += dt;

        if (__instance.m_timeSinceLastPing > ZRpc.m_timeout) {
          ZLog.LogWarning("ZRpc timeout detected!");
          __instance.m_socket.Close();
        }

        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRpc.ReceivePing))]
      static bool ReceivePingPrefix(ref ZRpc __instance, ref ZPackage package) {
        if (package.ReadBool()) {
          __instance.m_sentPackages++;
          __instance.m_sentData += _pingResponse.Length;
          ((ZSteamSocket)__instance.m_socket).m_sendQueue.Enqueue(_pingResponse);
        } else {
          __instance.m_timeSinceLastPing = 0f;
        }

        return false;
      }
    }
  }
}