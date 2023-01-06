using System.Reflection;

using BepInEx;

using HarmonyLib;

using static YellowPages.PluginConfig;

namespace YellowPages {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class YellowPages : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.yellowpages";
    public const string PluginName = "YellowPages";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}