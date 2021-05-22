using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace OdinSaves {
  [BepInPlugin(OdinSaves.Package, OdinSaves.ModName, OdinSaves.Version)]
  public class OdinSaves : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.odinsaves";
    public const string Version = "0.0.2";
    public const string ModName = "Odin Saves";

    private static ConfigEntry<bool> _isModEnabled;
    private static ConfigEntry<int> savePlayerProfileInterval;
    private static ConfigEntry<bool> setLogoutPointOnSave;
    private static ConfigEntry<bool> showMessageOnModSave;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind("Global", "isModEnabled", true, "Whether the mod should be enabled.");

      savePlayerProfileInterval = Config.Bind(
        "Global",
        "savePlayerProfileInterval",
        300,
        "Interval (in seconds) for how often to save the player profile. Game default (and maximum) is 1200s.");

      setLogoutPointOnSave = Config.Bind(
        "Global",
        "setLogoutPointOnSave",
        true,
        "Sets your logout point to your current position when the mod performs a save.");

      showMessageOnModSave = Config.Bind(
        "Global",
        "saveMessageOnModSave",
        true,
        "Show a message (in the middle of your screen) when the mod tries to save.");

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
      private static void UpdateSavingPostfix(ref Game __instance) {
        if (!_isModEnabled.Value) {
          return;
        }

        if (__instance.m_saveTimer == 0f || __instance.m_saveTimer < savePlayerProfileInterval.Value) {
          return;
        }

        if (showMessageOnModSave.Value) {
          MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Saving player profile...");
        }

        __instance.m_saveTimer = 0f;
        __instance.SavePlayerProfile(/*setLogoutPoint=*/ setLogoutPointOnSave.Value);

        if (ZNet.instance) {
          ZNet.instance.Save(/*sync=*/ false);
        }
      }
    }

    [HarmonyPatch(typeof(Player))]
    private class PlayerPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.OnDeath))]
      private static void PlayerOnDeathPostfix(ref Player __instance) {
        Game.instance.m_playerProfile.ClearLoguoutPoint();
      }
    }
  }
}
