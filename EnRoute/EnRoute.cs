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
    public const string PluginVersion = "1.1.0";

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
  }
}
