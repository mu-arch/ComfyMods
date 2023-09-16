using HarmonyLib;

namespace Keysential.Patches {
  [HarmonyPatch(typeof(Terminal))]
  static class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.Awake))]
    static void AwakePostfix() {
      StartVendorKeyManagerCommand.Register();
      StartKeyManagerCommand.Register();
      StopKeyManagerCommand.Register();
    }
  }
}
