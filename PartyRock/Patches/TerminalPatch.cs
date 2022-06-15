using HarmonyLib;

namespace PartyRock {
  [HarmonyPatch(typeof(Terminal))]
  public class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    static void InitTerminalPostfix() {
      new Terminal.ConsoleCommand(
          "playerlist",
          "PartyRock: toggle the PlayerListPanel.",
          args => PartyRock.TogglePlayerListPanel());

      new Terminal.ConsoleCommand(
          "cardpanel",
          "PartyRock: toggle the CardPanel",
          args => PartyRock.ToggleCardPanel());
    }
  }
}
