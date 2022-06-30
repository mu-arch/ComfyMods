using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Reflection;

using UnityEngine;

using static SkyTree.PluginConfig;

namespace SkyTree {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class SkyTree : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.skytree";
    public const string PluginName = "SkyTree";
    public const string PluginVersion = "1.3.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginVersion);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static IEnumerator FixYggdrasilBranchCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 3f);
      _logger.LogInfo("Starting FixYggdrasilBranch coroutine.");

      while (true) {
        yield return waitInterval;

        if (!ZNetScene.m_instance) {
          continue;
        }

        GameObject[] yggdrasils = Array.FindAll(
            FindObjectsOfType<GameObject>(), obj => obj.name.StartsWith("YggdrasilBranch"));

        if (yggdrasils.Length < 1) {
          continue;
        }

        int targetLayer = 15;
        string targetLayerName = $"{targetLayer}:{LayerMask.LayerToName(targetLayer)}";

        foreach (GameObject yggdrasil in yggdrasils) {
          int sourceLayer = yggdrasil.layer;
          string sourceLayerName = $"{sourceLayer}:{LayerMask.LayerToName(sourceLayer)}";

          _logger.LogInfo($"Setting YggdrasilBranch layer from {sourceLayerName} to {targetLayerName}.");
          yggdrasil.layer = targetLayer;

          Transform branch = yggdrasil.transform.Find("branch");

          if (!branch) {
            continue;
          }

          sourceLayer = branch.gameObject.layer;
          sourceLayerName = $"{sourceLayer}:{LayerMask.LayerToName(sourceLayer)}";

          _logger.LogInfo( $"Found YggdrasilBranch/branch, setting layer from {sourceLayerName} to {targetLayerName}.");
          branch.gameObject.layer = targetLayer;

          MeshFilter filter = branch.GetComponentInChildren<MeshFilter>();

          if (!filter || branch.TryGetComponent(out MeshCollider collider)) {
            continue;
          }

          _logger.LogInfo("Adding collider to YggdrasilBranch/branch.");
          collider = branch.gameObject.AddComponent<MeshCollider>();
          collider.sharedMesh = filter.sharedMesh;
        }

        _logger.LogInfo("Finished FixYggdrasilBranch coroutine.");
        yield break;
      }
    }
  }
}
