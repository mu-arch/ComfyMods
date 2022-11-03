using System.Collections.Generic;

namespace UnityEngine.UI {
  public class WobblyText : BaseMeshEffect {
    [field: SerializeField]
    public float Speed { get; set; } = -8f;

    [field: SerializeField]
    public float Density { get; set; } = 0.5f;

    [field: SerializeField]
    public float Magnitude { get; set; } = 2f;

    [field: SerializeField]
    public bool AutoUpdate { get; set; } = true;

    Text _text;
    readonly List<UIVertex> _vertices = new();

    protected override void Start() {
      _text = GetComponent<Text>();
      base.Start();
    }

    public override void ModifyMesh(VertexHelper helper) {
      if (!IsActive() || helper.currentVertCount == 0) {
        return;
      }

      _vertices.Clear();
      helper.GetUIVertexStream(_vertices);

      float t = Time.timeSinceLevelLoad;

      for (int i = 0, count = (_vertices.Count / 6); i < count; i++) {
        Vector3 delta = new(1, Magnitude * Mathf.Sin((t * Speed) + (i * Density)), 1);

        for (int j = 0; j < 6; j++) {
          UIVertex vertex = _vertices[(i * 6) + j];
          vertex.position += delta;

          _vertices[(i * 6) + j] = vertex;
        }
      }

      helper.Clear();
      helper.AddUIVertexTriangleStream(_vertices);
    }

    void Update() {
      if (AutoUpdate) {
        _text.SetVerticesDirty();
      }
    }
  }
}
