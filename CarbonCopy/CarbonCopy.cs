using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System.Reflection;

using UnityEngine;

namespace CarbonCopy {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class CarbonCopy : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.carboncopy";
    public const string PluginName = "CarbonCopy";
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

    [HarmonyPatch(typeof(Console))]
    class ConsolePatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Console.InputText))]
      static bool InputTextPrefix(ref Console __instance) {
        if (_isModEnabled.Value && ParseText(__instance.m_input.text)) {
          return false;
        }

        return true;
      }
    }

    [HarmonyPatch(typeof(Chat))]
    class ChatPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Chat.InputText))]
      static bool InputTextPrefix(ref Chat __instance) {
        if (_isModEnabled.Value && ParseText(__instance.m_input.text)) {
          return false;
        }

        return true;
      }
    }

    static bool ParseText(string text) {
      if (text == "/ccpanel") {
        BuildPanel panel = new(Hud.instance.transform);
        panel.Panel.SetActive(true);

        return true;
      }

      return false;
    }
  }
}