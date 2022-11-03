using System.Reflection;

using BepInEx;

using HarmonyLib;

using static BetterBattleUI.PluginConfig;

namespace BetterBattleUI {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class BetterBattleUI : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.betterbattleui";
    public const string PluginName = "BetterBattleUI";
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