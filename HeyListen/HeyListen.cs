using System.Reflection;

using BepInEx;

using HarmonyLib;

namespace HeyListen {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class HeyListen : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.heylisten";
    public const string PluginName = "HeyListen";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      PluginConfig.BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}