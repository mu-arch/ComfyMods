using UnityEngine;

namespace ColorfulPieces {
  public static class PluginConstants {
    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");

    public static readonly Vector3 NoColorVector3 = new(-1f, -1f, -1f);
    public static readonly float NoEmissionColorFactor = -1f;
  }
}
