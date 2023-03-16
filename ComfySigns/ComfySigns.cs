using System;
using System.Reflection;

using BepInEx;

using ComfyLib;

using HarmonyLib;

using TMPro;

using UnityEngine;

using static ComfySigns.PluginConfig;

namespace ComfySigns {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class ComfySigns : BaseUnityPlugin {
    public const string PluginGuid = "comfy.valheim.modname";
    public const string PluginName = "ComfySigns";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static readonly EventHandler OnSignConfigChanged = (_, _) => {
      TMP_FontAsset font = UIFonts.GetFontAsset(SignDefaultTextFont.Value);
      Color color = SignDefaultTextColor.Value;

      foreach (Sign sign in Resources.FindObjectsOfTypeAll<Sign>()) {
        if (sign && sign.m_nview && sign.m_nview.IsValid() && sign.m_textWidget) {
          sign.m_textWidget
              .SetFont(font)
              .SetColor(color);
        }
      }
    };
  }
}