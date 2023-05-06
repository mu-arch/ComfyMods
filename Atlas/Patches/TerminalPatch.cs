using System;
using System.Diagnostics;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(Terminal))]
  static class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    static void InitTerminalPostfix() {
      new Terminal.ConsoleCommand(
          "setworldtime",
          "setworldtime <time in seconds>",
          args => SetWorldTime(args));
    }

    static void SetWorldTime(Terminal.ConsoleEventArgs args) {
      if (args.Length < 2 || !double.TryParse(args[1], out double time) || time < 0f) {
        ZLog.LogError("Invalid or missing <time> argument.");
        return;
      }

      long offsetTicks = (long) ((time - ZNet.m_instance.m_netTime) * TimeSpan.TicksPerSecond);

      ZLog.Log($"Setting world time from {ZNet.m_instance.m_netTime} to: {time}.");
      ZLog.Log($"Offsetting all world ZDO.m_timeCreated values by: {offsetTicks}");

      Stopwatch stopwatch = Stopwatch.StartNew();
      ZNet.m_instance.m_netTime = time;

      foreach (ZDO zdo in ZDOMan.m_instance.m_objectsByID.Values) {
        zdo.m_timeCreated += offsetTicks;
      }

      ZLog.Log($"Finished setting world time in: {stopwatch.Elapsed}");
    }
  }
}
