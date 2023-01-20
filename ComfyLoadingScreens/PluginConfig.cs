using BepInEx.Configuration;

using ComfyLib;

using UnityEngine;
using UnityEngine.UI;

namespace ComfyLoadingScreens {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<Vector2> LoadingTipTextPosition { get; private set; }
    public static ConfigEntry<int> LoadingTipTextFontSize { get; private set; }
    public static ConfigEntry<Color> LoadingTipTextColor { get; private set; }

    public static ConfigEntry<Color> LoadingTipShadowEffectColor { get; private set; }
    public static ConfigEntry<Vector2> LoadingTipShadowEffectDistance { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled =
          config.BindInOrder(
              "_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      LoadingTipTextPosition =
          config.BindInOrder(
              "LoadingTip.Text",
              "textPosition",
              new Vector2(0f, 90f),
              "LoadingTip.Text.position value.");

      LoadingTipTextFontSize =
          config.BindInOrder(
              "LoadingTip.Text",
              "textFontSize",
              24,
              "LoadingTip.Text.fontSize value.",
              new AcceptableValueRange<int>(0, 64));

      LoadingTipTextColor =
          config.BindInOrder(
              "LoadingTip.Text",
              "textColor",
              Color.white,
              "LoadingTip.Text.color value.");

      LoadingTipShadowEffectColor =
          config.BindInOrder(
              "LoadingTip.Text.Shadow",
              "shadowEffectColor",
              new Color(0f, 0f, 0f, 0.6f),
              "LoadingTip.Text.Shadow.effectColor value.");

      LoadingTipShadowEffectDistance =
          config.BindInOrder(
              "LoadingTip.Text.Shadow",
              "shadowEffectDistance",
              new Vector2(2f, -2f),
              "LoadingTip.Text.Shadow.effectDistance value.");

      config.SettingChanged += (_, eventArgs) => {
        if (eventArgs.ChangedSetting == IsModEnabled) {
          return;
        }

        ComfyLoadingScreens.SetupTipText(Hud.m_instance.Ref()?.m_loadingTip);
      };
    }
  }
}
