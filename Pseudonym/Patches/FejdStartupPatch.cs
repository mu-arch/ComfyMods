using HarmonyLib;

using UnityEngine;

namespace Pseudonym {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Awake))]
    static void AwakePostfix(ref FejdStartup __instance) {
      //
    }

    static PlayerProfile _editingPlayerProfile;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnCharacterNew))]
    static bool OnCharacterNewPrefix(ref FejdStartup __instance) {
      if (Input.GetKey(KeyCode.LeftShift) && __instance.TryGetPlayerProfile(out PlayerProfile profile)) {
        ZLog.Log($"Editing existing player: {profile.GetName()}");
        _editingPlayerProfile = profile;

        __instance.m_newCharacterPanel.SetActive(true);
        __instance.m_newCharacterError.SetActive(false);
        __instance.m_selectCharacterPanel.SetActive(false);

        __instance.m_csNewCharacterName.text = profile.GetName();
        __instance.SetupCharacterPreview(profile);

        Player player = __instance.m_playerInstance.Ref()?.GetComponent<Player>();

        PlayerCustomizaton playerCustomization =
            __instance.m_newCharacterPanel.GetComponentInChildren<PlayerCustomizaton>(includeInactive: true);

        if (player && playerCustomization) {
          float skinHue =
              InverseLerp(playerCustomization.m_skinColor0, playerCustomization.m_skinColor1, player.m_skinColor);
          ZLog.Log($"SkinHue is: {skinHue}");
          playerCustomization.m_skinHue.SetValueWithoutNotify(skinHue);
        }

        return false;
      }

      _editingPlayerProfile = null;
      return true;
    }

    static float InverseLerp(Vector4 a, Vector4 b, Vector4 value) {
      Vector4 ab = b - a;
      Vector4 av = value - a;

      return Vector4.Dot(ab, av) / Vector4.Dot(ab, ab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnNewCharacterDone))]
    static bool OnNewCharacterDonePrefix(ref FejdStartup __instance) {
      if (_editingPlayerProfile == null) {
        return true;
      }

      string playerName = __instance.m_csNewCharacterName.text;
      ZLog.Log($"Saving existing player: {_editingPlayerProfile.GetName()} -> {playerName}");

      _editingPlayerProfile.SetName(playerName);
      _editingPlayerProfile.SavePlayerData(__instance.m_playerInstance.GetComponent<Player>());
      _editingPlayerProfile.SavePlayerToDisk();
      _editingPlayerProfile = null;

      __instance.m_selectCharacterPanel.SetActive(true);
      __instance.m_newCharacterPanel.SetActive(false);
      __instance.UpdateCharacterList();

      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnNewCharacterCancel))]
    static void OnNewCharacterCancel() {
      _editingPlayerProfile = null;
    }
  }
}
