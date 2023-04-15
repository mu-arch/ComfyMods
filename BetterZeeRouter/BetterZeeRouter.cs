using System.Reflection;

using BepInEx;

using HarmonyLib;

using static BetterZeeRouter.PluginConfig;
using static BetterZeeRouter.RpcHashCodes;

namespace BetterZeeRouter {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterZeeRouter : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterzeerouter";
    public const string PluginName = "BetterZeeRouter";
    public const string PluginVersion = "1.4.0";

    Harmony _harmony;
    TeleportToHandler _teleportToHandler;

    public void Awake() {
      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);

        _routedRpcManager.AddHandler(RpcWntHealthChangedHashCode, _wntHealthChangedHandler);
        _routedRpcManager.AddHandler(RpcDamageTextHashCode, _damageTextHandler);
        _routedRpcManager.AddHandler(RpcSetTargetHashCode, _setTargetHandler);

        _teleportToHandler = new("TeleportToLog.txt");
        _routedRpcManager.AddHandler(RpcTeleportToHashCode, _teleportToHandler);
      }
    }

    public void OnDestroy() {
      _teleportToHandler?.Dispose();
      _harmony?.UnpatchSelf();
    }

    static readonly RoutedRpcManager _routedRpcManager = RoutedRpcManager.Instance;
    static readonly WntHealthChangedHandler _wntHealthChangedHandler = new();
    static readonly DamageTextHandler _damageTextHandler = new();
    static readonly SetTargetHandler _setTargetHandler = new();

    [HarmonyPatch(typeof(ZRoutedRpc))]
    static class ZRoutedRpcPatch {
      static readonly ZRoutedRpc.RoutedRPCData _routedRpcData = new();

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