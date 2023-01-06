using System;
using System.Globalization;

using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

namespace BetterZeeLog {
  [HarmonyPatch(typeof(ZLog))]
  static class ZLogPatch {
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
