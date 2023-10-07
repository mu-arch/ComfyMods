using System.Collections.Generic;
using System.Linq;

using TMPro;

using UnityEngine;

namespace ComfyLib {
  public static class FontCache {
    public static readonly string ValheimAveriaSansLibreFont = "Valheim-AveriaSansLibre";

    public static TMP_FontAsset ValheimAveriaSansLibreFontAsset {
      get => UnifiedPopup.instance.bodyText.font;
    }

    static readonly Dictionary<string, TMP_FontAsset> _fontAssetCache = new();

    public static TMP_FontAsset GetFontAssetByName(string fontAssetName) {
      if (!_fontAssetCache.TryGetValue(fontAssetName, out TMP_FontAsset fontAsset)) {
        fontAsset =
            fontAssetName == ValheimAveriaSansLibreFont
                ? ValheimAveriaSansLibreFontAsset
                :  Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name == fontAssetName);

        _fontAssetCache[fontAssetName] = fontAsset;
      }

      return fontAsset;
    }
  }
}
