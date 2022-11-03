using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;

using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static ColorfulDamage.PluginConfig;

namespace ColorfulDamage {
  [HarmonyPatch(typeof(DamageText))]
  static class DamageTextPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(DamageText.AddInworldText))]
    static bool AddInworldText(
        ref DamageText __instance, DamageText.TextType type, Vector3 pos, float distance, float dmg, bool mySelf) {
      if (!IsModEnabled.Value) {
        return true;
      }

      DamageText.WorldTextInstance worldText = new();
      __instance.m_worldTexts.Add(worldText);

      worldText.m_worldPos = pos;
      worldText.m_timer = 0f;
      worldText.m_gui = UnityEngine.Object.Instantiate(__instance.m_worldTextBase, __instance.transform);

      if (DamageTextUseShadowEffect.Value) {
        if (worldText.m_gui.TryGetComponent(out Outline outline)) {
          outline.enabled = false;
        }

        Shadow shadow = worldText.m_gui.AddComponent<Shadow>();
        shadow.effectColor = DamageTextShadowEffectColor.Value;
        shadow.effectDistance = DamageTextShadowEffectDistance.Value;
      }

      worldText.m_textField = worldText.m_gui.GetComponent<Text>();
      worldText.m_textField.text = GetWorldTextText(type, dmg);
      worldText.m_textField.color = GetWorldTextColor(type, dmg, mySelf);

      worldText.m_textField.font = FontCache.GetFont(DamageTextMessageFont.Value);
      worldText.m_textField.fontSize =
          distance > DamageTextSmallPopupDistance.Value
              ? DamageTextSmallFontSize.Value
              : DamageTextLargeFontSize.Value;

      return false;
    }

    static Color GetWorldTextColor(DamageText.TextType damageTextType, float damage, bool isPlayerDamage) {
      if (damageTextType == DamageText.TextType.Heal) {
        return DamageTextHealColor.Value;
      }

      if (isPlayerDamage) {
        return damage == 0f ? DamageTextPlayerNoDamageColor.Value : DamageTextPlayerDamageColor.Value;
      }

      return damageTextType switch {
        DamageText.TextType.Normal => DamageTextNormalColor.Value,
        DamageText.TextType.Resistant => DamageTextResistantColor.Value,
        DamageText.TextType.Weak => DamageTextWeakColor.Value,
        DamageText.TextType.Immune => DamageTextImmuneColor.Value,
        DamageText.TextType.Heal => DamageTextHealColor.Value,
        DamageText.TextType.TooHard => DamageTextTooHardColor.Value,
        DamageText.TextType.Blocked => DamageTextBlockedColor.Value,
        _ => Color.white,
      };
    }

    static readonly Lazy<string> _msgTooHard = new(() => Localization.m_instance.Localize("$msg_toohard"));
    static readonly Lazy<string> _msgBlocked = new(() => Localization.m_instance.Localize("$msg_blocked: "));

    static string GetWorldTextText(DamageText.TextType damageTextType, float damage) {
      return damageTextType switch {
        DamageText.TextType.Heal => "+" + damage.ToString("0.#", CultureInfo.InvariantCulture),
        DamageText.TextType.TooHard => _msgTooHard.Value,
        DamageText.TextType.Blocked => _msgBlocked.Value + damage.ToString("0.#", CultureInfo.InvariantCulture),
        _ => damage.ToString("0.#", CultureInfo.InvariantCulture),
      };
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(DamageText.UpdateWorldTexts))]
    static bool UpdateWorldTextsPrefix(ref DamageText __instance, float dt) {
      if (!IsModEnabled.Value) {
        return true;
      }

      Camera camera = Utils.GetMainCamera();

      float width = Screen.width;
      float height = Screen.height;
      
      for (int i = __instance.m_worldTexts.Count - 1; i >= 0; i--) {
        DamageText.WorldTextInstance worldText = __instance.m_worldTexts[i];
        worldText.m_timer += dt;

        if (worldText.m_timer > DamageTextPopupDuration.Value) {
          UnityEngine.Object.Destroy(worldText.m_gui);

          // Source: https://www.vertexfragment.com/ramblings/list-removal-performance/
          __instance.m_worldTexts[i] = __instance.m_worldTexts[__instance.m_worldTexts.Count - 1];
          __instance.m_worldTexts.RemoveAt(__instance.m_worldTexts.Count - 1);

          continue;
        }

        float t = worldText.m_timer / DamageTextPopupDuration.Value;

        Vector3 position =
            Vector3.Lerp(worldText.m_worldPos, worldText.m_worldPos + DamageTextPopupLerpPosition.Value, t);

        Vector3 point = camera.WorldToScreenPoint(position);

        if (point.x < 0f || point.x > width || point.y < 0f || point.y > height || point.z < 0f) {
          worldText.m_gui.SetActive(false);
          continue;
        }
        
        Color color = worldText.m_textField.color;
        color.a = 1f - (t * t * t);
        worldText.m_textField.color = color;

        worldText.m_gui.SetActive(true);
        worldText.m_gui.transform.position = point;
      }

      return false;
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(DamageText.RPC_DamageText))]
    static IEnumerable<CodeInstruction> RpcDamageTextTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(
              useEnd: false,
              new CodeMatch(OpCodes.Ldarg_0),
              new CodeMatch(
                  OpCodes.Ldfld, AccessTools.Field(typeof(DamageText), nameof(DamageText.m_maxTextDistance))))
          .Advance(offset: 2)
          .InsertAndAdvance(Transpilers.EmitDelegate<Func<float, float>>(MaxTextDistanceDelegate))
          .InstructionEnumeration();
    }

    static float MaxTextDistanceDelegate(float maxTextDistance) {
      return IsModEnabled.Value ? DamageTextMaxPopupDistance.Value : maxTextDistance;
    }
  }
}
