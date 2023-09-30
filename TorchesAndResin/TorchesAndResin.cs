using System.Reflection;

using BepInEx;

using HarmonyLib;

using static TorchesAndResin.PluginConfig;

namespace TorchesAndResin {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class TorchesAndResin : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.torchesandresin";
    public const string PluginName = "TorchesAndResin";
    public const string PluginVersion = "1.4.0";

    public const float TorchStartingFuel = 10000f;

    public static readonly int FuelHashCode = "fuel".GetStableHashCode();

    public static readonly string[] EligibleTorchItemNames = {
      "piece_groundtorch_wood", // Standing Wood Torch
      "piece_groundtorch",      // Standing Iron Torch
      "piece_walltorch",        // Sconce Torch
      "piece_brazierceiling01", // Hanging Brazier
      "piece_brazierfloor01",   // Floor Brazier
      "fire_pit_iron",          // Firepit Iron
    };

    Harmony _harmony;

    void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginVersion);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}
