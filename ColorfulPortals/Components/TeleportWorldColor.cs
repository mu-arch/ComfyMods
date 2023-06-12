using ComfyLib;

using UnityEngine;

using static ColorfulPortals.ColorfulPortals;

namespace ColorfulPortals {
  public class TeleportWorldColor : MonoBehaviour {
    TeleportWorld _teleportWorld;
    ZNetView _netView;
    Color _currentColor;
    long _lastDataRevision;

    Light _pointLight;
    ParticleSystem _suckParticles;
    ParticleSystem _particleSystem;
    Renderer _blueFlames;
    MaterialPropertyBlock _propertyBlock;

    void Awake() {
      _teleportWorld = GetComponent<TeleportWorld>();

      if (!_teleportWorld || !_teleportWorld.m_nview || !_teleportWorld.m_nview.IsValid()) {
        return;
      }

      _netView = _teleportWorld.m_nview;

      _currentColor = NoColor;
      _lastDataRevision = -1L;

      _pointLight = transform.Find("_target_found_red/Point light").GetComponent<Light>();
      _suckParticles = transform.Find("_target_found_red/suck particles").GetComponent<ParticleSystem>();
      _particleSystem = transform.Find("_target_found_red/Particle System").GetComponent<ParticleSystem>();
      _blueFlames = transform.Find("_target_found_red/blue flames").GetComponent<ParticleSystemRenderer>();
      _propertyBlock = new();
    }

    public void UpdatePortalColors(bool forceUpdate = false) {
      if (!_netView || !_netView.IsValid()) {
        return;
      }

      long dataRevision = _netView.m_zdo.DataRevision;

      if (!forceUpdate && _lastDataRevision >= dataRevision) {
        return;
      }

      _lastDataRevision = dataRevision;

      if (!_netView.m_zdo.TryGetVector3(TeleportWorldColorHashCode, out Vector3 colorVector3)) {
        return;
      }

      Color portalColor = new(colorVector3.x, colorVector3.y, colorVector3.z);

      if (_netView.m_zdo.TryGetFloat(TeleportWorldColorAlphaHashCode, out float colorAlpha)) {
        portalColor.a = colorAlpha;
      }

      if (portalColor == _currentColor) {
        return;
      }

      SetPortalColors(portalColor);
    }

    public void SetPortalColors(Color portalColor) {
      _currentColor = portalColor;
      _pointLight.color = portalColor;

      ParticleSystem.ColorOverLifetimeModule colorOverLifetime = _suckParticles.colorOverLifetime;
      colorOverLifetime.color = new ParticleSystem.MinMaxGradient(portalColor);

      ParticleSystem.MainModule main = _suckParticles.main;
      main.startColor = portalColor;

      colorOverLifetime = _particleSystem.colorOverLifetime;
      colorOverLifetime.color = new ParticleSystem.MinMaxGradient(portalColor);

      main = _particleSystem.main;
      main.startColor = portalColor;

      _blueFlames.GetPropertyBlock(_propertyBlock);
      _propertyBlock.SetColor(ColorShaderId, portalColor);
      _blueFlames.SetPropertyBlock(_propertyBlock);
    }
  }
}
