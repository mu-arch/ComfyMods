using HarmonyLib;

using static LicenseToSkill.PluginConfig;

namespace LicenseToSkill {
  [HarmonyPatch(typeof(Player))]
  public class PlayerPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player.HardDeath))]
    static void HardDeathPostfix(ref Player __instance, ref bool __result) {
      if (!IsModEnabled.Value || __instance != Player.m_localPlayer) {
        return;
      }

      __result = __instance.m_timeSinceDeath > (HardDeathCooldownOverride.Value * 60f);
    }
  }
}
