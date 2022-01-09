using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace BetterZeeLog {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterZeeLog : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeelog";
    public const string PluginName = "BetterZeeLog";
    public const string PluginVersion = "1.2.0";

    static ConfigEntry<bool> _isModEnabled;
    static ConfigEntry<bool> _removeStackTraceForNonErrorLogType;

    Harmony _harmony;
      
    void Awake() {
      _isModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      _removeStackTraceForNonErrorLogType =
          Config.Bind(
              "Logging",
              "removeStackTraceForNonErrorLogType",
              true,
              "Disables the stack track for 'Info' and 'Warning' log types (restart required).");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        if (_removeStackTraceForNonErrorLogType.Value) {
          Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
          Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }
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

    [HarmonyPatch(typeof(ZSteamSocket))]
    class ZSteamSocketPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZSteamSocket.SendQueuedPackages))]
      static IEnumerable<CodeInstruction> SendQueuedPackagesTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldstr),
                new CodeMatch(OpCodes.Ldloc_3),
                new CodeMatch(OpCodes.Box),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Call))
            .Advance(offset: 1)
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Pop))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
            .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Nop))
            .InstructionEnumeration();
      }
    }
  }
}