using TMPro;

using UnityEngine;

namespace ComfyLib {
  public static class ObjectExtensions {
    public static T Ref<T>(this T obj) where T : UnityEngine.Object {
      return obj ? obj : null;
    }
  }

  public static class TextMeshProUGUIExtensions {
    public static TextMeshProUGUI SetColor(this TextMeshProUGUI text, Color color) {
      if (text.color != color) {
        text.color = color;
      }

      return text;
    }
    public static TextMeshProUGUI SetFont(this TextMeshProUGUI text, TMP_FontAsset font) {
      if (text.font != font) {
        text.font = font;
      }

      return text;
    }
  }
}