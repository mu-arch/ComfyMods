using ComfyLib;

using System;
using System.Collections;

using UnityEngine;

using static HeyListen.HeyListen;
using static HeyListen.PluginConfig;

namespace HeyListen {
  public class DemisterBallControl : MonoBehaviour {
    ZNetView _netView;
    long _lastDataRevision;
    Vector3 _lastBodyColorVec;
    float _lastBodyBrightness;
    Vector3 _lastPointLightColorVec;

    RendererSetting _demisterBallRenderer;
    LightSetting _effectsPointLight;
    ParticleSystemSetting _flameEffectsFlames;
    ParticleSystemSetting _flameEffectsFlamesLocal;
    ParticleSystemSetting _flameEffectsFlare;
    ParticleSystemSetting _flameEffectsEmbers;
    ParticleSystemSetting _flameEffectsDistortion;
    ParticleSystemSetting _flameEffectsEnergy;
    ParticleSystemSetting _flameEffectsEnergy2;
    ParticleSystemSetting _flameEffectsSparcsFront;

    void Awake() {
      _lastDataRevision = -1L;
      _lastBodyColorVec = -Vector3.one;
      _lastBodyBrightness = -1f;
      _lastPointLightColorVec = -Vector3.one;

      _demisterBallRenderer = new(transform.Find("demister_ball").GetComponent<MeshRenderer>());
      _effectsPointLight = new(transform.Find("effects/Point light").GetComponent<Light>());

      _flameEffectsFlames = new(transform.Find("effects/flame/flames").GetComponent<ParticleSystem>());
      _flameEffectsFlamesLocal = new(transform.Find("effects/flame/flames_local").GetComponent<ParticleSystem>());
      _flameEffectsFlare = new(transform.Find("effects/flame/flare").GetComponent<ParticleSystem>());
      _flameEffectsEmbers = new(transform.Find("effects/flame/embers").GetComponent<ParticleSystem>());
      _flameEffectsDistortion = new(transform.Find("effects/flame/distortiion").GetComponent<ParticleSystem>());
      _flameEffectsEnergy = new(transform.Find("effects/flame/energy").GetComponent<ParticleSystem>());
      _flameEffectsEnergy2 = new(transform.Find("effects/flame/energy (1)").GetComponent<ParticleSystem>());
      _flameEffectsSparcsFront = new(transform.Find("effects/flame/sparcs_front").GetComponent<ParticleSystem>());

      ZLog.Log($"Renderer original emissionColor is: {_demisterBallRenderer.OriginalEmissionColor}");
      ZLog.Log($"Point light original color is: {_effectsPointLight.OriginalColor}");

      _netView = GetComponent<ZNetView>();

      if (!_netView || !_netView.IsValid()) {
        return;
      }

      StartCoroutine(UpdateDemisterBallCoroutine());
    }

    IEnumerator UpdateDemisterBallCoroutine() {
      ZLog.Log($"Starting UpdateDemisterBallCoroutine.");
      WaitForSeconds waitInterval = new(seconds: 2f);

      while (_netView && _netView.IsValid()) {
        UpdateDemisterBall(forceUpdate: false);
        yield return waitInterval;
      }
    }

    public void UpdateDemisterBall(bool forceUpdate = false) {
      if (!_netView || !_netView.IsValid() || !IsModEnabled.Value) {
        return;
      }

      if (!forceUpdate && _lastDataRevision >= _netView.m_zdo.m_dataRevision) {
        return;
      }

      _lastDataRevision = _netView.m_zdo.m_dataRevision;

      UpdateBodyColor(forceUpdate);
      UpdatePointLightColor(forceUpdate);
    }

    void UpdateBodyColor(bool forceUpdate = false) {
      Vector3 colorVec = _netView.m_zdo.GetVec3(DemisterBallBodyColorHashCode, -Vector3.one);
      float brightness = Mathf.Clamp(_netView.m_zdo.GetFloat(DemisterBallBodyBrightnessHashCode, -1f), -1f, 1f);

      if (!forceUpdate && colorVec == _lastBodyColorVec && brightness == _lastBodyBrightness) {
        return;
      }

      if (brightness >= 0f && colorVec != -Vector3.one) {
        Color color = Utils.Vec3ToColor(colorVec);
        color.a = brightness;

        _demisterBallRenderer.SetEmissionColor(color);
      } else {
        _demisterBallRenderer.SetEmissionColor(_demisterBallRenderer.OriginalEmissionColor);
      }

      _lastBodyColorVec = colorVec;
      _lastBodyBrightness = brightness;
    }

    void UpdatePointLightColor(bool forceUpdate) {
      Vector3 colorVec = _netView.m_zdo.GetVec3(DemisterBallPointLightColorHashCode, -Vector3.one);

      if (!forceUpdate && colorVec == _lastPointLightColorVec) {
        return;
      }

      _effectsPointLight.SetColor(
          colorVec == -Vector3.one ? _effectsPointLight.OriginalColor : Utils.Vec3ToColor(colorVec));

      _lastPointLightColorVec = colorVec;
    }

    public void UpdateFlameEffects(bool forceUpdate) {
      FlameEffects effects = DemisterBallFlameEffectsEnabled.Value;

      // ColorOverLifetime
      _flameEffectsFlames.SetActive(effects.HasFlag(FlameEffects.Flames));
      _flameEffectsFlamesLocal.SetActive(effects.HasFlag(FlameEffects.FlamesLocal));

      // ParticleSystem.main.startColor: keep alpha to 0.1 or less
      _flameEffectsFlare.SetActive(effects.HasFlag(FlameEffects.Flare));

      // ParticleSystemRenderer.material._EmissionColor: drives this color
      _flameEffectsEmbers.SetActive(effects.HasFlag(FlameEffects.Embers));
      _flameEffectsDistortion.SetActive(effects.HasFlag(FlameEffects.Distortion));
      _flameEffectsEnergy.SetActive(effects.HasFlag(FlameEffects.Energy));

      // ParticleSystem.main.startColor: keep alpha to 0.1 or less
      _flameEffectsEnergy2.SetActive(effects.HasFlag(FlameEffects.Energy2));

      // ParticleSystemRenderer.material._EmissionColor: drives this color
      _flameEffectsSparcsFront.SetActive(effects.HasFlag(FlameEffects.SparcsFront));
    }
  }
}
