using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static Pseudonym.PluginConfig;

namespace Pseudonym {
  [HarmonyPatch(typeof(FejdStartup))]
  static class FejdStartupPatch {
    static Button _editButton;
    static PlayerProfile _editingPlayerProfile;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.Start))]
    static void StartPostfix(ref FejdStartup __instance) {
      if (IsModEnabled.Value) {
        CreateEditButton(__instance);
      }
    }

    static void CreateEditButton(FejdStartup fejdStartup) {
      UnityEngine.Object.Destroy(_editButton);

      _editButton =
          UnityEngine.Object.Instantiate(fejdStartup.m_csNewButton, fejdStartup.m_csNewButton.transform.parent);

      _editButton.onClick.RemoveAllListeners();
      _editButton.onClick.AddListener(() => OnEditCharacter(fejdStartup));

      _editButton.GetComponentInChildren<Text>().text = "Edit";
      _editButton.GetComponent<RectTransform>().anchoredPosition += new Vector2(190f, 0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(FejdStartup.UpdateCharacterList))]
    static void UpdateCharacterList(ref FejdStartup __instance) {
      _editButton.Ref()?.gameObject.SetActive(__instance.m_profiles.Count > 0);
    }

    static void OnEditCharacter(FejdStartup fejdStartup) {
      if (fejdStartup.TryGetPlayerProfile(out PlayerProfile profile)) {
        ZLog.Log($"Editing existing player: {profile.GetName()}");
        _editingPlayerProfile = profile;

        fejdStartup.m_newCharacterPanel.SetActive(true);
        fejdStartup.m_newCharacterError.SetActive(false);
        fejdStartup.m_selectCharacterPanel.SetActive(false);

        fejdStartup.m_csNewCharacterName.text = profile.GetName();
        fejdStartup.SetupCharacterPreview(profile);

        SetupPlayerCustomization(
            fejdStartup.m_playerInstance.Ref()?.GetComponent<Player>(),
            fejdStartup.m_newCharacterPanel.GetComponentInChildren<PlayerCustomizaton>(includeInactive: true));
      } else {
        _editingPlayerProfile = null;
      }
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

        return false;
      }

      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(FejdStartup.OnNewCharacterCancel))]
    static void OnNewCharacterCancel() {
      _editingPlayerProfile = null;
    }
  }
}
