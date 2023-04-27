using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Effectual.PluginConfig;

namespace Effectual {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Effectual : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.effectual";
    public const string PluginName = "Effectual";
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