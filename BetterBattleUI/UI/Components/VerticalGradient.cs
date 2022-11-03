using System.Collections.Generic;

namespace UnityEngine.UI {
  public class VerticalGradient : BaseMeshEffect {
    [field: SerializeField]
    public UnityEngine.Gradient EffectGradient { get; set; } =
        new() {
          colorKeys = new GradientColorKey[] {
            new GradientColorKey(Color.gray, 0),
            new GradientColorKey(Color.white, 1)
          }
        };

    public enum Blend {
      Override,
      Add,
      Multiply
    }

    [field: SerializeField]
    public Blend BlendMode { get; set; } = Blend.Multiply;

    [field: SerializeField, Range(-1f, 1f)]
    public float EffectOffset { get; set; } = 0f;

    readonly List<UIVertex> _vertices = new();

    public override void ModifyMesh(VertexHelper helper) {
      if (!IsActive() || helper.currentVertCount == 0) {
        return;
      }

      _vertices.Clear();
      helper.GetUIVertexStream(_vertices);

      float y = _vertices[0].position.y;
      float top = y;
      float bottom = y;

      for (int i = _vertices.Count - 1; i > 0; i--) {
        y = _vertices[i].position.y;

        if (y > top) {
          top = y;
        } else if (y < bottom) {
          bottom = y;
        }
      }

      float height = 1f / (top - bottom);

      for (int i = 0, count = _vertices.Count; i < count; i++) {
        UIVertex vertex = _vertices[i];

        vertex.color =
            BlendColor(vertex.color, EffectGradient.Evaluate((vertex.position.y - bottom) * height - EffectOffset));

        _vertices[i] = vertex;
      }

      helper.Clear();
      helper.AddUIVertexTriangleStream(_vertices);
    }

    Color BlendColor(Color firstColor, Color secondColor) {
      return BlendMode switch {
        Blend.Add => firstColor + secondColor,
        Blend.Multiply => firstColor * secondColor,
        _ => secondColor
      };
    }
  }
}
