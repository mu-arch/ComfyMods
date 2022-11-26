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

      if (hudData.m_level3) {
        hudData.m_level3.gameObject.SetActive(true);
        hudData.m_level3 = default;
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
          .SetPosition(new(0f, 8f));

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

      Text healthText = UnityEngine.Object.Instantiate(hudData.m_name, healthTransform);

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

      _healthTextCache.Add(hudData, healthText);
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
