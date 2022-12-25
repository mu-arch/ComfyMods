using System.Reflection;

using BepInEx;

using HarmonyLib;

using UnityEngine;

using static HeyListen.PluginConfig;

namespace HeyListen {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class HeyListen : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.heylisten";
    public const string PluginName = "HeyListen";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static DemisterBallControl LocalPlayerDemisterBall { get; set; }
    public static ZNetView LocalPlayerDemisterBallNetView { get; set; }

    public static void UpdatePlayerDemisterBall() {
      if (LocalPlayerDemisterBall && LocalPlayerDemisterBallNetView) {
        ZNetView netView = LocalPlayerDemisterBallNetView;
        netView.m_zdo.Set(DemisterBallBodyScaleHashCode, DemisterBallBodyScale.Value);
        netView.m_zdo.Set(DemisterBallBodyColorHashCode, DemisterBallBodyColor.Value);
        netView.m_zdo.Set(DemisterBallBodyBrightnessHashCode, DemisterBallBodyBrightness.Value);
        netView.m_zdo.Set(DemisterBallPointLightColorHashCode, DemisterBallPointLightColor.Value);

        LocalPlayerDemisterBall.UpdateDemisterBall(forceUpdate: true);
      }
    }

    public static void UpdatePlayerDemisterBallFlameEffects() {
      if (LocalPlayerDemisterBall && LocalPlayerDemisterBallNetView) {
        ZDO zdo = LocalPlayerDemisterBallNetView.m_zdo;

        zdo.Set(FlameEffectsEnabledHashCode, (int) DemisterBallFlameEffectsEnabled.Value);
        zdo.Set(FlameEffectsColorHashCode, DemisterBallFlameEffectsColor.Value);
        zdo.Set(FlameEffectsEmbersColorHashCode, FlameEffectsEmbersColor.Value);
        zdo.Set(FlameEffectsEmbersBrightnessHashCode, FlameEffectsEmbersBrightness.Value);
        zdo.Set(FlameEffectsSparcsColorHashCode, FlameEffectsSparcsColor.Value);
        zdo.Set(FlameEffectsSparcsBrightnessHashCode, FlameEffectsSparcsBrightness.Value);

        LocalPlayerDemisterBall.UpdateFlameEffects();
      }
    }

    public static void UpdateUseCustomSettings() {
      foreach (DemisterBallControl control in DemisterBallControl.Instances) {
        Destroy(control.gameObject);
      }
    }

    public static void AddDemisterBallControl(SE_Demister demister) {
      ZLog.Log($"Adding DemisterBallControl to m_ballInstance.");
      DemisterBallControl demisterBallControl = demister.m_ballInstance.AddComponent<DemisterBallControl>();

      if (demister.m_character == Player.m_localPlayer) {
        LocalPlayerDemisterBall = demisterBallControl;
        LocalPlayerDemisterBallNetView = demister.m_ballInstance.GetComponent<ZNetView>();

        ZLog.Log($"Setting DemisterBallControl to local config.");
        UpdatePlayerDemisterBall();
        UpdatePlayerDemisterBallFlameEffects();
      } else {
        demisterBallControl.UpdateDemisterBall(forceUpdate: true);
      }
    }

    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");

    public static readonly int DemisterBallBodyScaleHashCode = "DemisterBallBodyScale".GetStableHashCode();
    public static readonly int DemisterBallBodyColorHashCode = "DemisterBallBodyColor".GetStableHashCode();
    public static readonly int DemisterBallBodyBrightnessHashCode = "DemisterBallBodyBrightness".GetStableHashCode();
    public static readonly int DemisterBallPointLightColorHashCode = "DemisterBallPointLightColor".GetStableHashCode();

    public static readonly int FlameEffectsEnabledHashCode = "FlameEffectsEnabled".GetStableHashCode();
    public static readonly int FlameEffectsColorHashCode = "FlameEffectsColor".GetStableHashCode();

    public static readonly int FlameEffectsEmbersColorHashCode = "FlameEffectsEmbersColor".GetStableHashCode();
    public static readonly int FlameEffectsEmbersBrightnessHashCode =
        "FlameEffectsEmbersBrightness".GetStableHashCode();
    public static readonly int FlameEffectsSparcsColorHashCode = "FlameEffectsSparcsColor".GetStableHashCode();
    public static readonly int FlameEffectsSparcsBrightnessHashCode =
        "FlameEffectsSparcsBrightness".GetStableHashCode();
  }
}