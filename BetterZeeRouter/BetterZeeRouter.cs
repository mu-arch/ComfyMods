using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System.Reflection;

namespace BetterZeeRouter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeRouter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeerouter";
    public const string PluginName = "BetterZeeRouter";
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

    static readonly int _wntHealthChangedHashCode = "WNTHealthChanged".GetStableHashCode();
    static readonly int _damageTextHashCode = "DamageText".GetStableHashCode();

    static long _wntHealthChangedCount = 0L;
    static long _damageTextCount = 0L;

    [HarmonyPatch(typeof(ZRoutedRpc))]
    class ZRoutedRpcPatch {
      static readonly ZRoutedRpc.RoutedRPCData _routedRpcData = new();

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.AddPeer))]
      static void AddPeerPostfix() {
        ZLog.Log($"WNTHealthChanged count: {_wntHealthChangedCount}");
        ZLog.Log($"DamageText count: {_damageTextCount}");
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.RemovePeer))]
      static void RemovePeerPostfix() {
        ZLog.Log($"WNTHealthChanged count: {_wntHealthChangedCount}");
        ZLog.Log($"DamageText count: {_damageTextCount}");
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRoutedRpc.RPC_RoutedRPC))]
      static bool RPC_RoutedRPCPrefix(ref ZRoutedRpc __instance, ref ZRpc rpc, ref ZPackage pkg) {
        _routedRpcData.Clear();
        _routedRpcData.Deserialize(ref pkg);

        if (_routedRpcData.m_targetPeerID == __instance.m_id || _routedRpcData.m_targetPeerID == 0L) {
          __instance.HandleRoutedRPC(_routedRpcData);
        }

        if (__instance.m_server && _routedRpcData.m_targetPeerID != __instance.m_id) {
          if (_routedRpcData.m_methodHash == _wntHealthChangedHashCode) {
            _wntHealthChangedCount++;
            return false;
          }

          if (_routedRpcData.m_methodHash == _damageTextHashCode) {
            _damageTextCount++;
            return false;
          }

          __instance.RouteRPC(_routedRpcData);
        }

        return false;
      }
    }
  }

  public static class RoutedRpcDataExtensions {
    public static void Deserialize(this ZRoutedRpc.RoutedRPCData routedRpcData, ref ZPackage package) {
      routedRpcData.m_msgID = package.ReadLong();
      routedRpcData.m_senderPeerID = package.ReadLong();
      routedRpcData.m_targetPeerID = package.ReadLong();
      routedRpcData.m_targetZDO = package.ReadZDOID();
      routedRpcData.m_methodHash = package.ReadInt();

      package.ReadPackage(ref routedRpcData.m_parameters);
    }

    public static void Clear(this ZRoutedRpc.RoutedRPCData routedRpcData) {
      routedRpcData.m_msgID = default;
      routedRpcData.m_senderPeerID = default;
      routedRpcData.m_targetPeerID = default;
      routedRpcData.m_targetZDO = default;
      routedRpcData.m_methodHash = default;
      routedRpcData.m_parameters?.Clear();
    }
  }
}