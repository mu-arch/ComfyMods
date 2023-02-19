using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Inventorious.PluginConfig;

namespace Inventorious {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Inventorious : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.inventorious";
    public const string PluginName = "Inventorious";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}