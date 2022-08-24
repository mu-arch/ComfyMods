using HarmonyLib;

using static LicenseToSkill.PluginConfig;

namespace LicenseToSkill {
  [HarmonyPatch(typeof(Skills))]
  class SkillsPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Skills.OnDeath))]
    static void OnDeathPrefix(ref Skills __instance) {
      if (!IsModEnabled.Value || __instance.m_player != Player.m_localPlayer) {
        return;
      }

      __instance.m_DeathLowerFactor = SkillLossPercentOverride.Value * 0.01f;
    }
  }
}
