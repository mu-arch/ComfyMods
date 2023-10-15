using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using ComfyLib;

using HarmonyLib;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static Intermission.PluginConfig;

namespace Intermission {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Intermission : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.intermission";
    public const string PluginName = "Intermission";
    public const string PluginVersion = "1.3.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    void Awake() {
      _logger = Logger;
      BindConfig(Config);

      if (IsModEnabled.Value) {
        CustomAssets.Initialize(Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath), PluginName));
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static void SetLoadingTip(TMP_Text tipText) {
      if (tipText && CustomAssets.GetRandomLoadingTip(out string loadingTipText)) {
        tipText.SetText(loadingTipText);
      }
    }

    public static void SetLoadingImage(Image loadingImage) {
      if (loadingImage && CustomAssets.GetRandomLoadingImage(out Sprite loadingImageSprite)) {
        loadingImage.SetSprite(loadingImageSprite);
      }
    }

    static TMP_Text _cachedTipText;

    public static void SetupTipText(TMP_Text tipText = default) {
      if (tipText) {
        _cachedTipText = tipText;
      } else if (_cachedTipText) {
        tipText = _cachedTipText;
      } else {
        LogError($"Could not find a TipText to setup!");
      }

      tipText
          .SetAlignment(TextAlignmentOptions.Top)
          .SetTextWrappingMode(TextWrappingModes.Normal)
          .SetFontSize(LoadingTipTextFontSize.Value)
          .SetColor(LoadingTipTextColor.Value);

      tipText.GetComponent<RectTransform>()
          .SetAnchorMin(Vector2.zero)
          .SetAnchorMax(Vector2.right)
          .SetPosition(LoadingTipTextPosition.Value)
          .SetSizeDelta(new(-50f, 78f));
    }

    static Image _cachedLoadingImage;

    public static void SetupLoadingImage(Image loadingImage = default) {
      if (loadingImage) {
        _cachedLoadingImage = loadingImage;
      } else if (_cachedLoadingImage) {
        loadingImage = _cachedLoadingImage;
      } else {
        LogError($"Could not find a LoadingImage to setup!");
      }

      loadingImage
          .SetType(Image.Type.Simple)
          .SetColor(LoadingImageBaseColor.Value)
          .SetPreserveAspect(true);
    }

    static Transform _cachedPanelSeparator;

    public static void SetupPanelSeparator(Transform panelSeparator = default) {
      if (panelSeparator) {
        _cachedPanelSeparator = panelSeparator;
      } else if (_cachedPanelSeparator) {
        panelSeparator = _cachedPanelSeparator;
      } else {
        LogError($"Could not find a PanelSeparator to setup!");
      }

      panelSeparator.SetActive(LoadingScreenShowPanelSeparator.Value);
      panelSeparator.GetComponent<RectTransform>()
          .SetAnchorMin(new(0.5f, 0f))
          .SetAnchorMax(new(0.5f, 0f))
          .SetPosition(LoadingScreenPanelSeparatorPosition.Value);
    }

    static Coroutine _scaleLerpCoroutine;

    public static void ScaleLerpLoadingImage(Image loadingImage) {
      if (_scaleLerpCoroutine != null) {
        Hud.m_instance.Ref()?.StopCoroutine(_scaleLerpCoroutine);
        _scaleLerpCoroutine = null;
      }

      if (LoadingImageUseScaleLerp.Value && loadingImage) {
        _scaleLerpCoroutine = 
            Hud.m_instance.Ref()?.StartCoroutine(
                ScaleLerp(
                    loadingImage.transform,
                    Vector3.one,
                    Vector3.one * LoadingImageScaleLerpEndScale.Value,
                    LoadingImageScaleLerpDuration.Value));
      }
    }

    public static IEnumerator ScaleLerp(
        Transform transform, Vector3 startScale, Vector3 endScale, float lerpDuration) {
      transform.localScale = startScale;
      float timeElapsed = 0f;

      while (timeElapsed < lerpDuration) {
        float t = timeElapsed / lerpDuration;
        t = t * t * (3f - (2f * t));

        transform.localScale = Vector3.Lerp(startScale, endScale, t);
        timeElapsed += Time.deltaTime;

        yield return null;
      }

      transform.localScale = endScale;
    }

    public static void LogInfo(object o) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
    }

    public static void LogError(object o) {
      _logger.LogError($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {o}");
    }
  }
}