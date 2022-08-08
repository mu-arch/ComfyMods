using System;

using UnityEngine.UI;

namespace Insightful {
  static class PluginExtensions {
    public static bool TryGetString(this ZDO zdo, int keyHashCode, out string result) {
      if (zdo.m_strings == null) {
        result = default;
        return false;
      }

      return zdo.m_strings.TryGetValue(keyHashCode, out result);
    }

    public static bool TryGetEnum<T>(this ZDO zdo, int keyHashCode, out T result) {
      if (zdo.m_ints != null && zdo.m_ints.TryGetValue(keyHashCode, out int value)) {
        result = (T) Enum.ToObject(typeof(T), value);
        return true;
      }

      result = default;
      return false;
    }

    public static Text Append(this Text unityText, string value) {
      unityText.text = unityText.text.Length == 0 ? value : $"{unityText.text}\n{value}";
      return unityText;
    }

    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
