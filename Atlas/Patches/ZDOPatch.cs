using System.Collections.Generic;

using HarmonyLib;

namespace Atlas {
  [HarmonyPatch(typeof(ZDO))]
  static class ZDOPatch {
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDO.Deserialize))]
    static bool DeserializePrefix(ref ZDO __instance, ref ZPackage pkg, ref ZDOExtraData.ConnectionType __result) {
      ZDOID zid = __instance.m_uid;
      __result = ZDOExtraData.ConnectionType.None;

      ushort dataFlag = pkg.ReadUShort();

      __instance.Persistent = (dataFlag & 256) > 0;
      __instance.Distant = (dataFlag & 512) > 0;
      __instance.Type = (ZDO.ObjectType) ((dataFlag >> 10) & 3);

      // Prefab
      __instance.m_prefab = pkg.ReadInt();

      // Rotation
      if ((dataFlag & 4096) > 0) {
        __instance.m_rotation = pkg.ReadVector3();
      }

      // Connection
      if ((dataFlag & 1) > 0) {
        ZDOExtraData.ConnectionType connectionType = (ZDOExtraData.ConnectionType) pkg.ReadByte();
        ZDOID connectionTarget = pkg.ReadZDOID();
        ZDOExtraData.SetConnection(zid, connectionType, connectionTarget);

        __result |= connectionType & ~ZDOExtraData.ConnectionType.Target;
      } else {
        ZDOExtraData.s_connections.Remove(zid);
      }

      // Float
      if ((dataFlag & 2) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_floats.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_floats[zid][pkg.ReadInt()] = pkg.ReadSingle();
        }
      } else {
        ZDOExtraData.s_floats.Release(zid);
      }

      // Vector3
      if ((dataFlag & 4) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_vec3.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_vec3[zid][pkg.ReadInt()] = pkg.ReadVector3();
        }
      } else {
        ZDOExtraData.s_vec3.Release(zid);
      }

      // Quaternion
      if ((dataFlag & 8) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_quats.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_quats[zid][pkg.ReadInt()] = pkg.ReadQuaternion();
        }
      } else {
        ZDOExtraData.s_quats.Release(zid);
      }

      // Int
      if ((dataFlag & 16) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_ints.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_ints[zid][pkg.ReadInt()] = pkg.ReadInt();
        }
      } else {
        ZDOExtraData.s_ints.Release(zid);
      }

      // Long
      if ((dataFlag & 32) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_longs.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_longs[zid][pkg.ReadInt()] = pkg.ReadLong();
        }
      } else {
        ZDOExtraData.s_longs.Release(zid);
      }

      // String
      if ((dataFlag & 64) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_strings.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_strings[zid][pkg.ReadInt()] = pkg.ReadString();
        }
      } else {
        ZDOExtraData.s_strings.Release(zid);
      }

      // ByteArray
      if ((dataFlag & 128) > 0) {
        int count = pkg.ReadByte();
        ZDOExtraData.s_byteArrays.InitOrReset(zid, count);

        for (int i = 0; i < count; i++) {
          ZDOExtraData.s_byteArrays[zid][pkg.ReadInt()] = pkg.ReadByteArray();
        }
      } else {
        ZDOExtraData.s_byteArrays.Release(zid);
      }

      return false;
    }

    static void InitOrReset<T>(
        this Dictionary<ZDOID, BinarySearchDictionary<int, T>> container, ZDOID zid, int capacity) {
      if (!container.TryGetValue(zid, out BinarySearchDictionary<int, T> values)) {
        values = Pool<BinarySearchDictionary<int, T>>.Create();
        container[zid] = values;
      }

      values.Clear();
      values.Reserve(capacity);
    }
  }
}
