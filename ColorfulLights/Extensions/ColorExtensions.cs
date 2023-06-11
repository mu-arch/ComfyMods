using UnityEngine;

namespace ColorfulLights {
  public static class ColorExtensions {
    public static string GetColorHtmlString(this Color color) {
      return color.a == 1f
          ? ColorUtility.ToHtmlStringRGB(color)
          : ColorUtility.ToHtmlStringRGBA(color);
    }

    public static Color SetAlpha(this Color color, float alpha) {
      color.a = alpha;
      return color;
    }
  }
}
