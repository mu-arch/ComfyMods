using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(Terminal))]
  static class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.Awake))]
    static void AwakePostfix() {
      SetWorldTimeCommand.Register();
    }
  }
}
