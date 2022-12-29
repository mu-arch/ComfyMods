using HarmonyLib;

using UnityEngine;

namespace Pseudonym {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
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

        SetupPlayerCustomization(
            __instance.m_playerInstance.Ref()?.GetComponent<Player>(),
            __instance.m_newCharacterPanel.GetComponentInChildren<PlayerCustomizaton>(includeInactive: true));

        return false;
      }

      _editingPlayerProfile = null;
      return true;
    }

    static void SetupPlayerCustomization(Player player, PlayerCustomizaton customization) {
      if (player && customization) {
        customization.m_maleToggle.SetIsOnWithoutNotify(player.m_modelIndex == 0);
        customization.m_femaleToggle.SetIsOnWithoutNotify(player.m_modelIndex == 1);

        float skinHue =
            InverseLerp(
                customization.m_skinColor0, customization.m_skinColor1, Utils.Vec3ToColor(player.m_skinColor));

        customization.m_skinHue.SetValueWithoutNotify(skinHue);

        float hairLevel = player.m_hairColor.x;
        Color hairColor = Utils.Vec3ToColor(player.m_hairColor / hairLevel);
        float hairTone = InverseLerp(customization.m_hairColor0, customization.m_hairColor1, hairColor);

        customization.m_hairTone.SetValueWithoutNotify(hairTone);
        customization.m_hairLevel.SetValueWithoutNotify(
            Mathf.InverseLerp(customization.m_hairMinLevel, customization.m_hairMaxLevel, hairLevel));
      } else {
        ZLog.LogWarning($"Could not setup player customization for editing.");
      }
    }

    static float InverseLerp(Vector4 a, Vector4 b, Vector4 value) {
      Vector4 ab = b - a;
      Vector4 av = value - a;

      return Vector4.Dot(ab, av) / Vector4.Dot(ab, ab);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnNewCharacterDone))]
    static bool OnNewCharacterDonePrefix(ref FejdStartup __instance) {
      if (_editingPlayerProfile != null) {
        string playerName = __instance.m_csNewCharacterName.text;
        ZLog.Log($"Saving existing player: {_editingPlayerProfile.GetName()} -> {playerName}");

        _editingPlayerProfile.SetName(playerName);
        _editingPlayerProfile.SavePlayerData(__instance.m_playerInstance.GetComponent<Player>());
        _editingPlayerProfile.SavePlayerToDisk();
        _editingPlayerProfile = null;

        __instance.m_selectCharacterPanel.SetActive(true);
        __instance.m_newCharacterPanel.SetActive(false);
        __instance.UpdateCharacterList();

        return true;
      }

      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnNewCharacterCancel))]
    static void OnNewCharacterCancel() {
      _editingPlayerProfile = null;
    }
  }
}
