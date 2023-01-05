using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static Enhuddlement.PluginConfig;

namespace Enhuddlement {
  [HarmonyPatch(typeof(EnemyHud))]
  static class EnemyHudPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyHud.ShowHud))]
    static void ShowHudPrefix(ref EnemyHud __instance, ref Character c, ref bool __state) {
      __state = __instance.m_huds.ContainsKey(c);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyHud.ShowHud))]
    static void ShowHudPostfix(ref EnemyHud __instance, ref Character c, ref bool isMount, ref bool __state) {
      if (!IsModEnabled.Value || __state || !__instance.m_huds.TryGetValue(c, out EnemyHud.HudData hudData)) {
        return;
      }

      if (c.IsPlayer()) {
        SetupPlayerHud(hudData);
      } else if (c.IsBoss()) {
        SetupBossHud(hudData);
      } else if (isMount) {
        // Nothing.
      } else {
        SetupEnemyHud(hudData);
      }
    }

    static readonly ConditionalWeakTable<EnemyHud.HudData, Text> _healthTextCache = new();

    static void SetupPlayerHud(EnemyHud.HudData hudData) {
      SetupHud(
          hudData,
          PlayerHudNameTextFontSize.Value,
          PlayerHudNameTextColor.Value,
          PlayerHudHealthTextFontSize.Value,
          PlayerHudHealthTextColor.Value,
          PlayerHudHealthBarWidth.Value,
          PlayerHudHealthBarHeight.Value);

      hudData.m_healthFast.SetColor(PlayerHudHealthBarColor.Value);
    }

    static void SetupEnemyHud(EnemyHud.HudData hudData) {
      SetupHud(
          hudData,
          EnemyHudNameTextFontSize.Value,
          EnemyHudNameTextColor.Value,
          EnemyHudHealthTextFontSize.Value,
          EnemyHudHealthTextColor.Value,
          EnemyHudHealthBarWidth.Value,
          EnemyHudHealthBarHeight.Value);

      hudData.m_healthFast.SetColor(
          hudData.m_character.IsTamed() ? EnemyHudHealthBarTamedColor.Value : EnemyHudHealthBarColor.Value);
    }

    static void SetupBossHud(EnemyHud.HudData hudData) {
      SetupHud(
          hudData,
          BossHudNameTextFontSize.Value,
          EnemyHudNameTextColor.Value,
          BossHudHealthTextFontSize.Value,
          EnemyHudHealthTextColor.Value,
          BossHudHealthBarWidth.Value,
          BossHudHealthBarHeight.Value);

      hudData.m_healthFast.SetColor(BossHudHealthBarColor.Value);

      if (BossHudNameUseGradientEffect.Value) {
        hudData.m_name.gameObject.AddComponent<VerticalGradient>();
      }
    }

    static void SetupHud(
        EnemyHud.HudData hudData,
        int nameTextFontSize,
        Color nameTextColor,
        int healthTextFontSize,
        Color healthTextFontColor,
        float healthBarWidth,
        float healthBarHeight) {
      hudData.m_name
          .SetColor(nameTextColor)
          .SetFontSize(nameTextFontSize)
          .SetAlignment(TextAnchor.LowerCenter);

      hudData.m_name.GetComponent<RectTransform>()
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 0f))
          .SetPosition(new(0f, 8f))
          .SetSizeDelta(new(hudData.m_name.preferredWidth, hudData.m_name.preferredHeight));

      Transform healthTransform = hudData.m_gui.transform.Find("Health");
      healthTransform.GetComponent<RectTransform>()
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 1f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(healthBarWidth, healthBarHeight));

      SetupHealthBars(hudData, healthBarWidth, healthBarHeight);

      Text healthText = CreateHealthText(hudData, healthTransform, healthTextFontSize, healthTextFontColor);
      _healthTextCache.Add(hudData, healthText);

      if (hudData.m_character.m_level > (hudData.m_character.IsBoss() ? 1 : 3)) {
        CreateEnemyLevelText(hudData, healthTransform);

        hudData.m_level2.Ref()?.gameObject.SetActive(false);
        hudData.m_level3.Ref()?.gameObject.SetActive(false);
        hudData.m_level2 = default;
        hudData.m_level3 = default;
      } else {
        SetupEnemyLevelStars(hudData, healthTransform);
      }

      SetupAlerted(hudData);
      SetupAware(hudData);
    }

    static void SetupAlerted(EnemyHud.HudData hudData) {
      if (hudData.m_alerted) {
        Text alertedText = hudData.m_alerted.GetComponent<Text>();

        hudData.m_alerted.SetParent(hudData.m_name.transform, worldPositionStays: false);
        hudData.m_alerted
            .SetAnchorMin(new(0.5f, 1f))
            .SetAnchorMax(new(0.5f, 1f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(Vector2.zero)
            .SetSizeDelta(new(alertedText.preferredWidth, alertedText.preferredHeight));

        hudData.m_alerted.gameObject.SetActive(!EnemyHudUseNameForStatus.Value);
      }
    }

    static void SetupAware(EnemyHud.HudData hudData) {
      if (hudData.m_aware) {
        hudData.m_aware.SetParent(hudData.m_name.transform, worldPositionStays: false);
        hudData.m_aware.SetAnchorMin(new(0.5f, 1f))
            .SetAnchorMax(new(0.5f, 1f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(Vector2.zero)
            .SetSizeDelta(new(30f, 30f));

        hudData.m_aware.gameObject.SetActive(!EnemyHudUseNameForStatus.Value);
      }
    }

    static void SetupHealthBars(EnemyHud.HudData hudData, float healthBarWidth, float healthBarHeight) {
      hudData.m_healthFast.m_width = healthBarWidth;
      hudData.m_healthFast.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero)
          .SetSizeDelta(Vector2.zero);

      hudData.m_healthFast.m_bar
          .SetAnchorMin(new(0f, 0.5f))
          .SetAnchorMax(new(0f, 0.5f))
          .SetPivot(new(0f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(healthBarWidth, healthBarHeight));

      hudData.m_healthFast.gameObject.SetActive(true);

      hudData.m_healthSlow.m_width = healthBarWidth;
      hudData.m_healthSlow.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(Vector2.zero)
          .SetPosition(Vector2.zero)
          .SetSizeDelta(Vector2.zero);

      hudData.m_healthSlow.m_bar
          .SetAnchorMin(new(0f, 0.5f))
          .SetAnchorMax(new(0f, 0.5f))
          .SetPivot(new(0f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(healthBarWidth, healthBarHeight));

      hudData.m_healthSlow.gameObject.SetActive(true);

      // TODO: friendly bars.
      hudData.m_healthFastFriendly.Ref()?.gameObject.SetActive(false);
    }

    static Text CreateHealthText(
        EnemyHud.HudData hudData, Transform parentTransform, int healthTextFontSize, Color healthTextFontColor) {
      Text healthText = UnityEngine.Object.Instantiate(hudData.m_name, parentTransform);
      healthText.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.one)
          .SetPivot(new(0.5f, 0.5f))
          .SetPosition(Vector2.zero);

      healthText
          .SetName("HealthText")
          .SetText(string.Empty)
          .SetFontSize(healthTextFontSize)
          .SetColor(healthTextFontColor)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetResizeTextForBestFit(false);

      return healthText;
    }

    static Text CreateEnemyLevelText(EnemyHud.HudData hudData, Transform healthTransform) {
      Text levelText = UnityEngine.Object.Instantiate(hudData.m_name);

      levelText
          .SetName("LevelText")
          .SetFontSize(Mathf.Clamp(levelText.fontSize, EnemyLevelTextMinFontSize.Value, 64))
          .SetColor(new(1f, 0.85882f, 0.23137f, 1f))
          .SetResizeTextForBestFit(false);

      if (EnemyLevelShowByName.Value) {
        levelText.transform.SetParent(hudData.m_name.transform, worldPositionStays: false);

        levelText.GetComponent<RectTransform>()
            .SetAnchorMin(new(1f, 0.5f))
            .SetAnchorMax(new(1f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(5f, 0f))
            .SetSizeDelta(new(100f, levelText.GetPreferredHeight() + 5f));

        levelText
            .SetAlignment(TextAnchor.MiddleLeft)
            .SetHorizontalOverflow(HorizontalWrapMode.Overflow)
            .SetVerticalOverflow(VerticalWrapMode.Overflow);
      } else {
        levelText.transform.SetParent(healthTransform, worldPositionStays: false);

        Vector2 sizeDelta = healthTransform.GetComponent<RectTransform>().sizeDelta;
        sizeDelta.y = levelText.GetPreferredHeight() * 2f;

        levelText.GetComponent<RectTransform>()
            .SetAnchorMin(Vector2.zero)
            .SetAnchorMax(Vector2.zero)
            .SetPivot(Vector2.zero)
            .SetPosition(new(0f, 0f - sizeDelta.y - 2f))
            .SetSizeDelta(sizeDelta);

        levelText
            .SetAlignment(TextAnchor.UpperLeft)
            .SetHorizontalOverflow(HorizontalWrapMode.Wrap)
            .SetVerticalOverflow(VerticalWrapMode.Overflow);
      }

      int stars = hudData.m_character.m_level - 1;

      levelText.SetText(
          stars <= EnemyLevelStarCutoff.Value
              ? string.Concat(Enumerable.Repeat(EnemyLevelStarSymbol.Value, stars))
              : $"{stars}{EnemyLevelStarSymbol.Value}");

      levelText.gameObject.AddComponent<VerticalGradient>();

      return levelText;
    }

    static void SetupEnemyLevelStars(EnemyHud.HudData hudData, Transform healthTransform) {
      if (hudData.m_level2) {
        if (EnemyLevelShowByName.Value) {
          hudData.m_level2.SetParent(hudData.m_name.transform);
          hudData.m_level2
              .SetAnchorMin(new(1f, 0.5f))
              .SetAnchorMax(new(1f, 0.5f))
              .SetPivot(new(0f, 0.5f))
              .SetPosition(new(12f, 0f))
              .SetSizeDelta(Vector2.zero);
        } else {
          hudData.m_level2.SetParent(healthTransform, worldPositionStays: false);
          hudData.m_level2
              .SetAnchorMin(Vector2.zero)
              .SetAnchorMax(Vector2.zero)
              .SetPivot(Vector2.zero)
              .SetPosition(new(7.5f, -10f))
              .SetSizeDelta(Vector2.zero);
        }

        hudData.m_level2.gameObject.SetActive(hudData.m_character.GetLevel() == 2);
      }

      if (hudData.m_level3) {
        if (EnemyLevelShowByName.Value) {
          hudData.m_level3.SetParent(hudData.m_name.transform, worldPositionStays: false);
          hudData.m_level3
              .SetAnchorMin(new(1f, 0.5f))
              .SetAnchorMax(new(1f, 0.5f))
              .SetPivot(new(0f, 0.5f))
              .SetPosition(new(20f, 0f))
              .SetSizeDelta(Vector2.zero);
        } else {
          hudData.m_level3.SetParent(healthTransform, worldPositionStays: false);
          hudData.m_level3
              .SetAnchorMin(Vector2.zero)
              .SetAnchorMax(Vector2.zero)
              .SetPivot(Vector2.zero)
              .SetPosition(new(15.5f, -10f))
              .SetSizeDelta(Vector2.zero);
        }

        hudData.m_level3.gameObject.SetActive(hudData.m_character.GetLevel() == 3);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(EnemyHud.UpdateHuds))]
    static bool UpdateHudsPrefix(ref EnemyHud __instance, ref Player player, ref Sadle sadle, float dt) {
      if (IsModEnabled.Value) {
        EnemyHudUpdater.UpdateHuds(ref __instance, ref player, ref sadle, dt, _healthTextCache);
        return false;
      }

      return true;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyHud.LateUpdate))]
    static IEnumerable<CodeInstruction> LateUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Stloc_3),
              new CodeMatch(OpCodes.Ldloc_3),
              new CodeMatch(OpCodes.Ldloc_1),
              new CodeMatch(OpCodes.Call))
          .Advance(offset: 3)
          .SetInstructionAndAdvance(
              Transpilers.EmitDelegate<Func<Character, Player, bool>>(CharacterLocalPlayerEqualityDelegate))
          .InstructionEnumeration();
    }

    static bool CharacterLocalPlayerEqualityDelegate(Character character, Player player) {
      if (PlayerHudShowLocalPlayer.Value) {
        return false;
      }

      return character == player;
    }
  }
}
