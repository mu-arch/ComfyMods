using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace BetterZeeDeeOhs {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterZeeDeeOhs : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeedeeohs";
    public const string PluginName = "BetterZeeDeeOhs";
    public const string PluginVersion = "1.0.0";

    static ManualLogSource _logger;

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

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
        _zdoPeerByNetPeerCache.TryRemove(netPeer, out _);
        _zdoPeerByRpcCache.TryRemove(netPeer.m_rpc, out _);
        _zdoPeerByUidCache.TryRemove(netPeer.m_uid, out _);
      }
    }
  }
}