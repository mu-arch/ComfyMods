using BepInEx;
using BepInEx.Configuration;
using System.Runtime.CompilerServices;

namespace YachtClub {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(Jotunn.Main.ModGuid)]
  internal class YachtClub : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.yachtclub";
    public const string PluginName = "YachtClub";
    public const string PluginVersion = "1.0.0";

    private static readonly ConditionalWeakTable<Ship, VikingShipData> _vikingShipData =
        new ConditionalWeakTable<Ship, VikingShipData>();

    private static ConfigEntry<bool> _isModEnabled;

    private void Awake() {
      Jotunn.Logger.ShowDate = true;

      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      On.Ship.Awake += ShipAwakePostfix;
    }

    private static void ShipAwakePostfix(On.Ship.orig_Awake orig, Ship self) {
      orig(self);

      if (_isModEnabled.Value && self.name.StartsWith("VikingShip")) {
        _vikingShipData.Add(self, new VikingShipData(self));
      }
    }
  }
}