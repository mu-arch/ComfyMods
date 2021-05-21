using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;

namespace LetMePlay {
  [BepInPlugin("your.unique.mod.identifier", LetMePlay.ModName, LetMePlay.Version)]
  public class LetMePlay : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.letmeplay";
    public const string Version = "0.0.1";
    public const string ModName = "Let Me Play";

    private static ConfigEntry<bool> _isModEnabled;
    private static ConfigEntry<bool> _disableWardShieldFlash;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind("Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _disableWardShieldFlash = Config.Bind(
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
  }
}
