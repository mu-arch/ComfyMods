using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ColorfulWards {
  public class PrivateAreaData {
    public List<Light> PointLight { get; } = new();
    public List<Renderer> GlowMaterial { get; } = new();
    public List<ParticleSystem> SparcsSystem { get; } = new();
    public List<ParticleSystemRenderer> SparcsRenderer { get; } = new();
    public List<ParticleSystem> FlareSystem { get; } = new();

    public Color TargetColor { get; set; } = Color.clear;

    public PrivateAreaData(PrivateArea privateArea) {
      PointLight.AddRange(FindChildren(privateArea, "Point light").Select(go => go.GetComponent<Light>()));
      GlowMaterial.AddRange(FindChildren(privateArea, "default").Select(go => go.GetComponent<Renderer>()));

      foreach (GameObject sparcsObject in FindChildren(privateArea, "sparcs")) {
        SparcsSystem.Add(sparcsObject.GetComponent<ParticleSystem>());
        SparcsRenderer.Add(sparcsObject.GetComponent<ParticleSystemRenderer>());
      }

      FlareSystem.AddRange(FindChildren(privateArea, "flare").Select(go => go.GetComponent<ParticleSystem>()));
    }

    private IEnumerable<GameObject> FindChildren(PrivateArea privateArea, string name) {
      return privateArea
          .GetComponentsInChildren<Transform>()
          .Where(t => t.name == name && t.gameObject != null)
          .Select(t => t.gameObject);
    }
  }
}
