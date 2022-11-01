using System.Collections.Generic;

using UnityEngine;

using static ColorfulPieces.ColorfulPieces;

namespace ColorfulPieces {
  public class PieceColor : MonoBehaviour {
    public static readonly Dictionary<string, int> RendererCountCache = new();
    public static readonly List<PieceColor> PieceColorCache = new();

    static readonly int _colorId = Shader.PropertyToID("_Color");
    static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");

    public List<Renderer> Renderers { get; } = new(0);
    public Color TargetColor { get; set; } = Color.clear;
    public float TargetEmissionColorFactor { get; set; } = 0f;

    readonly MaterialPropertyBlock _propertyBlock = new();

    static readonly Vector3 _noColor = Vector3.one * -1f;

    int _cacheIndex;
    long _lastDataRevision;
    Vector3 _lastColorVec3;
    float _lastEmissionColorFactor;

    ZNetView _netView;

    void Awake() {
      _lastDataRevision = -1L;
      _lastColorVec3 = _noColor;
      _lastEmissionColorFactor = 0f;
      _cacheIndex = -1;

      _propertyBlock.Clear();

      _netView = GetComponent<ZNetView>();

      if (!_netView || !_netView.IsValid()) {
        return;
      }

      PieceColorCache.Add(this);
      _cacheIndex = PieceColorCache.Count - 1;

      CacheRenderers();
    }

    void OnDestroy() {
      if (_cacheIndex >= 0 && _cacheIndex < PieceColorCache.Count) {
        PieceColorCache[_cacheIndex] = PieceColorCache[PieceColorCache.Count - 1];
        PieceColorCache[_cacheIndex]._cacheIndex = _cacheIndex;
        PieceColorCache.RemoveAt(PieceColorCache.Count - 1);
      }

      Renderers.Clear();
    }

    void CacheRenderers() {
      if (RendererCountCache.TryGetValue(gameObject.name, out int count)) {
        Renderers.Capacity = count;
      }

      Renderers.AddRange(gameObject.GetComponentsInChildren<MeshRenderer>(true));
      Renderers.AddRange(gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true));

      if (Renderers.Count != count) {
        //ZLog.Log($"Caching prefab renderer count {Renderers.Count} for: {gameObject.name}");

        RendererCountCache[gameObject.name] = Renderers.Count;
        Renderers.Capacity = Renderers.Count;
      }
    }

    public void UpdateColors() {
      if (!_netView || !_netView.IsValid()) {
        return;
      }

      if (_lastDataRevision >= _netView.m_zdo.m_dataRevision) {
        return;
      }

      _lastDataRevision = _netView.m_zdo.m_dataRevision;

      Vector3 colorVec3 = _netView.m_zdo.GetVec3(PieceColorHashCode, _noColor);
      float factor = _netView.m_zdo.GetFloat(PieceEmissionColorFactorHashCode, 0f);

      if (colorVec3 == _lastColorVec3 && factor == _lastEmissionColorFactor) {
        return;
      }

      _lastColorVec3 = colorVec3;
      _lastEmissionColorFactor = factor;

      if (colorVec3 == _noColor) {
        TargetColor = Color.clear;
        TargetEmissionColorFactor = 0f;

        ClearColors();
      } else {
        TargetColor = Utils.Vec3ToColor(colorVec3);
        TargetEmissionColorFactor = factor;

        SetColors();
      }
    }

    public void SetColors() {
      _propertyBlock.SetColor(_colorId, TargetColor);
      _propertyBlock.SetColor(_emissionColorId, TargetColor * TargetEmissionColorFactor);

      foreach (Renderer renderer in Renderers) {
        renderer.SetPropertyBlock(_propertyBlock);
      }
    }

    public void ClearColors() {
      _propertyBlock.Clear();

      foreach (Renderer renderer in Renderers) {
        renderer.SetPropertyBlock(null);
      }
    }
  }
}
