using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

using UnityEngine;
using UnityEngine.UI;

namespace HeadsUp {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class HeadsUp : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.headsup";
    public const string PluginName = "HeadsUp";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;
    Harmony _harmony;

    public void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(EnemyHud))]
    class EnemyHudPatch {
      static readonly ConditionalWeakTable<EnemyHud.HudData, Text> _hpTextCache = new();

      [HarmonyPrefix]
      [HarmonyPatch(nameof(EnemyHud.ShowHud))]
      static void ShowHudPrefix(ref EnemyHud __instance, ref Character c, ref bool __state) {
        if (!_isModEnabled.Value) {
          return;
        }

        __state = __instance.m_huds.ContainsKey(c);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(EnemyHud.ShowHud))]
      static void ShowHudPostfix(ref EnemyHud __instance, ref Character c, ref bool __state) {
        if (!_isModEnabled.Value
            || __state
            || !__instance.m_huds.TryGetValue(c, out EnemyHud.HudData hudData)) {
          return;
        }

        if (c.IsPlayer()) {
          RectTransform healthRectTransform = hudData.m_gui.transform.Find("Health").GetComponent<RectTransform>();
          Vector2 biggerSizeDelta = new(healthRectTransform.sizeDelta.x, 18f);

          healthRectTransform.sizeDelta = biggerSizeDelta;
          hudData.m_healthFast.m_bar.sizeDelta = new(hudData.m_healthFast.m_width, biggerSizeDelta.y);
          hudData.m_healthSlow.m_bar.sizeDelta = new(hudData.m_healthSlow.m_width, biggerSizeDelta.y);

          healthRectTransform.anchoredPosition = new(0f, -40f);

          Text hpText = Instantiate(hudData.m_name, hudData.m_name.transform.parent);
          Destroy(hpText.GetComponent<Outline>());

          _hpTextCache.Add(hudData, hpText);

          hpText.name = "HpText";
          hpText.rectTransform.anchoredPosition = Vector2.zero;
          hpText.fontSize = 14;
          hpText.color = Color.white;
          hpText.text = $"{hudData.m_character.GetHealth():0} / {hudData.m_character.GetMaxHealth():0}";
        } else if (c.IsBoss()) {

        } else {
          RectTransform healthRectTransform = hudData.m_gui.transform.Find("Health").GetComponent<RectTransform>();
          Vector2 biggerSizeDelta = new(healthRectTransform.sizeDelta.x, healthRectTransform.sizeDelta.y * 3f);

          healthRectTransform.sizeDelta = biggerSizeDelta;
          hudData.m_healthFast.m_bar.sizeDelta = new(hudData.m_healthFast.m_width, biggerSizeDelta.y);
          hudData.m_healthSlow.m_bar.sizeDelta = new(hudData.m_healthSlow.m_width, biggerSizeDelta.y);

          Text hpText = Instantiate(hudData.m_name, hudData.m_name.transform.parent);
          Destroy(hpText.GetComponent<Outline>());

          _hpTextCache.Add(hudData, hpText);

          hpText.name = "HpText";
          hpText.rectTransform.anchoredPosition = new(hpText.rectTransform.anchoredPosition.x, 1f);
          hpText.fontSize = 12;
          hpText.color = Color.white;
          hpText.text = $"{hudData.m_character.GetHealth():0} / {hudData.m_character.GetMaxHealth():0}";
        }
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(EnemyHud.UpdateHuds))]
      static IEnumerable<CodeInstruction> UpdateHudsTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldfld, typeof(EnemyHud.HudData).GetField(nameof(EnemyHud.HudData.m_healthFast))),
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Callvirt, typeof(GuiBar).GetMethod(nameof(GuiBar.SetValue))))
            .Advance(offset: 3)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(6)))
            .InsertAndAdvance(Transpilers.EmitDelegate<Action<EnemyHud.HudData>>(UpdateHealthTextDelegate))
            .InstructionEnumeration();
      }

      static void UpdateHealthTextDelegate(EnemyHud.HudData hudData) {
        if (_isModEnabled.Value && _hpTextCache.TryGetValue(hudData, out Text text)) {
          text.text = $"{hudData.m_character.GetHealth():0} / {hudData.m_character.GetMaxHealth():0}";
        }
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
        if (_isModEnabled.Value) {
          return false;
        }

        return character == player;
      }
    }
  }
}