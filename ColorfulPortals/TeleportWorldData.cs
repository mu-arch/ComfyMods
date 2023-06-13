using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ColorfulPortals {
  public class TeleportWorldData {
    public List<Light> Lights { get; } = new List<Light>();
    public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
    public List<Material> Materials { get; } = new List<Material>();
    public Color TargetColor = Color.clear;

    public TeleportWorldData(TeleportWorld teleportWorld) {
      Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

      Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("suck particles"));
      Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System"));

      Materials.AddRange(
          teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("blue flames")
              .Where(psr => psr.material != null)
              .Select(psr => psr.material));
    }
  }
}
