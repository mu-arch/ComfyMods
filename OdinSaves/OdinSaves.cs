using System;
using System.Globalization;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using static OdinSaves.PluginConfig;

namespace OdinSaves {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class OdinSaves : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.odinsaves";
    public const string PluginName = "OdinSaves";
    public const string PluginVersion = "1.4.0";

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

    public static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}
