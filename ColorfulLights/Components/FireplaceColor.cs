using System.Collections.Generic;

using UnityEngine;

using static ColorfulLights.ColorfulLights;
using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  public class FireplaceColor : MonoBehaviour {
    public static long FireplaceColorTotalCount { get; private set; } = 0L;
    public static long FireplaceColorCount { get; private set; } = 0L;

    private ZNetView _netView;

    private Vector3 _targetColorVec3;
    private float _targetColorAlpha;

    private readonly List<Light> _lights = new();
    private readonly List<ParticleSystem> _systems = new();
    private readonly List<ParticleSystemRenderer> _renderers = new();

    public Color TargetColor {
      get {
        Color color = Utils.Vec3ToColor(_targetColorVec3);
        color.a = _targetColorAlpha;

        return color;
      }
    }

    private void Awake() {
      FireplaceColorTotalCount++;
      FireplaceColorCount++;

      _targetColorVec3 = Vector3.positiveInfinity;
      _targetColorAlpha = float.NaN;

      _lights.Clear();
      _systems.Clear();
      _renderers.Clear();

      Fireplace fireplace = GetComponent<Fireplace>();

      if (!fireplace || !fireplace.m_nview || !fireplace.m_nview.IsValid()) {
        return;
      }

      _netView = fireplace.m_nview;

      CacheComponents(fireplace.m_enabledObject);
      CacheComponents(fireplace.m_enabledObjectHigh);
      CacheComponents(fireplace.m_enabledObjectLow);
      CacheComponents(fireplace.m_fireworks);

      InvokeRepeating(nameof(UpdateColors), 0f, 2f);
    }

    private void OnDestroy() {
      FireplaceColorCount--;
    }

    private void CacheComponents(GameObject target) {
      if (target) {
        _lights.AddRange(target.GetComponentsInChildren<Light>(includeInactive: true));
        _systems.AddRange(target.GetComponentsInChildren<ParticleSystem>(includeInactive: true));
        _renderers.AddRange(target.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true));
      }
    }

    public void UpdateColors() {
      if (!_netView || !_netView.IsValid() || !IsModEnabled.Value) {
        CancelInvoke(nameof(UpdateColors));
        return;
      }

      if (_netView.m_zdo.m_vec3 == null
          || !_netView.m_zdo.m_vec3.TryGetValue(FirePlaceColorHashCode, out Vector3 colorVec3)) {
        return;
      }

      float colorAlpha = _netView.m_zdo.GetFloat(FireplaceColorAlphaHashCode, 1f);

      if (colorVec3 == _targetColorVec3 && colorAlpha == _targetColorAlpha) {
        return;
      }

      SetColors(colorVec3, colorAlpha);
    }

    public void SetColors(Vector3 colorVec3, float colorAlpha) {
      _targetColorVec3 = colorVec3;
      _targetColorAlpha = colorAlpha;

      SetColors(TargetColor);
    }

    private void SetColors(Color color) {
      ParticleSystem.MinMaxGradient gradient = new(color);

      foreach (ParticleSystem system in _systems) {
        ParticleSystem.ColorOverLifetimeModule colorOverLiftime = system.colorOverLifetime;

        if (colorOverLiftime.enabled) {
          colorOverLiftime.color = gradient;
        }

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = system.sizeOverLifetime;

        if (sizeOverLifetime.enabled) {
          ParticleSystem.MainModule main = system.main;
          main.startColor = color;
        }
      }

      foreach (ParticleSystemRenderer renderer in _renderers) {
        renderer.material.color = color;
      }

      foreach (Light light in _lights) {
        light.color = color;
      }
    }
  }
}
