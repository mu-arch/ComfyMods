using System;

using TMPro;

namespace Insightful {
  public static class PluginExtensions {
    public static bool TryGetString(this ZDO zdo, int keyHashCode, out string result) {
      if (ZDOExtraData.s_strings.TryGetValue(zdo.m_uid, out BinarySearchDictionary<int, string> values)
          && values.TryGetValue(keyHashCode, out result)) {
        return true;
      }

      result = default;
      return false;
    }

    public static bool TryGetEnum<T>(this ZDO zdo, int keyHashCode, out T result) {
      if (ZDOExtraData.s_ints.TryGetValue(zdo.m_uid, out BinarySearchDictionary<int, int> values)
          && values.TryGetValue(keyHashCode, out int value)) {
        result = (T) Enum.ToObject(typeof(T), value);
        return true;
      }

      result = default;
      return false;
    }

    public static TextMeshProUGUI Append(this TextMeshProUGUI unityText, string value) {
      unityText.text = unityText.text.Length == 0 ? value : $"{unityText.text}\n{value}";
      return unityText;
    }

    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
