using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace OdinSaves {
  [BepInPlugin(OdinSaves.Package, OdinSaves.ModName, OdinSaves.Version)]
  public class OdinSaves : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.odinsaves";
    public const string Version = "0.0.1";
    public const string ModName = "Odin Saves";

    private static ConfigEntry<bool> isModEnabled;
    private static ConfigEntry<int> savePlayerProfileInterval;

    private Harmony _harmony;

    private void Awake() {
      isModEnabled = Config.Bind("Global", "isModEnabled", true, "Whether the mod should be enabled.");

      savePlayerProfileInterval = Config.Bind(
        "Global",
        "savePlayerProfileInterval",
        300,
        "Interval (in seconds) for how often to save the player profile. Game default (and maximum) is 1200s.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchAll(null);
      }
    }

    [HarmonyPatch(typeof(Game))]
    private class GamePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Game.UpdateSaving))]
      private static void UpdateSavingPostfix(Game __instance) {
        if (!isModEnabled.Value) {
          return;
        }

        if (__instance.m_saveTimer == 0f || __instance.m_saveTimer < savePlayerProfileInterval.Value) {
          return;
        }

        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Saving player profile...");

        __instance.m_saveTimer = 0f;
        __instance.SavePlayerProfile(/*setLogoutPoint=*/ false);

        if (ZNet.instance) {
          ZNet.instance.Save(/*sync=*/ false);
        }
      }
    }
  }
}
