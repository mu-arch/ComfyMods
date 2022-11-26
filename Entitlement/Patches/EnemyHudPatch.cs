using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static Entitlement.PluginConfig;

namespace Entitlement {
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
        // Nothing.
      } else if (c.IsBoss()) {
        SetupBossHud(hudData);
      } else if (isMount) {
        // Nothing.
      } else {
        SetupEnemyHud(hudData);
      }
    }

    static readonly ConditionalWeakTable<EnemyHud.HudData, Text> _healthTextCache = new();

    static void SetupEnemyHud(EnemyHud.HudData hudData) {
      SetupHud(
          hudData,
          EnemyHudNameTextFontSize.Value,
          EnemyHudHealthTextFontSize.Value,
          EnemyHudHealthBarWidth.Value,
          EnemyHudHealthBarHeight.Value);
    }

    static void SetupBossHud(EnemyHud.HudData hudData) {
      SetupHud(
          hudData,
          BossHudNameTextFontSize.Value,
          BossHudHealthTextFontSize.Value,
          BossHudHealthBarWidth.Value,
          BossHudHealthBarHeight.Value);

      if (BossHudNameUseGradientEffect.Value) {
        hudData.m_name.gameObject.AddComponent<VerticalGradient>();
      }
    }

    static void SetupHud(
        EnemyHud.HudData hudData,
        int nameFontSize,
        int healthTextFontSize,
        float healthBarWidth,
        float healthBarHeight) {
      hudData.m_name
          .SetFontSize(nameFontSize)
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

      hudData.m_healthFast.SetWidth(healthBarWidth);
      hudData.m_healthFast.m_bar
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(healthBarWidth, healthBarHeight));

      hudData.m_healthSlow.SetWidth(healthBarWidth);
      hudData.m_healthSlow.m_bar
          .SetAnchorMin(new(0.5f, 0.5f))
          .SetAnchorMax(new(0.5f, 0.5f))
          .SetPivot(new(0.5f, 0.5f))
          .SetPosition(Vector2.zero)
          .SetSizeDelta(new(healthBarWidth, healthBarHeight));

      Text healthText = CreateEnemyHealthText(hudData, healthTransform, healthTextFontSize);
      _healthTextCache.Add(hudData, healthText);

      if (hudData.m_character.m_level > (hudData.m_character.IsBoss() ? 1 : 3)) {
        CreateEnemyLevelText(hudData, hudData.m_name.transform);

        hudData.m_level2?.gameObject.SetActive(false);
        hudData.m_level3?.gameObject.SetActive(false);
        hudData.m_level2 = default;
        hudData.m_level3 = default;
      } else {
        hudData.m_level2?.SetParent(hudData.m_name.transform, worldPositionStays: false);
        hudData.m_level2?.GetComponent<RectTransform>()
            .SetAnchorMin(new(1f, 0.5f))
            .SetAnchorMax(new(1f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(12f, 0f))
            .SetSizeDelta(Vector2.zero);

        hudData.m_level3?.SetParent(hudData.m_name.transform, worldPositionStays: false);
        hudData.m_level3?.GetComponent<RectTransform>()
            .SetAnchorMin(new(1f, 0.5f))
            .SetAnchorMax(new(1f, 0.5f))
            .SetPivot(new(0f, 0.5f))
            .SetPosition(new(20f, 0f))
            .SetSizeDelta(Vector2.zero);
      }
    }

    static Text CreateEnemyHealthText(EnemyHud.HudData hudData, Transform parentTransform, int healthTextFontSize) {
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
          .SetColor(Color.white)
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetResizeTextForBestFit(false);

      return healthText;
    }

    static Text CreateEnemyLevelText(EnemyHud.HudData hudData, Transform parentTransform) {
      Text levelText = UnityEngine.Object.Instantiate(hudData.m_name, parentTransform);

      levelText.GetComponent<RectTransform>()
          .SetAnchorMin(new(1f, 0.5f))
          .SetAnchorMax(new(1f, 0.5f))
          .SetPivot(new(0f, 0.5f))
          .SetPosition(new(5f, 0f));

      levelText
          .SetName("LevelText")
          .SetFontSize(Mathf.Clamp(levelText.fontSize, 18, levelText.fontSize))
          .SetColor(new(1f, 0.85882f, 0.23137f, 1f))
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetResizeTextForBestFit(false)
          .SetText($"{hudData.m_character.m_level - 1}{EnemyLevelStarSymbol.Value}");

      return levelText;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(EnemyHud.UpdateHuds))]
    static IEnumerable<CodeInstruction> UpdateHudsTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(
                  OpCodes.Ldfld, AccessTools.Field(typeof(EnemyHud.HudData), nameof(EnemyHud.HudData.m_character))),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.GetHoverName))),
              new CodeMatch(
                  OpCodes.Callvirt,
                  AccessTools.Method(
                      typeof(Localization), nameof(Localization.Localize), new Type[] { typeof(string) })),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Text), "set_text")),
              new CodeMatch(OpCodes.Ldloc_S))
          .Advance(offset: 6)
          .InsertAndAdvance(
              Transpilers.EmitDelegate<Func<EnemyHud.HudData, EnemyHud.HudData>>(NameSetTextPostDelegate))
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(
                  OpCodes.Ldfld, AccessTools.Field(typeof(EnemyHud.HudData), nameof(EnemyHud.HudData.m_character))),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.IsBoss))),
              new CodeMatch(OpCodes.Brtrue),
              new CodeMatch(OpCodes.Ldloc_S),
              new CodeMatch(
                  OpCodes.Ldfld, AccessTools.Field(typeof(EnemyHud.HudData), nameof(EnemyHud.HudData.m_gui))),
              new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameObject), "get_activeSelf")))
          .Advance(offset: 3)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(value => IsBossDelegate(value)))
          .InstructionEnumeration();
    }

    static EnemyHud.HudData NameSetTextPostDelegate(EnemyHud.HudData hudData) {
      if (IsModEnabled.Value
          && ShowEnemyHealthValue.Value
          && _healthTextCache.TryGetValue(hudData, out Text healthText)) {
        healthText.SetText($"{hudData.m_character.GetHealth():N0} / {hudData.m_character.GetMaxHealth():N0}");
      }

      return hudData;
    }

    static bool IsBossDelegate(bool value) {
      if (IsModEnabled.Value && FloatingBossHud.Value) {
        return false;
      }

      return value;
    }
  }
}
