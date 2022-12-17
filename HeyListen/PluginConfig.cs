using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;

namespace HeyListen {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> DemisterBallLockPosition { get; private set; }
    public static ConfigEntry<Vector3> DemisterBallLockOffset { get; private set; }

    public static ConfigEntry<float> DemisterBallColorBrightness { get; private set; }

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

      DemisterBallColorBrightness =
          config.BindInOrder(
              "DemisterBall.Color",
              "demisterBallColorBrightness",
              1f,
              "SE_Demister.m_ballPrefab.color.brightness.",
              ConfigDrawer.InputField());
    }
  }
}
