using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZDOMan))]
  static class ZDOManPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.Load))]
    static bool LoadPrefix(ref ZDOMan __instance, ref BinaryReader reader, ref int version) {
      reader.ReadInt64();
      uint nextUid = reader.ReadUInt32();

      Atlas.LoadZdos(ref __instance, ref reader, ref version, ref nextUid);
      Atlas.LoadDeadZdos(ref __instance, ref reader, ref nextUid);

      __instance.RemoveOldGeneratedZDOS();
      __instance.m_nextUid = nextUid;

      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.SaveAsync))]
    static bool SaveAsync(ref ZDOMan __instance, ref BinaryWriter writer) {
      writer.Write(__instance.m_saveData.m_myid);
      writer.Write(__instance.m_saveData.m_nextUid);

      SaveZdos(ref writer, __instance.m_saveData.m_zdos);
      SaveDeadZdos(ref writer, __instance.m_saveData.m_deadZDOs);

      __instance.m_saveData.m_zdos.Clear();
      __instance.m_saveData.m_deadZDOs.Clear();
      __instance.m_saveData = null;

      return false;
    }

    static void SaveZdos(ref BinaryWriter writer, List<ZDO> zdos) {
      ZLog.Log($"Saving {zdos.Count} ZDOs ...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      ZPackage package = new();
      int count = zdos.Count;

      writer.Write(count);

      for (int i = 0; i < count; i++) {
        ZDO zdo = zdos[i];

        writer.Write(zdo.m_uid.userID);
        writer.Write(zdo.m_uid.id);

        package.Clear();
        zdo.Save(package);

        int size = package.Size();

        writer.Write(size);
        writer.Write(package.m_stream.GetBuffer(), 0, size);
      }

      stopwatch.Stop();
      ZLog.Log($"Saved {count} ZDOs, time: {stopwatch.Elapsed}");

      package.Clear();
    }

    static void SaveDeadZdos(ref BinaryWriter writer, Dictionary<ZDOID, long> deadZdos) {
      ZLog.Log($"Saving {deadZdos.Count} dead ZDOs...");
      Stopwatch stopwatch = Stopwatch.StartNew();

      writer.Write(deadZdos.Count);

      foreach (KeyValuePair<ZDOID, long> pair in deadZdos) {
        writer.Write(pair.Key.m_userID);
        writer.Write(pair.Key.m_id);
        writer.Write(pair.Value);
      }

      stopwatch.Stop();
      ZLog.Log($"Saved {deadZdos.Count} dead ZDOs, time: {stopwatch.Elapsed}");
    }
  }
}
