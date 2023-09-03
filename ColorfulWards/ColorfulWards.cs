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
    public const string PluginVersion = "1.6.0";

    public static readonly Dictionary<PrivateArea, PrivateAreaData> PrivateAreaDataCache = new();

    public static readonly int PrivateAreaColorHashCode = "PrivateAreaColor".GetStableHashCode();
    public static readonly int PrivateAreaColorAlphaHashCode = "PrivateAreaColorAlpha".GetStableHashCode();
    public static readonly int WardLastColoredByHashCode = "WardLastColoredBy".GetStableHashCode();

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

      //if (!targetWard.m_piece.IsCreator()) {
      //  _logger.LogWarning("You are not the owner of this Ward.");
      //  return;
      //}

      targetWard.m_nview.ClaimOwnership();

      targetWard.m_nview.m_zdo.Set(PrivateAreaColorHashCode, Utils.ColorToVec3(TargetWardColor.Value));
      targetWard.m_nview.m_zdo.Set(PrivateAreaColorAlphaHashCode, TargetWardColor.Value.a);
      targetWard.m_nview.m_zdo.Set(WardLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

      targetWard.m_flashEffect?.Create(targetWard.transform.position, targetWard.transform.rotation);

      if (PrivateAreaDataCache.TryGetValue(targetWard, out PrivateAreaData privateAreaData)) {
        privateAreaData.TargetColor = TargetWardColor.Value;
        SetPrivateAreaColors(targetWard, privateAreaData);
      }

      //targetWard.StartCoroutine(UpdateEnabledEffect(targetWard));
    }

    //static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    //static IEnumerator UpdateEnabledEffect(PrivateArea privateArea) {
    //  if (privateArea.IsEnabled()) {
    //    privateArea.m_enabledEffect.SetActive(false);
    //    yield return WaitForEndOfFrame;
    //    privateArea.m_enabledEffect.SetActive(true);
    //  }
    //}

    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");
    static readonly MaterialPropertyBlock _propertyBlock = new();

    public static void SetPrivateAreaColors(PrivateArea privateArea, PrivateAreaData privateAreaData) {
      foreach (Light light in privateAreaData.PointLight) {
        light.color = privateAreaData.TargetColor;
      }

      foreach (Renderer renderer in privateAreaData.GlowMaterial) {
        renderer.GetPropertyBlock(_propertyBlock, 0);
        _propertyBlock.SetColor(EmissionColorShaderId, privateAreaData.TargetColor);
        renderer.SetPropertyBlock(_propertyBlock, 0);
      }

      foreach (ParticleSystem system in privateAreaData.SparcsSystem) {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(privateAreaData.TargetColor);

        ParticleSystem.MainModule main = system.main;
        main.startColor = privateAreaData.TargetColor;

        system.Clear();
        system.Simulate(0f);
        system.Play();
      }

      //foreach (ParticleSystemRenderer renderer in privateAreaData.SparcsRenderer) {
      //  renderer.GetPropertyBlock(_propertyBlock);
      //  _propertyBlock.SetColor(ColorShaderId, privateAreaData.TargetColor);
      //  renderer.SetPropertyBlock(_propertyBlock);
      //}

      foreach (ParticleSystem system in privateAreaData.FlareSystem) {
        Color flareColor = privateAreaData.TargetColor;
        flareColor.a = 0.1f;

        ParticleSystem.MainModule main = system.main;
        main.startColor = flareColor;

        system.Clear();
        system.Simulate(0f);
        system.Play();
      }
    }
  }
}
