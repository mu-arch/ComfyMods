using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;

using HarmonyLib;

namespace EnRoute {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class EnRoute : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.enroute";
    public const string PluginName = "EnRoute";
    public const string PluginVersion = "1.0.1";

    Harmony _harmony;

    void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static readonly HashSet<int> NearbyMethodHashCodes = new() {
      "Step".GetStableHashCode(),
      "DestroyZDO".GetStableHashCode(),
    };

    public static readonly HashSet<long> NearbyUserIds = new();

    public static long RouteToNearbyCount = 0L;
    public static long RouteToServerCount = 0L;

    public static void LogStats(TimeSpan timeElapsed) {
      ZLog.Log($"RouteToNearby: {RouteToNearbyCount}, RouteToServer: {RouteToServerCount}, Elapsed: {timeElapsed}");
    }

    public static void RefreshNearbyPlayers() {
      NearbyUserIds.Clear();

      ZoneSystem zoneSystem = ZoneSystem.m_instance;
      ZDOID playerCharacterId = ZNet.m_instance.m_characterID;
      Vector2i playerZone = zoneSystem.GetZone(ZNet.m_instance.m_referencePosition);

      foreach (ZNet.PlayerInfo playerInfo in ZNet.m_instance.m_players) {
        ZDOID characterId = playerInfo.m_characterID;

        if (characterId.IsNone() || characterId == playerCharacterId) {
          continue;
        }

        if (!playerInfo.m_publicPosition
            || Vector2i.Distance(playerZone, zoneSystem.GetZone(playerInfo.m_position)) <= 2) {
          NearbyUserIds.Add(characterId.UserID);
        }
      }
    }
  }
}
