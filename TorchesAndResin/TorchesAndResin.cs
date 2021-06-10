using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace TorchesAndResin {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class TorchesAndResin : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.torchesandresin";
    public const string PluginName = "Torches and Resin";
    public const string PluginVersion = "0.0.3";

    private const float _torchStartingFuel = 10000f;

    private static readonly string[] _eligibleTorchItemNames = {
      "$piece_groundtorchwood",
      "$piece_groundtorch",
      "$piece_sconce"
    };

    private static ConfigEntry<bool> _isModEnabled;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind<bool>("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchSelf();
      }
    }

    [HarmonyPatch(typeof(Fireplace))]
    class FireplacePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static void AwakePostfix(ref Fireplace __instance) {
        if (!_isModEnabled.Value) {
          return;
        }

        if (!__instance.m_nview || !__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) {
          return;
        }

        if (IsEligibleTorch(__instance.m_name)) {
          __instance.m_startFuel = _torchStartingFuel;
          __instance.m_nview.GetZDO().Set("fuel", __instance.m_startFuel);
        }
      }
    }

    private static bool IsEligibleTorch(string itemName) {
      return _eligibleTorchItemNames.Any(name => name.Equals(itemName));
    }
  }
}
