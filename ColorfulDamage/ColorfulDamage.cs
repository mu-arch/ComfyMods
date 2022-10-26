using System.Reflection;

using BepInEx;

using HarmonyLib;

namespace ColorfulDamage {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class ColorfulDamage : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.colorfuldamage";
    public const string PluginName = "ColorfulDamage";
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