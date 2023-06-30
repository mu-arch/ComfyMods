using HarmonyLib;

using static OdinSaves.PluginConfig;

namespace OdinSaves {
  [HarmonyPatch(typeof(Game))]
  static class GamePatch {
    static float _savePlayerProfileTimer = 0f;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.SavePlayerProfile))]
    static void SavePlayerProfilePostfix() {
      _savePlayerProfileTimer = 0f;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Game.UpdateSaving))]
    static void UpdateSavingPrefix(float dt) {
      _savePlayerProfileTimer += dt;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Game.UpdateSaving))]
    static void UpdateSavingPostfix(ref Game __instance, float dt) {
      if (!IsModEnabled.Value
          || _savePlayerProfileTimer <= 0f
          || _savePlayerProfileTimer < SavePlayerProfileInterval.Value) {
        return;
      }

      if (ShowMessageOnModSave.Value) {
        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "Saving player profile...");
      }

      _savePlayerProfileTimer = 0f;
      __instance.SavePlayerProfile(SetLogoutPointOnSave.Value);
    }
  }
}
