using System.Reflection;

using BepInEx;

using HarmonyLib;

namespace Atlas {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Atlas : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.atlas";
    public const string PluginName = "Atlas";
    public const string PluginVersion = "1.5.5";

    public static readonly int TimeCreatedHashCode = "timeCreated".GetStableHashCode();
    public static readonly int EpochTimeCreatedHashCode = "epochTimeCreated".GetStableHashCode();

    Harmony _harmony;

    void Awake() {
      PluginLogger.BindLogger(Logger);
      PluginConfig.BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}