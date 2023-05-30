using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

using BepInEx;

using HarmonyLib;

using static Atlas.PluginConfig;

namespace Atlas {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Atlas : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.atlas";
    public const string PluginName = "Atlas";
    public const string PluginVersion = "1.5.0";

    public static readonly int TimeCreatedHashCode = "timeCreated".GetStableHashCode();

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static IEnumerator SaveWorldCoroutine(ZNet zNet) {
      yield return SaveWorldAsync(zNet).ToIEnumerator();
    }

    static async Task SaveWorldAsync(ZNet zNet) {
      await Task
          .Run(() => {
            PrepareSaveData(ref zNet.m_zdoMan);
            ZoneSystem.m_instance.PrepareSave();
            RandEventSystem.m_instance.PrepareSave();
            zNet.SaveWorldThread();

            ZLog.Log($"Garbage collecting now...");
            GC.Collect();
          })
          .ConfigureAwait(continueOnCapturedContext: false);
    }

    static void PrepareSaveData(ref ZDOMan zdoMan) {
      ZDOMan.SaveData saveData = new();
      saveData.m_sessionID = zdoMan.m_sessionID;
      saveData.m_nextUid = zdoMan.m_nextUid;
      saveData.m_zdos = CloneZdos(ref zdoMan);

      Stopwatch stopwatch = Stopwatch.StartNew();
      ZDOExtraData.PrepareSave();

      stopwatch.Stop();
      ZLog.Log($"Finished ZDOExtraData.PrepareSave() in: {stopwatch.ElapsedMilliseconds} ms.");

      zdoMan.m_saveData = saveData;
    }

    static List<ZDO> CloneZdos(ref ZDOMan zdoMan) {
      List<ZDO> clonedZdos = new(capacity: zdoMan.m_objectsByID.Count);

      ZLog.Log($"Start cloning {clonedZdos.Capacity} ZDOs ...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      for (int i = zdoMan.m_objectsBySector.Length - 1; i >= 0; i--) {
        List<ZDO> zdos = zdoMan.m_objectsBySector[i];

        if (zdos == null) {
          continue;
        }

        for (int j = zdos.Count - 1; j >= 0; j--) {
          ZDO zdo = zdos[j];

          if (zdo != null && zdo.Valid && zdo.Persistent) {
            clonedZdos.Add(zdo.Clone());
          }
        }
      }

      List<Vector2i> keys = new(zdoMan.m_objectsByOutsideSector.Keys);

      foreach (Vector2i key in keys) {
        if (!zdoMan.m_objectsByOutsideSector.TryGetValue(key, out List<ZDO> zdos)) {
          continue;
        }

        for (int i = zdos.Count - 1; i >= 0; i--) {
          ZDO zdo = zdos[i];

          if (zdo != null && zdo.Valid && zdo.Persistent) {
            clonedZdos.Add(zdo.Clone());
          }
        }
      }

      stopwatch.Stop();
      ZLog.Log($"Finished cloning {clonedZdos.Count} ZDOs in: {stopwatch.ElapsedMilliseconds} ms.");

      return clonedZdos;
    }
  }
}