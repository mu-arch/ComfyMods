using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System.Reflection;

namespace BetterZeeRouter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeRouter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeerouter";
    public const string PluginName = "BetterZeeRouter";
    public const string PluginVersion = "1.1.0";

    static ConfigEntry<bool> _isModEnabled;
    Harmony _harmony;

    public void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);

        _routedRpcManager.AddHandler(_rpcWntHealthChangedHashCode, _wntHealthChangedHandler);
        _routedRpcManager.AddHandler(_rpcDamageTextHashCode, _damageTextHandler);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly int _rpcWntHealthChangedHashCode = "WNTHealthChanged".GetStableHashCode();
    static readonly int _rpcDamageTextHashCode = "DamageText".GetStableHashCode();

    static readonly RoutedRpcManager _routedRpcManager = RoutedRpcManager.Instance;
    static readonly WntHealthChangedHandler _wntHealthChangedHandler = new();
    static readonly DamageTextHandler _damageTextHandler = new();

    class WntHealthChangedHandler : RpcMethodHandler {
      public long WntHealthChangedCount { get; private set; }

      public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
        WntHealthChangedCount++;
        return false;
      }
    }

    class DamageTextHandler : RpcMethodHandler {
      public long DamageTextCount { get; private set; }

      public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
        DamageTextCount++;
        return false;
      }
    }

    [HarmonyPatch(typeof(ZRoutedRpc))]
    class ZRoutedRpcPatch {
      static readonly ZRoutedRpc.RoutedRPCData _routedRpcData = new();

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.AddPeer))]
      static void AddPeerPostfix() {
        ZLog.Log($"WNTHealthChanged count: {_wntHealthChangedHandler.WntHealthChangedCount}");
        ZLog.Log($"DamageText count: {_damageTextHandler.DamageTextCount}");
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZRoutedRpc.RemovePeer))]
      static void RemovePeerPostfix() {
        ZLog.Log($"WNTHealthChanged count: {_wntHealthChangedHandler.WntHealthChangedCount}");
        ZLog.Log($"DamageText count: {_damageTextHandler.DamageTextCount}");
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRoutedRpc.RPC_RoutedRPC))]
      static bool RPC_RoutedRPCPrefix(ref ZRoutedRpc __instance, ref ZRpc rpc, ref ZPackage pkg) {
        _routedRpcData.DeserializeFrom(ref pkg);

        if (_routedRpcData.m_targetPeerID == __instance.m_id || _routedRpcData.m_targetPeerID == 0L) {
          __instance.HandleRoutedRPC(_routedRpcData);
        }

        if (!__instance.m_server || _routedRpcData.m_targetPeerID == __instance.m_id) {
          return false;
        }

        if (_routedRpcManager.Process(_routedRpcData)) {
          __instance.RouteRPC(_routedRpcData);
        }

        return false;
      }
    }
  }
}