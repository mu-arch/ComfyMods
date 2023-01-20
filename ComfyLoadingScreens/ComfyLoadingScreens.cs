using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using BepInEx;

using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

using static ComfyLoadingScreens.PluginConfig;

namespace ComfyLoadingScreens {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class ComfyLoadingScreens : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.comfyloadingscreens";
    public const string PluginName = "ComfyLoadingScreens";
    public const string PluginVersion = "1.0.0";

    public static BaseUnityPlugin PluginInstance { get; private set; }
    public static Harmony HarmonyInstance { get; private set; }

    public void Awake() {
      PluginInstance = this;
      BindConfig(Config);

      if (IsModEnabled.Value) {
        CustomLoadingTips.Clear();
        CustomLoadingTips.AddRange(GetCustomLoadingTips());

        CustomLoadingImageFiles.Clear();
        CustomLoadingImageFiles.AddRange(GetCustomLoadingImageFiles());

        HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      HarmonyInstance?.UnpatchSelf();
    }

    public static List<string> CustomLoadingTips { get; } = new();

    public static IEnumerable<string> GetCustomLoadingTips() {
      string path = Path.Combine(Path.GetDirectoryName(PluginInstance.Info.Location), $"{PluginName}/tips.txt");

      if (File.Exists(path)) {
        string[] loadingTips = File.ReadAllLines(path);
        ZLog.Log($"Found {loadingTips.Length} custom tips in file: {path}");

        return loadingTips;
      }

      ZLog.Log($"Creating new empty custom tips file: {path}");
      Directory.CreateDirectory(path);
      File.Create(path);

      return Array.Empty<string>();
    }

    public static void SetCustomLoadingTip(Text tipText) {
      if (tipText && CustomLoadingTips.Count > 0) {
        string customTip = CustomLoadingTips.RandomElement();
        ZLog.Log($"Using custom tip: {customTip}");
        tipText.text = customTip;
      }
    }

    public static void SetupTipText(Text tipText) {
      if (!tipText) {
        return;
      }

      tipText.alignment = TextAnchor.UpperCenter;
      tipText.horizontalOverflow = HorizontalWrapMode.Overflow;
      tipText.fontSize = LoadingTipTextFontSize.Value;
      tipText.color = LoadingTipTextColor.Value;

      Outline outline = tipText.GetComponent<Outline>();
      outline.enabled = false;

      if (!tipText.gameObject.TryGetComponent(out Shadow shadow)) {
        shadow = tipText.gameObject.AddComponent<Shadow>();
      }

      shadow.enabled = true;
      shadow.effectColor = LoadingTipShadowEffectColor.Value;
      shadow.effectDistance = LoadingTipShadowEffectDistance.Value;

      RectTransform rectTransform = tipText.GetComponent<RectTransform>();
      rectTransform.anchorMin = new(0.5f, 0f);
      rectTransform.anchorMax = new(0.5f, 0f);
      rectTransform.anchoredPosition = LoadingTipTextPosition.Value;
    }

    public static IEnumerable<string> GetCustomLoadingImageFiles() {
      string path = Path.Combine(Path.GetDirectoryName(PluginInstance.Info.Location), PluginName);
      Directory.CreateDirectory(path);

      string[] loadingImageFiles = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
      ZLog.Log($"Found {loadingImageFiles.Length} custom loading screens in directory: {path}");

      return loadingImageFiles;
    }

    public static List<string> CustomLoadingImageFiles { get; } = new();

    static readonly Dictionary<string, Sprite> _customLoadingImageCache = new();

    public static Sprite GetCustomLoadingImage(string imageFile) {
      if (_customLoadingImageCache.TryGetValue(imageFile, out Sprite sprite)) {
        return sprite;
      }

      if (File.Exists(imageFile)) {
        ZLog.Log($"Loading custom image file: {imageFile}");
      } else {
        ZLog.LogError($"Could not find custom loading image file: {imageFile}");
        return null;
      }

      Texture2D texture = new(1, 1);
      texture.name = $"{Path.GetFileName(imageFile)}.texture";
      texture.LoadImage(File.ReadAllBytes(imageFile));

      sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), Vector2.zero, 1);
      sprite.name = $"{Path.GetFileName(imageFile)}.sprite";

      _customLoadingImageCache[imageFile] = sprite;

      return sprite;
    }

    public static void SetCustomLoadingImage(Image loadingImage) {
      if (loadingImage && CustomLoadingImageFiles.Count > 0) {
        Sprite customImageSprite = GetCustomLoadingImage(CustomLoadingImageFiles.RandomElement());

        if (customImageSprite) {
          ZLog.Log($"Using custom image sprite: {customImageSprite.name}");
          loadingImage.sprite = customImageSprite;
          loadingImage.type = Image.Type.Simple;
          loadingImage.color = Color.white;
          loadingImage.preserveAspect = true;
        }
      }
    }
  }
}