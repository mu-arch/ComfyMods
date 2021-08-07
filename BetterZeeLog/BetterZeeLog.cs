using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace BetterZeeLog {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class NewMod : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeelog";
    public const string PluginName = "BetterZeeLog";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;

    Harmony _harmony;

    void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
      }
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZLog))]
    class ZLogPatch {
      static readonly bool _isDebugBuild = Debug.isDebugBuild;
      static readonly ManualLogSource _zLog = BepInEx.Logging.Logger.CreateLogSource("ZLog");

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZLog.Log))]
      static bool LogPrefix(ref object o) {
        _zLog.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZLog.LogWarning))]
      static bool LogWarningPrefix(ref object o) {
        _zLog.LogWarning($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZLog.LogError))]
      static bool LogErrorPrefix(ref object o) {
        _zLog.LogError($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
        return false;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZLog.DevLog))]
      static bool DevLogPrefix(ref object o) {
        if (_isDebugBuild) {
          _zLog.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
        }

        return false;
      }
    }
  }
}