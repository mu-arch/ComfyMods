using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace BetterZeeDeeOhs {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class NewMod : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeedeeohs";
    public const string PluginName = "BetterZeeDeeOhs";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}