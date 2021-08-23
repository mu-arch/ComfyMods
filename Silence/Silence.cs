using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;
using System.Collections.Generic;

using System;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;
using System.Collections;

namespace Silence {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Silence : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.silence";
    public const string PluginName = "Silence";
    public const string PluginVersion = "1.0.0";

    public static ManualLogSource _logger;

    static ConfigEntry<bool> _isModEnabled;
    static ConfigEntry<KeyboardShortcut> _toggleSilenceShortcut;

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      _isModEnabled = Config.Bind(
          "_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      _toggleSilenceShortcut =
          Config.Bind(
              "Silence",
              "toggleSilenceShortcut",
              new KeyboardShortcut(KeyCode.S, KeyCode.RightShift),
              "Shortcut to toggle silence.");

      if (_isModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static Chat _chat;
    static bool _enableChat = true;

    [HarmonyPatch(typeof(Chat))]
    class ChatPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Chat.Awake))]
      static void AwakePostfix(ref Chat __instance) {
        _chat = __instance;
      }

      static readonly CodeMatch _inputGetKeyDownMatch =
          new(
              OpCodes.Call,
              AccessTools.Method(typeof(Input), nameof(Input.GetKeyDown), new Type[] { typeof(KeyCode) }));

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Chat.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(useEnd: false, _inputGetKeyDownMatch)
            .SetAndAdvance(
                OpCodes.Call,
                Transpilers.EmitDelegate<Func<KeyCode, bool>>(
                    keyCode => _enableChat && Input.GetKeyDown(keyCode)).operand)
            .InstructionEnumeration();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Chat.AddInworldText))]
      static bool AddInworldTextPrefix() {
        return _enableChat;
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.TakeInput))]
      static void TakeInputPostfix(ref Player __instance, ref bool __result) {
        if (!_toggleSilenceShortcut.Value.IsDown()) {
          return;
        }

        __instance.StartCoroutine(ToggleSilenceCoroutine());
        __result = false;
      }
    }

    static IEnumerator ToggleSilenceCoroutine() {
      yield return null;

      _enableChat = !_enableChat;

      _logger.LogInfo($"Setting enableChat to: {_enableChat}");
      MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, $"Setting enableChat to: {_enableChat}");

      if (!_enableChat) {
        _chat.m_hideTimer = _chat.m_hideDelay;
        _chat.m_wasFocused = false;

        foreach (Chat.WorldTextInstance worldText in _chat.m_worldTexts) {
          Destroy(worldText.m_gui);
        }

        _chat.m_worldTexts.Clear();
      }
    }
  }
}
