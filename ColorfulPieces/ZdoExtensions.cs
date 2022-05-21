using UnityEngine;

namespace ColorfulPieces {
  public static class ZdoExtensions {
    public static bool TryGetVec3(this ZDO zdo, int keyHashCode, out Vector3 value) {
      if (zdo == null || zdo.m_vec3 == null) {
        value = default;
        return false;
      }

      return zdo.m_vec3.TryGetValue(keyHashCode, out value);
    }

    public static bool TryGetFloat(this ZDO zdo, int keyHashCode, out float value) {
      if (zdo == null || zdo.m_floats == null) {
        value = default;
        return false;
      }

      return zdo.m_floats.TryGetValue(keyHashCode, out value);
    }

    public static bool RemoveVec3(this ZDO zdo, int keyHashCode) {
      return zdo?.m_vec3?.Remove(keyHashCode) ?? false;
    }

    public static bool RemoveFloat(this ZDO zdo, int keyHashCode) {
      return zdo?.m_floats?.Remove(keyHashCode) ?? false;
    }
  }
}
