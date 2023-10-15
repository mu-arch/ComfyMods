using HarmonyLib;

using static Pinnacle.PluginConfig;
using static Pinnacle.PinImportExport;

namespace Pinnacle {
  [HarmonyPatch(typeof(Terminal))]
  static class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    static void InitTerminalPostfix() {
      new Terminal.ConsoleCommand(
          "pinnacle-exportpins-binary",
          "<filename> [name-filter-regex] -- export pins to a file in binary format.",
          args => ExportPinsToFile(args, PinFileFormat.Binary));

      new Terminal.ConsoleCommand(
          "pinnacle-exportpins-text",
          "<filename> [name-filter-regex] -- export pins to a file in plain text format.",
          args => ExportPinsToFile(args, PinFileFormat.PlainText));

      new Terminal.ConsoleCommand(
          "pinnacle-importpins-binary",
          "<filename> [name-filter-regex] -- import pins in binary format from file.",
          args => ImportPinsFromBinaryFile(args));

      new Terminal.ConsoleCommand(
          "pinnacle-removeallpins",
          "Pinnacle: removes ALL pins.",
          args => RemoveAllPins(args));

      new Terminal.ConsoleCommand(
          "pinnacle-namepindata-clearall",
          "PInnacle: clears all name pin data from the Minimap.",
          args => ClearAllNamePinData());

      if (IsModEnabled.Value) {
        ModifyResetMapCommand();
      }
    }

    static void ModifyResetMapCommand() {
      if (Terminal.commands.TryGetValue("resetmap", out Terminal.ConsoleCommand command)) {
        command.IsCheat = false;
        command.OnlyServer = false;

        Pinnacle.LogInfo($"Modified 'resetmap' command: IsCheat = false, OnlyServer = false.");
      }
    }

    static void RemoveAllPins(Terminal.ConsoleEventArgs args) {
      if (!Minimap.m_instance) {
        return;
      }

      int count = Minimap.m_instance.m_pins.RemoveAll(pin => pin.m_save);
      args.Context.AddString($"Removed {count} pins.");
    }

    static void ClearAllNamePinData() {
      Minimap minimap = Minimap.m_instance;

      if (!minimap) {
        return;
      }

      int count = minimap.m_pinNameRootLarge.childCount;
      Pinnacle.LogInfo($"Pinnacle: destroying {count} NamePinData GameObjects...");

      for (int i = count - 1; i >= 0; i--) {
        UnityEngine.Object.Destroy(minimap.m_pinNameRootLarge.GetChild(i).gameObject);
      }
    }
  }
}
