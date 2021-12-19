using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Steamworks;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

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

    class SocketWrapper {
      static Func<HSteamNetConnection, IntPtr[], int, int> _receiveFunc;

      public static SocketWrapper Create(ZSteamSocket socket) {
        _receiveFunc ??=
            ZNet.m_isServer
                ? SteamGameServerNetworkingSockets.ReceiveMessagesOnConnection
                : SteamNetworkingSockets.ReceiveMessagesOnConnection;

        return new SocketWrapper();
      }

      public readonly IntPtr[] MessagePtr = new IntPtr[1];
      public readonly ZPackage Package = new();

      public bool TryReceiveMessage(ZSteamSocket socket) {
        if (socket.m_con == HSteamNetConnection.Invalid || _receiveFunc(socket.m_con, MessagePtr, 1) != 1) {
          return false;
        }

        SteamNetworkingMessage_t messageT = Marshal.PtrToStructure<SteamNetworkingMessage_t>(MessagePtr[0]);

        Package.m_writer.Flush();
        Package.m_stream.SetLength(messageT.m_cbSize);
        Package.m_stream.Position = 0L;

        Marshal.Copy(messageT.m_pData, Package.m_stream.GetBuffer(), 0, messageT.m_cbSize);

        messageT.m_pfnRelease = MessagePtr[0];
        messageT.Release();

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
        __result = socket.TryReceiveMessage(__instance) ? socket.Package : null;

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
    }
  }
}