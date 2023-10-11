using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Recipedia : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.recipedia";
    public const string PluginName = "Recipedia";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}