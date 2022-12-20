using UnityEngine;

namespace ComfyLib {
  public class ParticleSystemSetting {
    readonly ParticleSystem _particleSystem;

    public ParticleSystemSetting(ParticleSystem particleSystem) {
      _particleSystem = particleSystem;
    }

    public ParticleSystemSetting SetActive(bool active) {
      _particleSystem.gameObject.SetActive(active);
      return this;
    }
  }
}
