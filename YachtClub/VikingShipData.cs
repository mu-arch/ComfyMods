using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YachtClub {
  class VikingShipData {
    private readonly string[] _hullMaterialNames = {
      "ship/visual/hull_new/hull",
      "ship/visual/hull_new/plank",
      "ship/visual/hull_new/plank (1)",
      "ship/visual/hull_worn/hull",
      "ship/visual/hull_worn/plank",
      "ship/visual/hull_worn/plank (1)",
      "ship/visual/hull_broken/hull",
      "ship/visual/hull_broken/plank",
      "ship/visual/hull_broken/plank (1)",
    };

    List<Material> HullMaterials { get; } = new List<Material>();
    Color HullColor { get; set; } = Color.clear;
    float HullEmissionFactor { get; set; } = 0f;

    internal VikingShipData(Ship ship) {
      Transform rootTransform = ship.transform;

      HullMaterials.AddRange(GetChildMaterials(rootTransform, _hullMaterialNames));
    }

    private static IEnumerable<Material> GetChildMaterials(Transform rootTransform, IEnumerable<string> names) {
      return names
          .Select(rootTransform.Find)
              .Where(transform => transform != null)
              .Select(transform => transform.GetComponent<MeshRenderer>())
              .Where(renderer => renderer != null)
              .Select(renderer => renderer.material)
              .Where(material => material != null);
    }
  }
}
