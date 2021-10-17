using BepInEx;

using HarmonyLib;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace Atlas {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Atlas : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.atlas";
    public const string PluginName = "Atlas";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZNet))]
    class ZNetPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZNet.SaveWorld))]
      static bool SaveWorldPrefix(ref ZNet __instance, ref bool sync) {
        if (sync) {
          return true;
        }

        __instance.StartCoroutine(SaveWorldCoroutine(__instance));
        return false;
      }
    }

    static IEnumerator SaveWorldCoroutine(ZNet zNet) {
      yield return SaveWorldAsync(zNet).ToIEnumerator();
    }

    static async Task SaveWorldAsync(ZNet zNet) {
      await Task
          .Run(() => {
            PrepareZDOManSave(ref zNet.m_zdoMan);
            ZoneSystem.instance.PrepareSave();
            RandEventSystem.instance.PrepareSave();
            zNet.SaveWorldThread();
          })
          .ConfigureAwait(continueOnCapturedContext: false);
    }

    static void PrepareZDOManSave(ref ZDOMan zdoMan) {
      ZDOMan.SaveData saveData = new();
      saveData.m_myid = zdoMan.m_myid;
      saveData.m_nextUid = zdoMan.m_nextUid;
      saveData.m_deadZDOs = new();
      saveData.m_zdos = CloneZdos(ref zdoMan);

      zdoMan.m_saveData = saveData;

    }
    static List<ZDO> CloneZdos(ref ZDOMan zdoMan) {
      List<ZDO> clonedZdos = new(capacity: zdoMan.m_objectsByID.Count);

      ZLog.Log($"Start cloning {clonedZdos.Capacity} ZDOs...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      for (int i = zdoMan.m_objectsBySector.Length - 1; i >= 0; i--) {
        List<ZDO> zdos = zdoMan.m_objectsBySector[i];

        if (zdos == null) {
          continue;
        }

        for (int j = zdos.Count - 1; j >= 0; j--) {
          ZDO zdo = zdos[j];

          if (zdo != null && zdo.m_zdoMan != null && zdo.m_persistent) {
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

          if (zdo != null && zdo.m_zdoMan != null && zdo.m_persistent) {
            clonedZdos.Add(zdo.Clone());
          }
        }
      }

      stopwatch.Stop();
      ZLog.Log($"Finished cloning {clonedZdos.Count} ZDOs in {stopwatch.ElapsedMilliseconds} ms.");

      return clonedZdos;
    }
  }

  public static class TaskExtensions {
    public static IEnumerator ToIEnumerator(this Task task) {
      while (!task.IsCompleted) {
        yield return null;
      }

      if (task.IsFaulted) {
        throw task.Exception;
      }
    }
  }
}