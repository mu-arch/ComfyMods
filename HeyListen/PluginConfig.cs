using BepInEx.Configuration;

using ComfyLib;

using System;

using UnityEngine;

namespace HeyListen {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> DemisterBallLockPosition { get; private set; }
    public static ConfigEntry<Vector3> DemisterBallLockOffset { get; private set; }

    public static ConfigEntry<Color> DemisterBallBodyColor { get; private set; }
    public static ConfigEntry<float> DemisterBallBodyBrightness { get; private set; }
    public static ConfigEntry<Color> DemisterBallPointLightColor { get; private set; }

    [Flags]
    public enum FlameEffects {
      None = 0,
      Flames = 1,
      FlamesLocal = 2,
      Flare = 4,
      Embers = 8,
      Distortion = 16,
      Energy = 32,
      Energy2 = 64,
      SparcsFront = 128
    }

    public static ConfigEntry<FlameEffects> DemisterBallFlameEffectsEnabled { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled = config.BindInOrder("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      DemisterBallLockPosition =
          config.BindInOrder(
              "DemisterBall",
              "demisterBallLockPosition",
              true,
              "SE_Demister.m_ballPrefab.transform.position lock to player.");

      DemisterBallLockOffset =
          config.BindInOrder(
              "DemisterBall",
              "demisterBallLockOffset",
              new Vector3(-0.2f, 0.5f, 0f),
              "SE_Demister.m_ballPrefab.transform.position offset when locked to player.");

      DemisterBallBodyColor =
          config.BindInOrder(
              "DemisterBall.Body",
              "demisterBallBodyColor",
              new Color(0f, 0.832f, 1f, 1f),
              "SE Demister.m_ballPrefab.color");

      DemisterBallBodyColor.SettingChanged += (_, _) => HeyListen.UpdateLocalPlayerDemisterBall();

      DemisterBallBodyBrightness =
          config.BindInOrder(
              "DemisterBall.Body",
              "demisterBallBodyBrightness",
              1f,
              "SE_Demister.m_ballPrefab.brightness",
              new AcceptableValueRange<float>(0f, 1f));

      DemisterBallBodyColor.SettingChanged += (_, _) => HeyListen.UpdateLocalPlayerDemisterBall();

      DemisterBallPointLightColor =
          config.BindInOrder(
              "DemisterBall.PointLight",
              "demisterBallPointLightColor",
              new Color(0.482f, 0.803f, 1f, 1f),
              "SE_Demister.m_ballPrefab/effects/Point light.color");

      DemisterBallPointLightColor.SettingChanged += (_, _) => HeyListen.UpdateLocalPlayerDemisterBall();

      DemisterBallFlameEffectsEnabled =
          config.BindInOrder(
              "DemisterBall.FlameEffects",
              "DemisterBallFlameEffectsEnabled",
              FlameEffects.Flare | FlameEffects.Embers | FlameEffects.Energy2 | FlameEffects.SparcsFront,
              "SE_Demister.m_ballPrefab/effectrs/flame/...");

      DemisterBallFlameEffectsEnabled.SettingChanged += (_, _) => {
        HeyListen.LocalPlayerDemisterBall?.Ref().UpdateFlameEffects(forceUpdate: false);
      };
    }
  }
}
