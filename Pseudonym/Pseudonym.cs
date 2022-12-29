using System.Reflection;

using BepInEx;

using HarmonyLib;

namespace Pseudonym {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Pseudonym : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.pseudonym";
    public const string PluginName = "Pseudonym";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }


}