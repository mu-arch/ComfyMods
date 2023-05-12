using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace BetterServerPortals {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterServerPortals : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterserverportals";
    public const string PluginName = "BetterServerPortals";
    public const string PluginVersion = "1.2.0";

    static ConfigEntry<float> _connectPortalCoroutineWait;
    static ConfigEntry<string> _portalPrefabNames;

    static readonly KeyValuePair<int, int> _targetZdoidHashPair = ZDO.GetHashZDOID("target");
    static readonly int _targetUHashCode = "target_u".GetStableHashCode();
    static readonly int _targetIHashCode = "target_i".GetStableHashCode();
    static readonly int _tagHashCode = "tag".GetStableHashCode();

    static ManualLogSource _logger;

    static HashSet<int> _portalPrefabHashCodes;
    static WaitForSeconds _connectPortalWaitInterval;

    static readonly HashSet<ZDO> _cachedPortalZdos = new(capacity: 5000);
    static readonly Dictionary<string, ZDO> _zdoByTagCache = new();
    static readonly List<ZDO> _zdosToForceSend = new();
    static ZDOID _targetZdoid = ZDOID.None;

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      _connectPortalCoroutineWait =
          Config.Bind(
              "Portals", "connectPortalCoroutineWait", 5f, "Wait time (seconds) when ConnectPortal coroutine yields.");

      _connectPortalWaitInterval = new(seconds: _connectPortalCoroutineWait.Value);

      _portalPrefabNames =
          Config.Bind(
              "Portals",
              "portalPrefabNames",
              "portal_wood,portal",
              "Comma-separated list of portal prefab names to search for.");

      _portalPrefabHashCodes = CreateHashCodesSet(_portalPrefabNames.Value);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static HashSet<int> CreateHashCodesSet(string prefabsText) {
      return prefabsText
          .Split(',')
          .Select(p => p.Trim())
          .Where(p => !p.IsNullOrWhiteSpace())
          .Select(p => p.GetStableHashCode())
          .ToHashSet();
    }

    [HarmonyPatch(typeof(Game))]
    class GamePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Game.Start))]
      static void StartPostfix(ref Game __instance) {
        if (ZNet.m_isServer) {
          __instance.StopCoroutine(nameof(Game.ConnectPortals));
          __instance.StartCoroutine(ConnectPortalsCoroutine(__instance));
        }
      }
    }

    [HarmonyPatch(typeof(ZDOMan))]
    class ZDOManPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.AddToSector))]
      static void AddToSectorPostfix(ref ZDO zdo) {
        if (zdo != null && _portalPrefabHashCodes.Contains(zdo.m_prefab)) {
          _cachedPortalZdos.Add(zdo);
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
      static void RemoveFromSectorPostfix(ref ZDO zdo) {
        _cachedPortalZdos.Remove(zdo);
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
      static IEnumerable<CodeInstruction> RPC_ZDODataTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
          .MatchForward(useEnd: true, new CodeMatch(OpCodes.Callvirt, typeof(ZDO).GetMethod(nameof(ZDO.Deserialize))))
          .InsertAndAdvance(
              new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(13)),
              Transpilers.EmitDelegate<Action<ZDO>>(
                  zdo => {
                    if (_portalPrefabHashCodes.Contains(zdo.m_prefab)) {
                      _cachedPortalZdos.Add(zdo);
                    }
                  }))
          .InstructionEnumeration();
      }
    }

    static IEnumerator ConnectPortalsCoroutine(Game game) {
      ZDOMan zdoMan = ZDOMan.instance;
      Stopwatch stopwatch = Stopwatch.StartNew();

      while (true) {
        _zdosToForceSend.Clear();
        _zdoByTagCache.Clear();

        game.m_tempPortalList.Clear();
        game.m_tempPortalList.AddRange(_cachedPortalZdos);

        foreach (ZDO zdo in game.m_tempPortalList) {
          string portalTag = zdo.GetString(_tagHashCode, string.Empty);

          _targetZdoid.m_userID = zdo.GetLong(_targetUHashCode);
          _targetZdoid.m_id = (uint) zdo.GetLong(_targetIHashCode);

          if (_targetZdoid.m_userID == 0L || _targetZdoid.m_id == 0U) {
            if (portalTag != string.Empty) {
              _zdoByTagCache[portalTag] = zdo;
            }

            continue;
          }

          if (portalTag == string.Empty
              || !zdoMan.m_objectsByID.TryGetValue(_targetZdoid, out ZDO targetZdo)
              || targetZdo.GetString(_tagHashCode, string.Empty) != portalTag ) {
            zdo.SetOwner(zdoMan.m_myid);
            zdo.Set(_targetUHashCode, 0L);
            zdo.Set(_targetIHashCode, 0L);

            _zdosToForceSend.Add(zdo);

            if (portalTag != string.Empty) {
              _zdoByTagCache[portalTag] = zdo;
            }
          }
        }

        foreach (ZDO zdo in game.m_tempPortalList) {
          string portalTag = zdo.GetString(_tagHashCode, string.Empty);

          _targetZdoid.m_userID = zdo.GetLong(_targetUHashCode);
          _targetZdoid.m_id = (uint) zdo.GetLong(_targetIHashCode);

          if (portalTag == string.Empty || !(_targetZdoid.m_userID == 0L || _targetZdoid.m_id == 0L)) {
            continue;
          }

          if (!_zdoByTagCache.TryGetValue(portalTag, out ZDO targetZdo) || targetZdo == zdo) {
            continue;
          }

          _targetZdoid.m_userID = targetZdo.GetLong(_targetUHashCode);
          _targetZdoid.m_id = (uint) targetZdo.GetLong(_targetIHashCode);

          if (_targetZdoid.m_userID == 0L || _targetZdoid.m_id == 0L) {
            zdo.SetOwner(zdoMan.m_myid);
            zdo.Set(_targetZdoidHashPair, targetZdo.m_uid);

            targetZdo.SetOwner(zdoMan.m_myid);
            targetZdo.Set(_targetZdoidHashPair, zdo.m_uid);

            _zdosToForceSend.Add(zdo);
            _zdosToForceSend.Add(targetZdo);
          }
        }

        foreach (ZDO zdo in _zdosToForceSend) {
          zdoMan.ForceSendZDO(zdo.m_uid);
        }

        if (stopwatch.ElapsedMilliseconds > 60000L) {
          LogInfo($"Processed {_cachedPortalZdos.Count} portals.");
          stopwatch.Restart();
        }

        yield return _connectPortalWaitInterval;
      }
    }

    static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}