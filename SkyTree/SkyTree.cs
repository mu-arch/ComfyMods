using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static SkyTree.PluginConfig;

namespace SkyTree {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class SkyTree : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.skytree";
    public const string PluginName = "SkyTree";
    public const string PluginVersion = "1.5.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    void Awake() {
      BindConfig(Config);

      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static readonly int SkyboxLayer = 19;
    public static readonly int StaticSolidLayer = 15;

    public static readonly List<GameObject> YggdrasilBranches = new();

    public static IEnumerator FixYggdrasilBranchCoroutine() {
      YggdrasilBranches.Clear();

      LogInfo("Starting FixYggdrasilBranch coroutine.");
      WaitForSeconds waitInterval = new(seconds: 3f);

      while (true) {
        yield return waitInterval;

        if (!ZNetScene.instance) {
          continue;
        }

        GameObject[] yggdrasils = Array.FindAll(
            FindObjectsOfType<GameObject>(), obj => obj.name.StartsWith("YggdrasilBranch"));

        if (yggdrasils.Length < 1) {
          continue;
        }

        string targetLayerName = $"{StaticSolidLayer}:{LayerMask.LayerToName(StaticSolidLayer)}";

        foreach (GameObject yggdrasil in yggdrasils) {
          int sourceLayer = yggdrasil.layer;
          string sourceLayerName = $"{sourceLayer}:{LayerMask.LayerToName(sourceLayer)}";

          LogInfo($"Setting YggdrasilBranch layer from {sourceLayerName} to {targetLayerName}.");

          yggdrasil.layer = StaticSolidLayer;
          YggdrasilBranches.Add(yggdrasil);

          Transform branch = yggdrasil.transform.Find("branch");

          if (!branch) {
            continue;
          }

          sourceLayer = branch.gameObject.layer;
          sourceLayerName = $"{sourceLayer}:{LayerMask.LayerToName(sourceLayer)}";

          LogInfo($"Found YggdrasilBranch/branch, setting layer from {sourceLayerName} to {targetLayerName}.");

          branch.gameObject.layer = StaticSolidLayer;
          YggdrasilBranches.Add(branch.gameObject);

          MeshFilter filter = branch.GetComponentInChildren<MeshFilter>();

          if (!filter || branch.TryGetComponent(out MeshCollider collider)) {
            continue;
          }

          LogInfo("Adding collider to YggdrasilBranch/branch.");
          collider = branch.gameObject.AddComponent<MeshCollider>();
          collider.sharedMesh = filter.sharedMesh;
        }

        LogInfo("Finished FixYggdrasilBranch coroutine.");
        yield break;
      }
    }

    public static void LogInfo(object o) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
    }

    public static void LogWarning(object o) {
      _logger.LogWarning($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
    }
  }
}
