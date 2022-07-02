using HarmonyLib;

using static ZoneScouter.PluginConfig;
using static ZoneScouter.ZoneScouter;

namespace ZoneScouter.Patches {
  [HarmonyPatch(typeof(Hud))]
  public class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix() {
      if (IsModEnabled.Value) {
        ToggleSectorBoundaries();
      }
    }
  }
}
