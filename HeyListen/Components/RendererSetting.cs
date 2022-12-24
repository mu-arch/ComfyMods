using UnityEngine;

namespace ComfyLib {
  public class RendererSetting {
    static readonly int _colorShaderId = Shader.PropertyToID("_Color");
    static readonly int _emissionColorShaderId = Shader.PropertyToID("_EmissionColor");

    public Color OriginalColor { get; }
    public Color OriginalEmissionColor { get; }

    public Color CurrentColor { get; private set; }
    public Color CurrentEmissionColor { get; private set; }

    readonly Renderer _renderer;

    public RendererSetting(Renderer renderer) {
      _renderer = renderer;

      OriginalColor = _renderer.material.GetColor(_colorShaderId);
      OriginalEmissionColor = _renderer.material.GetColor(_emissionColorShaderId);

      CurrentColor = OriginalColor;
      CurrentEmissionColor = CurrentEmissionColor;
    }

    public RendererSetting SetActive(bool active) {
      _renderer.gameObject.SetActive(active);
      return this;
    }

    public RendererSetting SetColor(Color color) {
      if (color != CurrentColor) {
        CurrentColor = color;
        _renderer.material.SetColor(_colorShaderId, color);
      }

      return this;
    }

    public RendererSetting SetEmissionColor(Color color) {
      if (color != CurrentEmissionColor) {
        CurrentEmissionColor = color;
        _renderer.material.SetColor(_emissionColorShaderId, color);
      }

      return this;
    }
  }
}
