using BepInEx.Configuration;

using ComfyLib;

using System;

using UnityEngine;

namespace ComfyLoadingScreens {
  public static class PluginConfig {
    public static ConfigEntry<bool> IsModEnabled { get; private set; }

    public static ConfigEntry<bool> LoadingImageUseScaleLerp { get; private set; }
    public static ConfigEntry<float> LoadingImageScaleLerpEndScale { get; private set; }
    public static ConfigEntry<float> LoadingImageScaleLerpDuration { get; private set; }

    public static ConfigEntry<Vector2> LoadingTipTextPosition { get; private set; }
    public static ConfigEntry<int> LoadingTipTextFontSize { get; private set; }
    public static ConfigEntry<Color> LoadingTipTextColor { get; private set; }

    public static ConfigEntry<Color> LoadingTipShadowEffectColor { get; private set; }
    public static ConfigEntry<Vector2> LoadingTipShadowEffectDistance { get; private set; }

    public static void BindConfig(ConfigFile config) {
      IsModEnabled =
          config.BindInOrder(
              "_Global", "isModEnabled", true, "Globally enable or disable this mod (restart required).");

      LoadingImageUseScaleLerp =
          config.BindInOrder(
              "LoadingImage.ScaleLerp",
              "useScaleLerp",
              true,
              "If true, performs a scale lerp animation on the loading image.");

      LoadingImageScaleLerpEndScale =
          config.BindInOrder(
              "LoadingImage.ScaleLerp",
              "lerpEndScale",
              1.05f,
              "Image.scale ending factor for the scale lerp animation.",
              new AcceptableValueRange<float>(0.5f, 1.5f));

      LoadingImageScaleLerpDuration =
          config.BindInOrder(
              "LoadingImage.ScaleLerp",
              "lerpDuration",
              15f,
              "Duration for the scale lerp animation.");

      LoadingTipTextPosition =
          config.BindInOrder(
              "LoadingTip.Text",
              "textPosition",
              new Vector2(0f, 90f),
              "LoadingTip.Text.position value.");

      LoadingTipTextPosition.SettingChanged += OnLoadingTipConfigChanged;

      LoadingTipTextFontSize =
          config.BindInOrder(
              "LoadingTip.Text",
              "textFontSize",
              24,
              "LoadingTip.Text.fontSize value.",
              new AcceptableValueRange<int>(0, 64));

      LoadingTipTextFontSize.SettingChanged += OnLoadingTipConfigChanged;

      LoadingTipTextColor =
          config.BindInOrder(
              "LoadingTip.Text",
              "textColor",
              Color.white,
              "LoadingTip.Text.color value.");

      LoadingTipTextColor.SettingChanged += OnLoadingTipConfigChanged;

      LoadingTipShadowEffectColor =
          config.BindInOrder(
              "LoadingTip.Text.Shadow",
              "shadowEffectColor",
              new Color(0f, 0f, 0f, 0.6f),
              "LoadingTip.Text.Shadow.effectColor value.");

      LoadingTipShadowEffectColor.SettingChanged += OnLoadingTipConfigChanged;

      LoadingTipShadowEffectDistance =
          config.BindInOrder(
              "LoadingTip.Text.Shadow",
              "shadowEffectDistance",
              new Vector2(2f, -2f),
              "LoadingTip.Text.Shadow.effectDistance value.");

      LoadingTipShadowEffectDistance.SettingChanged += OnLoadingTipConfigChanged;
    }

    static void OnLoadingTipConfigChanged(object sender, EventArgs args) {
      ComfyLoadingScreens.SetupTipText(Hud.m_instance.Ref()?.m_loadingTip);
    }
  }
}
