using System.Reflection;

using BepInEx;

using HarmonyLib;

using static LicenseToSkill.PluginConfig;

namespace LicenseToSkill {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class LicenseToSkill : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.comfytools.licensetoskill";
    public const string PluginName = "LicenseToSkill";
    public const string PluginVersion = "1.2.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}