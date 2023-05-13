using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static BetterServerPortals.PluginConfig;

namespace BetterServerPortals {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterServerPortals : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterserverportals";
    public const string PluginName = "BetterServerPortals";
    public const string PluginVersion = "1.2.0";

    static ManualLogSource _logger;

    Harmony _harmony;

    void Awake() {
      BindConfig(Config);

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static HashSet<ZDO> CachedPortalZdos { get; private set; } = new(capacity: 512);

    static readonly KeyValuePair<int, int> _targetZdoidHashPair = ZDO.GetHashZDOID("target");
    static readonly int _targetUHashCode = "target_u".GetStableHashCode();
    static readonly int _targetIHashCode = "target_i".GetStableHashCode();
    static readonly int _tagHashCode = "tag".GetStableHashCode();

    public static IEnumerator ConnectPortalsCoroutine(Game game) {
      LogInfo("Starting ConnectPortals coroutine with cache...");

      ZDOID targetZdoid = ZDOID.None;
      List<ZDO> zdosToForceSend = new();
      Dictionary<string, ZDO> zdoByTagCache = new();

      WaitForSeconds waitInterval = new(ConnectPortalCoroutineWait.Value);
      ZDOMan zdoMan = ZDOMan.instance;
      Stopwatch stopwatch = Stopwatch.StartNew();

      while (true) {
        zdosToForceSend.Clear();
        zdoByTagCache.Clear();

        game.m_tempPortalList.Clear();
        game.m_tempPortalList.AddRange(CachedPortalZdos);

        foreach (ZDO zdo in game.m_tempPortalList) {
          string portalTag = zdo.GetString(_tagHashCode, string.Empty);

          targetZdoid.m_userID = zdo.GetLong(_targetUHashCode);
          targetZdoid.m_id = (uint) zdo.GetLong(_targetIHashCode);

          if (targetZdoid.m_userID == 0L || targetZdoid.m_id == 0U) {
            if (portalTag != string.Empty) {
              zdoByTagCache[portalTag] = zdo;
            }

            continue;
          }

          if (portalTag == string.Empty
              || !zdoMan.m_objectsByID.TryGetValue(targetZdoid, out ZDO targetZdo)
              || targetZdo.GetString(_tagHashCode, string.Empty) != portalTag ) {
            zdo.SetOwner(zdoMan.m_myid);
            zdo.Set(_targetUHashCode, 0L);
            zdo.Set(_targetIHashCode, 0L);

            zdosToForceSend.Add(zdo);

            if (portalTag != string.Empty) {
              zdoByTagCache[portalTag] = zdo;
            }
          }
        }

        foreach (ZDO zdo in game.m_tempPortalList) {
          string portalTag = zdo.GetString(_tagHashCode, string.Empty);

          targetZdoid.m_userID = zdo.GetLong(_targetUHashCode);
          targetZdoid.m_id = (uint) zdo.GetLong(_targetIHashCode);

          if (portalTag == string.Empty || !(targetZdoid.m_userID == 0L || targetZdoid.m_id == 0L)) {
            continue;
          }

          if (!zdoByTagCache.TryGetValue(portalTag, out ZDO targetZdo) || targetZdo == zdo) {
            continue;
          }

          targetZdoid.m_userID = targetZdo.GetLong(_targetUHashCode);
          targetZdoid.m_id = (uint) targetZdo.GetLong(_targetIHashCode);

          if (targetZdoid.m_userID == 0L || targetZdoid.m_id == 0L) {
            zdo.SetOwner(zdoMan.m_myid);
            zdo.Set(_targetZdoidHashPair, targetZdo.m_uid);

            targetZdo.SetOwner(zdoMan.m_myid);
            targetZdo.Set(_targetZdoidHashPair, zdo.m_uid);

            zdosToForceSend.Add(zdo);
            zdosToForceSend.Add(targetZdo);
          }
        }

        foreach (ZDO zdo in zdosToForceSend) {
          zdoMan.ForceSendZDO(zdo.m_uid);
        }

        if (stopwatch.ElapsedMilliseconds > 60000L) {
          LogInfo($"Processed {CachedPortalZdos.Count} portals.");
          stopwatch.Restart();
        }

        yield return waitInterval;
      }
    }

    static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}