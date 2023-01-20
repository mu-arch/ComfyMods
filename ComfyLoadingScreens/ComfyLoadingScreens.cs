using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using BepInEx;

using HarmonyLib;

using UnityEngine;

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
      BindConfig(Config);

      PluginInstance = this;
      HarmonyInstance = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      HarmonyInstance?.UnpatchSelf();
    }

    public static IEnumerable<string> GetCustomLoadingTips() {
      string path = Path.Combine(Path.GetDirectoryName(PluginInstance.Info.Location), $"{PluginName}/loadingtips.txt");

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

    public static IEnumerable<string> GetCustomLoadingImageFiles() {
      string path = Path.Combine(Path.GetDirectoryName(PluginInstance.Info.Location), PluginName);
      Directory.CreateDirectory(path);

      string[] loadingImageFiles = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
      ZLog.Log($"Found {loadingImageFiles.Length} custom loading screens in directory: {path}");

      return loadingImageFiles;
    }

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
      texture.name = $"{imageFile}-texture";
      texture.LoadImage(File.ReadAllBytes(imageFile));

      sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), Vector2.zero, 1);
      sprite.name = $"{imageFile}-sprite";

      _customLoadingImageCache[imageFile] = sprite;

      return sprite;
    }
  }
}