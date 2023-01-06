using HarmonyLib;

using static LicenseToSkill.PluginConfig;

namespace LicenseToSkill {
  [HarmonyPatch(typeof(SEMan))]
  public class SEManPatch {
    static readonly string StatusEffectSoftDeath = "SoftDeath";

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SEMan.AddStatusEffect), typeof(string), typeof(bool), typeof(int), typeof(float))]
    static void AddStatusEffectPostfix(
        ref SEMan __instance, ref StatusEffect __result, string name, bool resetTime) {
      if (!IsModEnabled.Value
          || !__result
          || __instance.m_character != Player.m_localPlayer
          || name != StatusEffectSoftDeath) {
        return;
      }

      __result.m_ttl = HardDeathCooldownOverride.Value * 60f;
    }
  }
}
