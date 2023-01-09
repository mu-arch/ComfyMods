using HarmonyLib;

using static GetOffMyLawn.GetOffMyLawn;
using static GetOffMyLawn.PluginConfig;

namespace GetOffMyLawn {
  [HarmonyPatch(typeof(WearNTear))]
  public class WearNTearPatch {
    static readonly long PieceHealthDamageThreshold = 100_000L;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(WearNTear.ApplyDamage))]
    static bool ApplyDamagePrefix(ref WearNTear __instance, ref bool __result, ref float damage) {
      if (!IsModEnabled.Value || !EnablePieceHealthDamageThreshold.Value) {
        return true;
      }

      float health = __instance.m_nview.m_zdo.GetFloat(HealthHashCode, __instance.m_health);

      if (health <= 0f) {
        __result = false;
        return false;
      } else if (health >= PieceHealthDamageThreshold) {
        __result = false;
        return false;
      }

      health -= damage;
      __instance.m_nview.m_zdo.Set(HealthHashCode, health);

      if (health <= 0f) {
        __instance.Destroy();
      } else {
        __instance.m_nview.InvokeRPC(ZNetView.Everybody, "WNTHealthChanged", health);
      }

      __result = true;
      return false;
    }
  }
}
