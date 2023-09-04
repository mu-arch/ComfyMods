using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using ComfyLib;

using HarmonyLib;

using UnityEngine;

using static ColorfulWards.PluginConfig;

namespace ColorfulWards {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulWards : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfulwards";
    public const string PluginName = "ColorfulWards";
    public const string PluginVersion = "1.6.0";

    public static readonly Color NoColor = new(-1f, -1f, -1f);
    public static readonly Vector3 NoColorVector3 = new(-1f, -1f, -1f);
    public static readonly Vector3 BlackColorVector3 = new(0.00012345f, 0.00012345f, 0.00012345f);

    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");

    public static readonly int PrivateAreaColorHashCode = "PrivateAreaColor".GetStableHashCode();
    public static readonly int PrivateAreaColorAlphaHashCode = "PrivateAreaColorAlpha".GetStableHashCode();
    public static readonly int WardLastColoredByHashCode = "WardLastColoredBy".GetStableHashCode();
    public static readonly int WardLastColoredByHostHashCode = "WardLastColoredByHost".GetStableHashCode();

    static ManualLogSource _logger;
    Harmony _harmony;

    void Awake() {
      _logger = Logger;

      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static void ChangeWardColor(PrivateArea targetWard) {
      if (!targetWard) {
        return;
      }

      if (!targetWard.m_nview || !targetWard.m_nview.IsValid()) {
        _logger.LogWarning("PrivateArea does not have a valid ZNetView.");
        return;
      }

      if (!targetWard.m_piece.IsCreator()) {
        _logger.LogWarning("You are not the owner of this Ward.");
        return;
      }

      targetWard.m_nview.ClaimOwnership();
      SetPrivateAreaColorZDOValues(targetWard.m_nview.m_zdo, TargetWardColor.Value);

      targetWard.m_flashEffect?.Create(targetWard.transform.position, targetWard.transform.rotation);

      if (targetWard.TryGetComponent(out PrivateAreaColor privateAreaColor)) {
        privateAreaColor.UpdateColors(true);
      }
    }

    static void SetPrivateAreaColorZDOValues(ZDO zdo, Color targetWardColor) {
      zdo.Set(PrivateAreaColorHashCode, ColorToVector3(targetWardColor));
      zdo.Set(PrivateAreaColorAlphaHashCode, targetWardColor.a);
      zdo.Set(WardLastColoredByHashCode, Player.m_localPlayer.Ref()?.GetPlayerID() ?? 0L);
      zdo.Set(WardLastColoredByHostHashCode, PrivilegeManager.GetNetworkUserId());
    }

    public static Vector3 ColorToVector3(Color color) {
      return color == Color.black ? BlackColorVector3 : new(color.r, color.g, color.b);
    }

    public static Color Vector3ToColor(Vector3 vector3) {
      return vector3 == BlackColorVector3 ? Color.black : new(vector3.x, vector3.y, vector3.z);
    }
  }
}
