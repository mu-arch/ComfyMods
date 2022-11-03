using System.Collections.Generic;

using UnityEngine;

using static ColorfulLights.ColorfulLights;
using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  public class FireplaceColor : MonoBehaviour {
    public static long TotalCount { get; private set; } = 0L;
    public static long CurrentCount { get; private set; } = 0L;

    public Color TargetColor { get; private set; } = Color.clear;

    private Vector3 _targetColorVec3;
    private float _targetColorAlpha;

    private ZNetView _netView;

    private readonly List<Light> _lights = new();
    private readonly List<ParticleSystem> _systems = new();
    private readonly List<ParticleSystemRenderer> _renderers = new();

    private void Awake() {
      TotalCount++;
      CurrentCount++;

      TargetColor = Color.clear;

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
      CurrentCount--;
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

      SetFireplaceColors(colorVec3, colorAlpha);
    }

    public void SetFireplaceColors(Vector3 colorVec3, float colorAlpha) {
      TargetColor = Utils.Vec3ToColor(colorVec3).SetAlpha(colorAlpha);

      _targetColorVec3 = colorVec3;
      _targetColorAlpha = colorAlpha;

      SetParticleColors( _lights, _systems, _renderers, TargetColor);
    }

    public static void SetParticleColors(
        IEnumerable<Light> lights,
        IEnumerable<ParticleSystem> systems,
        IEnumerable<ParticleSystemRenderer> renderers,
        Color color) {
      ParticleSystem.MinMaxGradient gradient = new(color);

      foreach (ParticleSystem system in systems) {
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

      foreach (ParticleSystemRenderer renderer in renderers) {
        renderer.material.color = color;
      }

      foreach (Light light in lights) {
        light.color = color;
      }
    }
  }
}
