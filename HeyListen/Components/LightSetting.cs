using UnityEngine;

namespace ComfyLib {
  public class LightSetting {
    public Color OriginalColor { get; }
    public Color CurrentColor { get; private set; }

    readonly Light _light;

    public LightSetting(Light light) {
      _light = light;

      OriginalColor = light.color;
      CurrentColor = OriginalColor;
    }

    public LightSetting SetColor(Color color) {
      if (color != CurrentColor) {
        CurrentColor = color;
        _light.color = color;
      }

      return this;
    }
  }
}
