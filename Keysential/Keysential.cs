using System.Collections.Generic;
using System.Reflection;

using BepInEx;

using HarmonyLib;

using static Keysential.PluginConfig;

namespace Keysential {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Keysential : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.keysential";
    public const string PluginName = "Keysential";
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