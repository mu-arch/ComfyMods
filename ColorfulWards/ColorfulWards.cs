using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static ColorfulWards.PluginConfig;

namespace ColorfulWards {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulWards : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfulwards";
    public const string PluginName = "ColorfulWards";
    public const string PluginVersion = "1.4.1";

    static readonly Dictionary<PrivateArea, PrivateAreaData> PrivateAreaDataCache = new();

    static readonly int PrivateAreaColorHashCode = "PrivateAreaColor".GetStableHashCode();
    static readonly int PrivateAreaColorAlphaHashCode = "PrivateAreaColorAlpha".GetStableHashCode();
    static readonly int WardLastColoredByHashCode = "WardLastColoredBy".GetStableHashCode();

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static IEnumerator ChangeWardColorCoroutine(PrivateArea targetWard) {
      yield return null;

      if (!targetWard) {
        yield break;
      }

      if (!targetWard.m_nview || !targetWard.m_nview.IsValid()) {
        _logger.LogWarning("PrivateArea does not have a valid ZNetView.");
        yield break;
      }

      if (!targetWard.m_piece.IsCreator()) {
        _logger.LogWarning("You are not the owner of this Ward.");
        yield break;
      }

      targetWard.m_nview.ClaimOwnership();

      targetWard.m_nview.m_zdo.Set(PrivateAreaColorHashCode, Utils.ColorToVec3(TargetWardColor.Value));
      targetWard.m_nview.m_zdo.Set(PrivateAreaColorAlphaHashCode, TargetWardColor.Value.a);
      targetWard.m_nview.m_zdo.Set(WardLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

      targetWard.m_flashEffect?.Create(targetWard.transform.position, targetWard.transform.rotation);

      if (PrivateAreaDataCache.TryGetValue(targetWard, out PrivateAreaData privateAreaData)) {
        privateAreaData.TargetColor = TargetWardColor.Value;
        SetPrivateAreaColors(targetWard, privateAreaData);
      }

      targetWard.StartCoroutine(UpdateEnabledEffect(targetWard));
    }

    static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    static IEnumerator UpdateEnabledEffect(PrivateArea privateArea) {
      if (privateArea.IsEnabled()) {
        privateArea.m_enabledEffect.SetActive(false);
        yield return WaitForEndOfFrame;
        privateArea.m_enabledEffect.SetActive(true);
      }
    }

    [HarmonyPatch(typeof(PrivateArea))]
    class PrivateAreaPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(PrivateArea.IsInside))]
      static bool PrivateAreaIsInside(
          ref PrivateArea __instance, ref bool __result, Vector3 point, float radius) {
        if (!IsModEnabled.Value || !UseRadiusForVerticalCheck.Value) {
          return true;
        }

        __result = Vector3.Distance(__instance.transform.position, point) < __instance.m_radius + radius;
        return false;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(PrivateArea.Awake))]
      static void PrivateAreaAwakePostfix(ref PrivateArea __instance) {
        if (!IsModEnabled.Value || !__instance) {
          return;
        }

        PrivateAreaDataCache.Add(__instance, new PrivateAreaData(__instance));
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(PrivateArea.OnDestroy))]
      static void PrivateAreaOnDestroyPrefix(ref PrivateArea __instance) {
        PrivateAreaDataCache.Remove(__instance);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(PrivateArea.UpdateStatus))]
      static void PrivateAreaUpdateStatusPostfix(ref PrivateArea __instance) {
        if (!IsModEnabled.Value
            || !__instance
            || !__instance.m_nview
            || __instance.m_nview.m_zdo == null
            || __instance.m_nview.m_zdo.m_zdoMan == null
            || __instance.m_nview.m_zdo.m_vec3 == null
            || !__instance.m_nview.m_zdo.m_vec3.ContainsKey(PrivateAreaColorHashCode)
            || !PrivateAreaDataCache.TryGetValue(__instance, out PrivateAreaData privateAreaData)) {
          return;
        }

        Color wardColor = Utils.Vec3ToColor(__instance.m_nview.m_zdo.m_vec3[PrivateAreaColorHashCode]);
        wardColor.a = __instance.m_nview.m_zdo.GetFloat(PrivateAreaColorAlphaHashCode, defaultValue: 1f);

        privateAreaData.TargetColor = wardColor;
        SetPrivateAreaColors(__instance, privateAreaData);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(PrivateArea.GetHoverText))]
      static void PrivateAreaGetHoverText(ref PrivateArea __instance, ref string __result) {
        if (!IsModEnabled.Value
            || !ShowChangeColorHoverText.Value
            || !__instance
            || !__instance.m_piece
            || !__instance.m_piece.IsCreator()) {
          return;
        }

        __result =
            string.Format(
                "{0}\n[<color={1}>{2}</color>] Change ward color to: <color=#{3}>#{3}</color>",
                __result,
                "#FFA726",
                ChangeWardColorShortcut.Value,
                GetColorHtmlString(TargetWardColor.Value));
      }
    }

    static void SetPrivateAreaColors(PrivateArea privateArea, PrivateAreaData privateAreaData) {
      foreach (Light light in privateAreaData.PointLight) {
        light.color = privateAreaData.TargetColor;
      }

      foreach (Material material in privateAreaData.GlowMaterial) {
        material.SetColor("_EmissionColor", privateAreaData.TargetColor);
      }

      foreach (ParticleSystem system in privateAreaData.SparcsSystem) {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(privateAreaData.TargetColor);

        ParticleSystem.MainModule main = system.main;
        main.startColor = privateAreaData.TargetColor;
      }

      foreach (ParticleSystemRenderer renderer in privateAreaData.SparcsRenderer) {
        renderer.material.color = privateAreaData.TargetColor;
      }

      foreach (ParticleSystem system in privateAreaData.FlareSystem) {
        Color flareColor = privateAreaData.TargetColor;
        flareColor.a = 0.1f;

        ParticleSystem.MainModule main = system.main;
        main.startColor = flareColor;
      }
    }
  }
}
