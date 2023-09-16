namespace Keysential {
  public static class StopKeyManagerCommand {
    public static Terminal.ConsoleCommand Register() {
      return new Terminal.ConsoleCommand(
          "stopkeymanager",
          "stopkeymanager <id: id1>",
          args => Run(args));
    }

    public static bool Run(Terminal.ConsoleEventArgs args) {
      if (args.Length < 2) {
        Keysential.LogError($"Not enough args for stopkeymanager command.");
        return false;
      }

      return GlobalKeysManager.StopKeyManager(args[1]);
    }
  }
}
