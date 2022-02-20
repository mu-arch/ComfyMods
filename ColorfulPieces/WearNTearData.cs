using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace ColorfulPieces {
  public class WearNTearData {
    public long LastDataRevision { get; set; } = -1L;
    public List<Material> Materials { get; } = new List<Material>();
    public Color TargetColor { get; set; } = Color.clear;
    public float TargetEmissionColorFactor { get; set; } = 0f;

    public WearNTearData(WearNTear wearNTear) {
      Materials.AddRange(wearNTear.GetComponentsInChildren<MeshRenderer>(true).SelectMany(r => r.materials));
      Materials.AddRange(wearNTear.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(r => r.materials));

      SaveMaterialColors();
    }

    public WearNTearData SaveMaterialColors() {
      foreach (Material material in Materials) {
        if (material.HasProperty("_Color") && !material.HasProperty("_SavedColor")) {
          material.SetColor("_SavedColor", material.GetColor("_Color"));
        }

        if (material.HasProperty("_EmissionColor") && !material.HasProperty("_SavedEmissionColor")) {
          material.SetColor("_SavedEmissionColor", material.GetColor("_EmissionColor"));
        }
      }

      return this;
    }

    public WearNTearData ClearMaterialColors() {
      foreach (Material material in Materials) {
        if (material.HasProperty("_SavedEmissionColor")) {
          material.SetColor("_EmissionColor", material.GetColor("_SavedEmissionColor"));
        }

        if (material.HasProperty("_SavedColor")) {
          material.SetColor("_Color", material.GetColor("_SavedColor"));
          material.color = material.GetColor("_SavedColor");
        }
      }

      return this;
    }
  }
}