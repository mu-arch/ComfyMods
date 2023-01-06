﻿using BepInEx.Configuration;

namespace BetterZeeLog {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }
    public static ConfigEntry<bool> RemoveStackTraceForNonErrorLogType { get; private set; }
    public static ConfigEntry<bool> RemoveFailedToSendDataLogging { get; private set; }

    public static void BindConfig(ConfigFile Config) {
      IsModEnabled =
          Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      RemoveStackTraceForNonErrorLogType =
          Config.Bind(
              "Logging",
              "removeStackTraceForNonErrorLogType",
              true,
              "Disables the stack track for 'Info' and 'Warning' log types (restart required).");

      RemoveFailedToSendDataLogging =
          Config.Bind(
              "Logging",
              "removeFailedToSendDataLogging",
              true,
              "Removes (NOPs out) 'Failed to send data' logging in ZSteamSocket (restart required).");
    }
  }
}
