using System.Reflection;

using BepInEx;

using HarmonyLib;

using static ComfySigns.PluginConfig;

namespace ComfySigns {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class ComfySigns : BaseUnityPlugin {
    public const string PluginGuid = "comfy.valheim.modname";
    public const string PluginName = "ComfySigns";
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