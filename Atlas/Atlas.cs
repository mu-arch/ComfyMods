using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Atlas.PluginConfig;

namespace Atlas {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Atlas : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.atlas";
    public const string PluginName = "Atlas";
    public const string PluginVersion = "1.5.3";

    public static readonly int TimeCreatedHashCode = "timeCreated".GetStableHashCode();

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}