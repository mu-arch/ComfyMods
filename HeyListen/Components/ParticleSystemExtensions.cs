using UnityEngine;

namespace ComfyLib {
  public static class ParticleSystemExtensions {
    public static ParticleSystem.ColorOverLifetimeModule ColorOverLifetime(this ParticleSystem particleSystem) {
      return particleSystem.colorOverLifetime;
    }

    public static ParticleSystem.ColorOverLifetimeModule SetColor(
        this ParticleSystem.ColorOverLifetimeModule colorOverLifetime, ParticleSystem.MinMaxGradient color) {
      colorOverLifetime.color = color;
      return colorOverLifetime;
    }
  }
}
