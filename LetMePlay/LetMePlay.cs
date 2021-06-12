using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LetMePlay {
  [BepInPlugin(Package, ModName, Version)]
  public class LetMePlay : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.letmeplay";
    public const string Version = "0.0.2";
    public const string ModName = "Let Me Play";

    private static ConfigEntry<bool> _isModEnabled;
    private static ConfigEntry<bool> _disableWardShieldFlash;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind("Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _disableWardShieldFlash =
          Config.Bind<bool>(
              "Effects",
              "disableWardShieldFlash",
              false,
              "Disable wards from flashing their blue shield.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchSelf();
      }
    }

    [HarmonyPatch(typeof(PrivateArea))]
    private class PrivateAreaPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(PrivateArea.RPC_FlashShield))]
      private static bool PrivateAreaRpcFlashShield(PrivateArea __instance, long uid) {
        if (_isModEnabled.Value && _disableWardShieldFlash.Value) {
          return false;
        }

        return true;
      }
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData))]
    private class ItemDataPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ItemDrop.ItemData.GetIcon))]
      private static void ItemDataGetIcon(ItemDrop.ItemData __instance) {
        if (_isModEnabled.Value) {
          if (__instance.m_variant < 0 || __instance.m_variant >= __instance.m_shared.m_icons.Length) {
            Array.Resize(ref __instance.m_shared.m_icons, __instance.m_variant + 1);
            __instance.m_shared.m_icons[__instance.m_variant] = GetSprite("yagluthdrop");
            __instance.m_shared.m_name = __instance.m_dropPrefab.name;
            __instance.m_shared.m_description = "Non-player item: " + __instance.m_dropPrefab.name;
            __instance.m_crafterID = 12345678L;
            __instance.m_crafterName = "redseiko.valheim.letmeplay";
          }
        }
      }
    }

    private static Sprite GetSprite(string spriteName) {
      return Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(obj => obj.name == spriteName);
    }

  }
}
