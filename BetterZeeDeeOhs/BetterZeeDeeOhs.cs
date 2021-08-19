using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace BetterZeeDeeOhs {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class BetterZeeDeeOhs : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.betterzeedeeohs";
    public const string PluginName = "BetterZeeDeeOhs";
    public const string PluginVersion = "1.0.0";

    static ManualLogSource _logger;

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static readonly ReaderWriterLockSlim _objectsBySectorLock = new(LockRecursionPolicy.SupportsRecursion);

    [HarmonyPatch(typeof(ZDOMan))]
    class ZDOManPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.AddToSector))]
      static void AddToSectorPrefix() {
        _objectsBySectorLock.EnterWriteLock();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.AddToSector))]
      static void AddToSectorPostfix() {
        _objectsBySectorLock.ExitWriteLock();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
      static void RemoveFromSectorPrefix() {
        _objectsBySectorLock.EnterWriteLock();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.RemoveFromSector))]
      static void RemoveFromSectorPostfix() {
        _objectsBySectorLock.ExitWriteLock();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.FindObjects))]
      static void FindObjectsPrefix() {
        _objectsBySectorLock.EnterUpgradeableReadLock();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.FindObjects))]
      static void FindObjectsPostfix() {
        _objectsBySectorLock.ExitUpgradeableReadLock();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.FindDistantObjects))]
      static void FindDistantObjectsPrefix() {
        _objectsBySectorLock.EnterUpgradeableReadLock();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.FindDistantObjects))]
      static void FindDistantObjectsPostfix() {
        _objectsBySectorLock.ExitUpgradeableReadLock();
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZDOMan.GetAllZDOsWithPrefabIterative))]
      static void GetAllZDOsWithPrefabIterativePrefix() {
        _objectsBySectorLock.EnterUpgradeableReadLock();
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.GetAllZDOsWithPrefabIterative))]
      static void GetAllZDOsWithPrefabIterativePostfix() {
        _objectsBySectorLock.ExitUpgradeableReadLock();
      }
    }

    [HarmonyPatch(typeof(ZNet))]
    class ZNetPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZNet.SaveWorld))]
      static bool SaveWorldPrefix(ref ZNet __instance) {
        __instance.StartCoroutine(SaveWorldCoroutine(__instance));
        return false;
      }
    }

    static IEnumerator SaveWorldCoroutine(ZNet zNet) {
      _logger.LogInfo("Starting SaveWorldCoroutine()...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      yield return PrepareSaveAsync(zNet.m_zdoMan, ZoneSystem.instance, RandEventSystem.instance).ToIEnumerator();
      yield return SaveWorldThreadAsync(zNet).ToIEnumerator();

      _logger.LogInfo($"Finished SaveWorldCoroutine() in {stopwatch.ElapsedMilliseconds} ms.");
    }

    static async Task PrepareSaveAsync(ZDOMan zdoMan, ZoneSystem zoneSystem, RandEventSystem randEventSystem) {
      await Task.Run(() => {
        ZDOMan.SaveData saveData = new();
        saveData.m_myid = zdoMan.m_myid;
        saveData.m_nextUid = zdoMan.m_nextUid;
        saveData.m_zdos = GetSaveClone(zdoMan);
        saveData.m_deadZDOs = new();

        zdoMan.m_saveData = saveData;

        zoneSystem.PrepareSave();
        randEventSystem.PrepareSave();
      });
    }

    static List<ZDO> GetSaveClone(ZDOMan zdoMan) {
      _logger.LogInfo("Starting GetSaveClone()...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      List<ZDO> clonedZdos = new(capacity: zdoMan.m_objectsByID.Count + 1024);

      for (int i = 0; i < zdoMan.m_objectsBySector.Length; i++) {
        List<ZDO> sectorZdos = zdoMan.m_objectsBySector[i];

        if (sectorZdos == null) {
          continue;
        }

        _objectsBySectorLock.EnterReadLock();

        foreach (ZDO zdo in sectorZdos) {
          if (zdo.m_persistent) {
            clonedZdos.Add(zdo.Clone());
          }
        }

        _objectsBySectorLock.ExitReadLock();
      }

      _objectsBySectorLock.EnterReadLock();

      foreach (List<ZDO> sectorZdos in zdoMan.m_objectsByOutsideSector.Values) {
        foreach (ZDO zdo in sectorZdos) {
          if (zdo.m_persistent) {
            clonedZdos.Add(zdo.Clone());
          }
        }
      }

      _objectsBySectorLock.ExitReadLock();

      _logger.LogInfo($"Finished GetSaveClone() in {stopwatch.ElapsedMilliseconds} ms, {clonedZdos.Count} ZDOs.");
      return clonedZdos;
    }

    static async Task SaveWorldThreadAsync(ZNet zNet) {
      await Task.Run(() => {
        _logger.LogInfo("Starting SaveWorldThreadAsync()...");
        zNet.SaveWorldThread();
        _logger.LogInfo("Finished SaveWorldThreadAsync()!");
      });
    }
  }
}