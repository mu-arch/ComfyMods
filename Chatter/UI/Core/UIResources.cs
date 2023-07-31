using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TMPro;

using UnityEngine;

namespace Chatter {
  public static class UIResources {
    static readonly Dictionary<string, Font> _fontCache = new();

    public static Font GetFont(string fontName) {
      if (_fontCache.TryGetValue(fontName, out Font cachedFont)) {
        return cachedFont;
      }

      if (OsFontMap.Value.TryGetValue(fontName, out string osFontPath)) {
        _fontCache[fontName] = new(osFontPath);
      } else {
        foreach (Font font in Resources.FindObjectsOfTypeAll<Font>()) {
          _fontCache[font.name] = font;
        }
      }

      _fontCache.TryGetValue(fontName, out cachedFont);
      return cachedFont;
    }

    public static readonly Lazy<Dictionary<string, string>> OsFontMap =
        new(() => {
          Dictionary<string, string> map = new();
          foreach (string osFontPath in Font.GetPathsToOSFonts()) {
            map[Path.GetFileNameWithoutExtension(osFontPath)] = osFontPath;
          }

          return map;
        });

    public static readonly string AveriaSerifLibre = "AveriaSerifLibre-Bold";
    public static Font AveriaSerifLibreFont { get => GetFont(AveriaSerifLibre); }

    static readonly Dictionary<string, TMP_FontAsset> _fontAssetCache = new();

    public static TMP_FontAsset GetFontAssetByFont(Font font) {
      if (_fontAssetCache.TryGetValue(font.name, out TMP_FontAsset fontAsset)) {
        return fontAsset;
      }

      fontAsset = (font == AveriaSerifLibreFont) ? AveriaSerifLibreFontAsset : TMP_FontAsset.CreateFontAsset(font);

      _fontAssetCache[fontAsset.name] = fontAsset;
      _fontAssetCache[font.name] = fontAsset;

      return fontAsset;
    }

    public static TMP_FontAsset GetFontAssetByName(string fontAssetName) {
      if (_fontAssetCache.TryGetValue(fontAssetName, out TMP_FontAsset fontAsset)) {
        return fontAsset;
      }

      foreach (TMP_FontAsset existingFontAsset in Resources.FindObjectsOfTypeAll<TMP_FontAsset>()) {
        _fontAssetCache[existingFontAsset.name] = existingFontAsset;
      }

      return _fontAssetCache[fontAssetName];
    }

    public static TMP_FontAsset AveriaSerifLibreFontAsset {
      get => UnifiedPopup.instance.bodyText.font;
    }
  }
}