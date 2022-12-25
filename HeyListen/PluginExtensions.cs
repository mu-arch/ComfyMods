namespace HeyListen {
  public static class ColorExtensions {
    public static UnityEngine.Color SetAlpha(this UnityEngine.Color color, float alpha) {
      color.a = alpha;
      return color;
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T gameObject) where T : UnityEngine.Object {
      return gameObject ? gameObject : null;
    }
  }

  public static class ZDOExtensions {
    public static void Set(this ZDO zdo, int key, UnityEngine.Color value) {
      zdo.InitQuats();

      if (zdo.m_quats.TryGetValue(key, out UnityEngine.Quaternion quat)) {
        UnityEngine.Color color = new(quat.x, quat.y, quat.z, quat.w);
        
        if (color == value) {
          return;
        }
      }

      zdo.m_quats[key] = new(value.r, value.g, value.b, value.a);
      zdo.IncreseDataRevision();
    }

    public static UnityEngine.Color GetColor(this ZDO zdo, int key, UnityEngine.Color defaultValue) {
      if (zdo.m_quats != null && zdo.m_quats.TryGetValue(key, out UnityEngine.Quaternion quat)) {
        return new(quat.x, quat.y, quat.z, quat.w);
      }

      return defaultValue;
    }

    public static bool TryGetColor(this ZDO zdo, int key, out UnityEngine.Color result) {
      if (zdo.m_quats != null && zdo.m_quats.TryGetValue(key, out UnityEngine.Quaternion quat)) {
        result = new(quat.x, quat.y, quat.z, quat.w);
        return true;
      }

      result = default;
      return false;
    }

    public static bool TryGetFloat(this ZDO zdo, int key, out float result) {
      if (zdo.m_floats != null && zdo.m_floats.TryGetValue(key, out result)) {
        return true;
      }

      result = default;
      return false;
    }
  }
}
