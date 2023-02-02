using System.Collections;
using System.IO;
using System.Reflection;

using BepInEx;

using ComfyLib;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static Intermission.PluginConfig;

namespace Intermission {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Intermission : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.intermission";
    public const string PluginName = "Intermission";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      if (IsModEnabled.Value) {
        CustomAssets.Initialize(Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath), PluginName));
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static void SetLoadingTip(Text tipText) {
      if (tipText && CustomAssets.GetRandomLoadingTip(out string loadingTipText)) {
        tipText.SetText(loadingTipText);
      }
    }

    public static void SetLoadingImage(Image loadingImage) {
      if (loadingImage && CustomAssets.GetRandomLoadingImage(out Sprite loadingImageSprite)) {
        loadingImage
            .SetSprite(loadingImageSprite)
            .SetType(Image.Type.Simple)
            .SetColor(Color.white)
            .SetPreserveAspect(true);
      }
    }

    static Text _cachedTipText;

    public static void SetupTipText(Text tipText = default) {
      if (tipText) {
        _cachedTipText = tipText;
      } else if (_cachedTipText) {
        tipText = _cachedTipText;
      } else {
        return;
      }

      tipText
          .SetAlignment(TextAnchor.UpperCenter)
          .SetHorizontalOverflow(HorizontalWrapMode.Overflow)
          .SetFontSize(LoadingTipTextFontSize.Value)
          .SetColor(LoadingTipTextColor.Value);

      tipText.GetComponent<Outline>()
          .SetEnabled(false);

      tipText.GetOrAddComponent<Shadow>()
          .SetEnabled(true)
          .SetEffectColor(LoadingTipShadowEffectColor.Value)
          .SetEffectDistance(LoadingTipShadowEffectDistance.Value);

      tipText.GetComponent<RectTransform>()
          .SetAnchorMin(new(0.5f, 0f))
          .SetAnchorMax(new(0.5f, 0f))
          .SetPosition(LoadingTipTextPosition.Value)
          .SetSizeDelta(new(700f, 78f));
    }

    static Transform _cachedPanelSeparator;

    public static void SetupPanelSeparator(Transform panelSeparator = default) {
      if (panelSeparator) {
        _cachedPanelSeparator = panelSeparator;
      } else if (_cachedPanelSeparator) {
        panelSeparator = _cachedPanelSeparator;
      } else {
        return;
      }

      panelSeparator.SetActive(LoadingScreenShowPanelSeparator.Value);
      panelSeparator.GetComponent<RectTransform>()
          .SetAnchorMin(new(0.5f, 0f))
          .SetAnchorMax(new(0.5f, 0f))
          .SetPosition(LoadingScreenPanelSeparatorPosition.Value);
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
  }
}