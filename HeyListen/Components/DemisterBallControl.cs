using ComfyLib;

using System.Collections;

using UnityEngine;

using static HeyListen.HeyListen;
using static HeyListen.PluginConfig;

namespace HeyListen {
  public class DemisterBallControl : MonoBehaviour {
    public static readonly Color NoColor = new(-1f, -1f, -1f, -1f);

    ZNetView _netView;
    long _lastDataRevision;
    float _lastBodyScale;
    Color _lastBodyColor;
    float _lastBodyBrightness;
    Color _lastPointLightColor;

    RendererSetting _demisterBallRenderer;
    LightSetting _effectsPointLight;
    ParticleSystemSetting _flameEffectsFlames;
    ParticleSystemSetting _flameEffectsFlamesLocal;
    ParticleSystemSetting _flameEffectsFlare;
    RendererSetting _flameEffectsEmbers;
    RendererSetting _flameEffectsDistortion;
    RendererSetting _flameEffectsEnergy;
    ParticleSystemSetting _flameEffectsEnergy2;
    RendererSetting _flameEffectsSparcs;

    void Awake() {
      _lastDataRevision = -1L;
      _lastBodyScale = -1f;
      _lastBodyColor = NoColor;
      _lastBodyBrightness = -1f;
      _lastPointLightColor = NoColor;

      _demisterBallRenderer = new(transform.Find("demister_ball").GetComponent<MeshRenderer>());
      _effectsPointLight = new(transform.Find("effects/Point light").GetComponent<Light>());

      _flameEffectsFlames = new(transform.Find("effects/flame/flames").GetComponent<ParticleSystem>());
      _flameEffectsFlamesLocal = new(transform.Find("effects/flame/flames_local").GetComponent<ParticleSystem>());
      _flameEffectsFlare = new(transform.Find("effects/flame/flare").GetComponent<ParticleSystem>());
      _flameEffectsEmbers = new(transform.Find("effects/flame/embers").GetComponent<ParticleSystemRenderer>());
      _flameEffectsDistortion = new(transform.Find("effects/flame/distortiion").GetComponent<ParticleSystemRenderer>());
      _flameEffectsEnergy = new(transform.Find("effects/flame/energy").GetComponent<ParticleSystemRenderer>());
      _flameEffectsEnergy2 = new(transform.Find("effects/flame/energy (1)").GetComponent<ParticleSystem>());
      _flameEffectsSparcs = new(transform.Find("effects/flame/sparcs_front").GetComponent<ParticleSystemRenderer>());      

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

      UpdateBodyScale(forceUpdate);
      UpdateBodyColor(forceUpdate);
      UpdatePointLightColor(forceUpdate);
    }

    void UpdateBodyScale(bool forceUpdate = false) {
      float scale = _netView.m_zdo.GetFloat(DemisterBallBodyScaleHashCode, -1f);

      if (!forceUpdate && scale == _lastBodyScale) {
        return;
      }

      _lastBodyScale = scale;
      Vector3 localScale = Vector3.one * scale;

      transform.localScale = localScale;
      _flameEffectsFlames.SetScale(localScale);
      _flameEffectsFlamesLocal.SetScale(localScale);
      _flameEffectsEmbers.SetScale(localScale);
    }

    void UpdateBodyColor(bool forceUpdate = false) {
      Color color = _netView.m_zdo.GetColor(DemisterBallBodyColorHashCode, NoColor);
      float brightness = Mathf.Clamp(_netView.m_zdo.GetFloat(DemisterBallBodyBrightnessHashCode, -1f), -1f, 2f);

      if (!forceUpdate && color == _lastBodyColor && brightness == _lastBodyBrightness) {
        return;
      }

      if (brightness >= 0f && color != NoColor) {
        _demisterBallRenderer.SetColor(color);
        _demisterBallRenderer.SetEmissionColor(color * new Color(brightness, brightness, brightness, 1f));
      } else {
        _demisterBallRenderer.SetColor(_demisterBallRenderer.OriginalColor);
        _demisterBallRenderer.SetEmissionColor(_demisterBallRenderer.OriginalEmissionColor);
      }

      _lastBodyColor = color;
      _lastBodyBrightness = brightness;
    }

    void UpdatePointLightColor(bool forceUpdate) {
      Color color = _netView.m_zdo.GetColor(DemisterBallPointLightColorHashCode, NoColor);

      if (!forceUpdate && color == _lastPointLightColor) {
        return;
      }

      _effectsPointLight.SetColor(color == NoColor ? _effectsPointLight.OriginalColor : color);
      _lastPointLightColor = color;
    }

    public void UpdateFlameEffects(
        FlameEffects effectsEnabled,
        Color effectsColor,
        Color embersColor,
        float embersBrightness,
        Color sparcsColor,
        float sparcsBrightness) {
      FlameEffects effects = DemisterBallFlameEffectsEnabled.Value;

      // ParticleSystem.ColorOverLifetime.color
      _flameEffectsFlames.SetActive(effects.HasFlag(FlameEffects.Flames));
      _flameEffectsFlames.SetColorOverLifetimeColor(effectsColor);

      // ParticleSystem.ColorOverLifetime.color
      _flameEffectsFlamesLocal.SetActive(effects.HasFlag(FlameEffects.FlamesL));
      _flameEffectsFlamesLocal.SetColorOverLifetimeColor(effectsColor);

      // ParticleSystem.main.startColor: keep alpha to 0.1 or less
      _flameEffectsFlare.SetActive(effects.HasFlag(FlameEffects.Flare));
      _flameEffectsFlare.SetStartColor(effectsColor.SetAlpha(0.1f)); // <--

      // ParticleSystemRenderer.material._EmissionColor: drives this color
      _flameEffectsEmbers
          .SetActive(effects.HasFlag(FlameEffects.Embers))
          .SetColor(embersColor)
          .SetEmissionColor(embersColor * new Color(embersBrightness, embersBrightness, embersBrightness, 1f));

      // ParticleSystemRenderer.material._EmissionColor: drives this color
      _flameEffectsDistortion.SetActive(effects.HasFlag(FlameEffects.Distortion));
      _flameEffectsEnergy.SetActive(effects.HasFlag(FlameEffects.Energy));

      // ParticleSystem.main.startColor: keep alpha to 0.1 or less
      _flameEffectsEnergy2.SetActive(effects.HasFlag(FlameEffects.EnergyII));
      _flameEffectsEnergy2.SetStartColor(effectsColor.SetAlpha(0.1f)); // <--

      // ParticleSystemRenderer.material._EmissionColor: drives this color
      _flameEffectsSparcs
          .SetActive(effects.HasFlag(FlameEffects.SparcsF))
          .SetColor(sparcsColor)
          .SetEmissionColor(sparcsColor * new Color(sparcsBrightness, sparcsBrightness, sparcsBrightness, 1f));
    }
  }
}
