using System.Reflection;
using System.Text.RegularExpressions;

using BepInEx;

using HarmonyLib;

using static LicensePlate.PluginConfig;

namespace LicensePlate {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class LicensePlate : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.licenseplate";
    public const string PluginName = "LicensePlate";
    public const string PluginVersion = "1.0.0";

    public static readonly int ShipLicensePlateHashCode = "ShipLicensePlate".GetStableHashCode();
    public static readonly int VagonLicensePlateHashCode = "VagonLicensePlate".GetStableHashCode();
    public static readonly int LicensePlateLastSetByHashCode = "LicensePlateLastSetBy".GetStableHashCode();
    public static readonly Regex HtmlTagsRegex = new("<.*?>");

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }
  }
}