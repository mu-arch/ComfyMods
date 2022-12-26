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
        UpdateDemisterBallControlZdo(LocalPlayerDemisterBallNetView.m_zdo);
        LocalPlayerDemisterBall.UpdateDemisterBall(forceUpdate: true);
      }
    }

    public static void UpdatePlayerDemisterBallFlameEffects() {
      if (LocalPlayerDemisterBall && LocalPlayerDemisterBallNetView) {
        UpdateDemisterBallControlZdo(LocalPlayerDemisterBallNetView.m_zdo);
        LocalPlayerDemisterBall.UpdateFlameEffects();
      }
    }

    public static void UpdateUseCustomSettings() {
      foreach (Demister demister in Demister.m_instances) {
        GameObject prefab = demister.transform.root.gameObject;

        if (!prefab.name.StartsWith("demister_ball", System.StringComparison.Ordinal)) {
          continue;
        }

        if (prefab.TryGetComponent(out DemisterBallControl demisterBallControl)) {
          if (!DemisterBallUseCustomSettings.Value) {
            Destroy(demisterBallControl);
          }
        } else if (DemisterBallUseCustomSettings.Value) {
          prefab.AddComponent<DemisterBallControl>();
        }
      }

      if (!DemisterBallUseCustomSettings.Value) {
        LocalPlayerDemisterBall = default;
        LocalPlayerDemisterBallNetView = default;
      }
    }

    public static void SetLocalPlayerDemisterBallControl(DemisterBallControl demisterBallControl) {
      ZLog.Log($"Setting DemisterBallControl to local config.");
      LocalPlayerDemisterBall = demisterBallControl;
      LocalPlayerDemisterBallNetView = demisterBallControl.NetView;

      UpdateDemisterBallControlZdo(LocalPlayerDemisterBallNetView.m_zdo);
      LocalPlayerDemisterBall.UpdateDemisterBall(forceUpdate: true);
    }

    static void UpdateDemisterBallControlZdo(ZDO zdo) {
      zdo.Set(DemisterBallBodyScaleHashCode, DemisterBallBodyScale.Value);
      zdo.Set(DemisterBallBodyColorHashCode, DemisterBallBodyColor.Value);
      zdo.Set(DemisterBallBodyBrightnessHashCode, DemisterBallBodyBrightness.Value);
      zdo.Set(DemisterBallPointLightColorHashCode, DemisterBallPointLightColor.Value);

      zdo.Set(FlameEffectsEnabledHashCode, (int) DemisterBallFlameEffectsEnabled.Value);
      zdo.Set(FlameEffectsColorHashCode, DemisterBallFlameEffectsColor.Value);
      zdo.Set(FlameEffectsEmbersColorHashCode, FlameEffectsEmbersColor.Value);
      zdo.Set(FlameEffectsEmbersBrightnessHashCode, FlameEffectsEmbersBrightness.Value);
      zdo.Set(FlameEffectsSparcsColorHashCode, FlameEffectsSparcsColor.Value);
      zdo.Set(FlameEffectsSparcsBrightnessHashCode, FlameEffectsSparcsBrightness.Value);
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