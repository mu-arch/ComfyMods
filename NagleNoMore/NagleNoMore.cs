using BepInEx;

using HarmonyLib;

using Steamworks;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace NagleNoMore {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class NagleNoMore : BaseUnityPlugin {
    public const string PluginGUID = "valheim.mod.template";
    public const string PluginName = "NagleNoMore";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZSteamSocket.SendQueuedPackages))]
      static bool SendQueuedPackagesPrefix(ref ZSteamSocket __instance) {
        if (__instance.m_con == HSteamNetConnection.Invalid) {
          return false;
        }

        while (__instance.m_sendQueue.Count > 0) {
          byte[] array = __instance.m_sendQueue.Peek();

          IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
          Marshal.Copy(source: array, startIndex: 0, destination: intPtr, length: array.Length);

          // nSendFlags is: k_nSteamNetworkingSend_NoNagle = 1 | k_nSteamNetworkingSend_Reliable = 8
          EResult result =
              SteamNetworkingSockets.SendMessageToConnection(
                  __instance.m_con, intPtr, (uint) array.Length, nSendFlags: 9, out _);

          Marshal.FreeHGlobal(intPtr);

          switch (result) {
            case EResult.k_EResultOK:
              break;

            case EResult.k_EResultInvalidParam:
            case EResult.k_EResultLimitExceeded:
            case EResult.k_EResultIgnored:
              __instance.m_sendQueue.Dequeue();
              return false;

            case EResult.k_EResultNoConnection:
            case EResult.k_EResultInvalidState:
              __instance.Dispose();
              return false;

            default:
              __instance.Dispose();
              return false;
          }

          __instance.m_totalSent += array.Length;
          __instance.m_sendQueue.Dequeue();
        }

        return false;
      }
    }
  }
}