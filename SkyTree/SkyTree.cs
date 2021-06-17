using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SkyTree {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class SkyTree : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.skytree";
    public const string PluginName = "SkyTree";
    public const string PluginVersion = "1.0.0";

    private static ConfigEntry<bool> _isModEnabled;

    private Harmony _harmony;

    private void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchSelf();
      }
    }

    [HarmonyPatch(typeof(ZNetScene))]
    private class ZNetScenePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZNetScene.Awake))]
      private static void AwakePostfix(ZNetScene __instance) {
        if (_isModEnabled.Value) {
          __instance.StartCoroutine(FixYggdrasilBranchCoroutine());
        }
      }
    }

    private static IEnumerator FixYggdrasilBranchCoroutine() {
      ZLog.Log("Starting FixYggdrasilBranch coroutine.");

      while (true) {
        yield return new WaitForSeconds(3);

        if (!ZNetScene.instance) {
          continue;
        }

        GameObject[] objects = Array.FindAll(
            FindObjectsOfType<GameObject>(), obj => obj.name.StartsWith("YggdrasilBranch"));

        if (objects.Length <= 0) {
          continue;
        }

        foreach (var yggdrasil in objects) {
          ZLog.Log("Setting YggdrasilBranch layer to 15.");
          yggdrasil.layer = 15;

          var branch = yggdrasil.transform.Find("branch");

          if (!branch) {
            continue;
          }

          ZLog.Log("Found YggdrasilBranch/branch, setting layer to 15.");
          branch.gameObject.layer = 15;

          var filter = branch.GetComponentInChildren<MeshFilter>();

          if (!filter) {
            continue;
          }

          if (!branch.TryGetComponent(out MeshCollider collider)) {
            ZLog.Log("Adding collider to YggdrasilBranch/branch.");
            collider = branch.gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = filter.sharedMesh;
          }
        }

        ZLog.Log("Finished FixYggdrasilBranch coroutine.");
        yield break;
      }
    }
  }
}
