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

    public static void UpdateLocalPlayerDemisterBall() {
      if (LocalPlayerDemisterBall && LocalPlayerDemisterBallNetView) {
        ZNetView netView = LocalPlayerDemisterBallNetView;
        netView.m_zdo.Set(DemisterBallBodyColorHashCode, DemisterBallBodyColor.Value);
        netView.m_zdo.Set(DemisterBallBodyBrightnessHashCode, DemisterBallBodyBrightness.Value);
        netView.m_zdo.Set(DemisterBallPointLightColorHashCode, DemisterBallPointLightColor.Value);

        LocalPlayerDemisterBall.UpdateDemisterBall(forceUpdate: true);
      }
    }

    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");
    public static readonly int EmissionColorShaderId = Shader.PropertyToID("_EmissionColor");

    public static readonly int DemisterBallControlRevisionHashCode = "DemisterBallControlRevision".GetStableHashCode();
    public static readonly int DemisterBallBodyColorHashCode = "DemisterBallBodyColor".GetStableHashCode();
    public static readonly int DemisterBallBodyBrightnessHashCode = "DemisterBallBodyBrightness".GetStableHashCode();
    public static readonly int DemisterBallPointLightColorHashCode = "DemisterBallPointLightColor".GetStableHashCode();
  }
}