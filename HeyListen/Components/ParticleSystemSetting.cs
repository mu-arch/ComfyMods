using UnityEngine;

namespace ComfyLib {
  public class ParticleSystemSetting {
    public ParticleSystem.MinMaxGradient OriginalColorOveLifetimeColor { get; }
    public ParticleSystem.MinMaxGradient CurrentColorOverLifetimeColor { get; private set; }

    readonly ParticleSystem _particleSystem;

    public ParticleSystemSetting(ParticleSystem particleSystem) {
      _particleSystem = particleSystem;

      ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
      OriginalColorOveLifetimeColor = colorOverLifetime.color;
      CurrentColorOverLifetimeColor = colorOverLifetime.color;
    }

    public ParticleSystemSetting SetActive(bool active) {
      _particleSystem.gameObject.SetActive(active);
      return this;
    }

    public ParticleSystemSetting SetColorOverLifetimeColor(Color color) {
      CurrentColorOverLifetimeColor = new(color);
      _particleSystem.ColorOverLifetime().SetColor(CurrentColorOverLifetimeColor);

      return this;
    }
  }
}
