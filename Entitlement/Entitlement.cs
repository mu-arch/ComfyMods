using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Entitlement.PluginConfig;

namespace Entitlement {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Entitlement : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.entitlement";
    public const string PluginName = "Entitlement";
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