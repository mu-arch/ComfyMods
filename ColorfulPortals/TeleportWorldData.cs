using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ColorfulPortals {
  public class TeleportWorldData {
    public List<Light> Lights { get; } = new List<Light>();
    public List<ParticleSystem> Systems { get; } = new List<ParticleSystem>();
    public List<Material> Materials { get; } = new List<Material>();
    public Color TargetColor = Color.clear;

    static bool _hasLogged = false;

    public TeleportWorldData(TeleportWorld teleportWorld) {
      Lights.AddRange(teleportWorld.GetComponentsInNamedChild<Light>("Point light"));

      Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("suck particles"));
      Systems.AddRange(teleportWorld.GetComponentsInNamedChild<ParticleSystem>("Particle System"));

      Materials.AddRange(
          teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("blue flames")
              .Where(psr => psr.material != null)
              .Select(psr => psr.material));

      if (!_hasLogged) {
        _hasLogged = true;
        foreach (Light light in Lights) {
          ZLog.Log(GetGameObjectPath(light.gameObject));
        }

        foreach(var ps in Systems) {
          ZLog.Log(ps.gameObject);
        }

        foreach(var m in teleportWorld.GetComponentsInNamedChild<ParticleSystemRenderer>("blue flames")) {
          ZLog.Log($"M: {m.gameObject}");
        }
      }
    }

    public static string GetGameObjectPath(GameObject obj) {
      string path = "/" + obj.name;
      while (obj.transform.parent != null) {
        obj = obj.transform.parent.gameObject;
        path = "/" + obj.name + path;
      }
      return path;
    }
  }
}
