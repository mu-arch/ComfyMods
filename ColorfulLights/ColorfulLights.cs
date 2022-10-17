using System.Collections;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulLights : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfullights";
    public const string PluginName = "ColorfulLights";
    public const string PluginVersion = "1.7.1";

    public static readonly int FirePlaceColorHashCode = "FireplaceColor".GetStableHashCode();
    public static readonly int FireplaceColorAlphaHashCode = "FireplaceColorAlpha".GetStableHashCode();
    public static readonly int LightLastColoredByHashCode = "LightLastColoredBy".GetStableHashCode();

    public static ManualLogSource PluginLogger { get; private set; }

    Harmony _harmony;

    public void Awake() {
      PluginLogger = Logger;
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static IEnumerator ChangeFireplaceColorCoroutine(Fireplace targetFireplace) {
      yield return null;

      if (!targetFireplace) {
        yield break;
      }

      if (!targetFireplace.m_nview || !targetFireplace.m_nview.IsValid()) {
        PluginLogger.LogWarning("Fireplace does not have a valid ZNetView.");
        yield break;
      }

      if (!PrivateArea.CheckAccess(targetFireplace.transform.position, radius: 0f, flash: true, wardCheck: false)) {
        PluginLogger.LogWarning("Fireplace is within private area with no access.");
        yield break;
      }

      Vector3 colorVec3 = Utils.ColorToVec3(TargetFireplaceColor.Value);
      float colorAlpha = TargetFireplaceColor.Value.a;

      targetFireplace.m_nview.ClaimOwnership();
      targetFireplace.m_nview.m_zdo.Set(FirePlaceColorHashCode, colorVec3);
      targetFireplace.m_nview.m_zdo.Set(FireplaceColorAlphaHashCode, colorAlpha);
      targetFireplace.m_nview.m_zdo.Set(LightLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());

      targetFireplace.m_fuelAddedEffects?.Create(
          targetFireplace.transform.position, targetFireplace.transform.rotation);

      if (targetFireplace.TryGetComponent(out FireplaceColor fireplaceColor)) {
        fireplaceColor.SetFireplaceColors(colorVec3, colorAlpha);
      }
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
