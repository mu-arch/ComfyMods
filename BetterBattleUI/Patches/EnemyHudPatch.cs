using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static BetterBattleUI.PluginConfig;

namespace BetterBattleUI {
  [HarmonyPatch(typeof(EnemyHud))]
  static class EnemyHudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(EnemyHud.Awake))]
    static void AwakePostfix(ref EnemyHud __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      // TODO: temporary, just for testing.
      GameObject bossName = __instance.m_baseHudBoss.transform.Find("Name").gameObject;
      bossName.AddComponent<VerticalGradient>();
      bossName.AddComponent<WobblyText>();
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
          .ThrowIfNotMatch(
              "Could not find: value.m_name.text = Localization.instance.Localize(value.m_character.GetHoverName());")
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

    //static EnemyHud.HudData IsHoverCreatureDelegate(EnemyHud.HudData hudData) {
    //  return hudData;
    //}

    static EnemyHud.HudData NameSetTextPostDelegate(EnemyHud.HudData hudData) {
      hudData.m_name.text += $" ({hudData.m_character.GetHealth():N0} / {hudData.m_character.GetMaxHealth():N0})";
      return hudData;
    }

    static bool IsBossDelegate(bool value) {
      return false;
    }
  }
}
