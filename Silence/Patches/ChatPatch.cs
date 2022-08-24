using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

using UnityEngine;

using static Silence.Silence;

namespace Silence.Patches {
  [HarmonyPatch(typeof(Chat))]
  public class ChatPatch {
    static readonly CodeMatch InputGetKeyDownMatch =
        new(
            OpCodes.Call,
            AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new Type[] { typeof(KeyCode) }));

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Chat.Awake))]
    static void AwakePostfix(ref Chat __instance) {
      ChatInstance = __instance;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Chat.LateUpdate))]
    static void LateUpdatePostfix(ref Chat __instance) {
      if (!EnableChatWindow && __instance.m_chatWindow.gameObject.activeSelf) {
        __instance.m_chatWindow.gameObject.SetActive(false);
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(Chat.Update))]
    static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
          .MatchForward(useEnd: false, InputGetKeyDownMatch)
          .Advance(offset: 1)
          .InsertAndAdvance(
              new CodeInstruction(
                  OpCodes.Call,
                  Transpilers.EmitDelegate<Func<bool, bool>>(result => result && EnableChatWindow).operand))
          .InstructionEnumeration();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Chat.AddInworldText))]
    static bool AddInworldTextPrefix() {
      return EnableInWorldTexts;
    }
  }
}
