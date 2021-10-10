using BepInEx;

using HarmonyLib;

using System.Reflection;

namespace Compress {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Compress : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.compress";
    public const string PluginName = "Compress";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZRpc))]
    class ZRpcPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRpc.SendPackage))]
      static void SendPackagePrefix(ref ZPackage pkg) {
        byte[] data = Utils.Compress(pkg.GetArray());
        pkg.Clear();
        pkg.Write(data);
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZRpc.HandlePackage))]
      static void HandlePackagePrefix(ref ZPackage package) {
        package = package.ReadCompressedPackage();
      }
    }
  }
}