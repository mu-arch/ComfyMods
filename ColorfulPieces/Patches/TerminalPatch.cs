using HarmonyLib;

using static ColorfulPieces.ColorfulPieces;
using static ColorfulPieces.PluginConfig;

namespace ColorfulPieces {
  [HarmonyPatch(typeof(Terminal))]
  class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    static void InitTerminalPostfix() {
      if (!IsModEnabled.Value) {
        return;
      }

      new Terminal.ConsoleCommand(
          "clearcolor",
          "ColorfulPieces: Clears all colors applied to all pieces within radius of player.",
          args => {
            if (!IsModEnabled.Value
                || args.Length < 2
                || !float.TryParse(args.Args[1], out float radius)
                || !Player.m_localPlayer) {
              return;
            }

            args.Context.StartCoroutine(
                ClearColorsInRadiusCoroutine(Player.m_localPlayer.transform.position, radius));
          });

      new Terminal.ConsoleCommand(
          "changecolor",
          "ColorfulPieces: Changes the color of all pieces within radius of player to the currently set color.",
          args => {
            if (!IsModEnabled.Value
                || args.Length < 2
                || !float.TryParse(args.Args[1], out float radius)
                || !Player.m_localPlayer) {
              return;
            }

            args.Context.StartCoroutine(
                ChangeColorsInRadiusCoroutine(Player.m_localPlayer.transform.position, radius));
          });
    }
  }
}
