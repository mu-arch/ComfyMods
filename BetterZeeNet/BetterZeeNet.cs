using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using Steamworks;

using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BetterZeeNet {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeNet : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeenet";
    public const string PluginName = "BetterZeeNet";
    public const string PluginVersion = "1.0.0";

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

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      static readonly IntPtr[] _messagePtr = new IntPtr[1];
      static readonly ZPackage _package = new();

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.Recv))]
      static bool RecvPrefix(ref ZSteamSocket __instance, ref ZPackage __result) {
        if (!__instance.IsConnected()) {
          __result = null;
          return false;
        }

        if (SteamNetworkingSockets.ReceiveMessagesOnConnection(__instance.m_con, _messagePtr, 1) != 1) {
          __result = null;
          return false;
        }

        SteamNetworkingMessage_t message = Marshal.PtrToStructure<SteamNetworkingMessage_t>(_messagePtr[0]);

        _package.m_writer.Flush();
        _package.m_stream.SetLength(message.m_cbSize);
        _package.m_stream.Position = 0L;

        Marshal.Copy(message.m_pData, _package.m_stream.GetBuffer(), 0, message.m_cbSize);

        message.m_pfnRelease = _messagePtr[0];
        message.Release();

        __instance.m_totalRecv += _package.Size();
        __instance.m_gotData = true;

        __result = _package;
        return false;
      }
    }
  }
}