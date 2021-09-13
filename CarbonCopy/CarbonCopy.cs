using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CarbonCopy {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class CarbonCopy : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.carboncopy";
    public const string PluginName = "CarbonCopy";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Console))]
    class ConsolePatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Console.InputText))]
      static bool InputTextPrefix(ref Console __instance) {
        if (_isModEnabled.Value && ParseText(__instance.m_input.text)) {
          return false;
        }

        return true;
      }
    }

    [HarmonyPatch(typeof(Chat))]
    class ChatPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Chat.InputText))]
      static bool InputTextPrefix(ref Chat __instance) {
        if (_isModEnabled.Value && ParseText(__instance.m_input.text)) {
          return false;
        }

        return true;
      }
    }

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      static void AwakePostfix() {
        if (_isModEnabled.Value) {
          CreateResources();
        }
      }
    }

    static bool ParseText(string text) {
      if (text == "/ccpanel") {
        GameObject panel = CreatePanel(Hud.instance.transform);
        panel.SetActive(true);

        return true;
      }

      return false;
    }

    static readonly Dictionary<string, Sprite> _spriteByNameCache = new();

    static Sprite GetSprite(string spriteName) {
      if (!_spriteByNameCache.TryGetValue(spriteName, out Sprite sprite)) {
        sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(sprite => sprite.name == spriteName);
        _spriteByNameCache[spriteName] = sprite;
      }

      return sprite;
    }

    static DefaultControls.Resources _resources = new();

    static DefaultControls.Resources CreateResources() {
      _resources.standard ??= GetSprite("UISprite");
      _resources.background ??= GetSprite("Background");
      _resources.inputField ??= GetSprite("InputFieldBackground");
      _resources.knob ??= GetSprite("Knob");
      _resources.checkmark ??= GetSprite("Checkmark");
      _resources.dropdown ??= GetSprite("DropdownArrow");
      _resources.mask ??= GetSprite("UIMask");  

      return _resources;
    }

    static GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = DefaultControls.CreatePanel(_resources);
      panel.transform.SetParent(parentTransform, worldPositionStays: false);
      panel.name = "Panel";

      RectTransform panelTransform = panel.GetComponent<RectTransform>();
      panelTransform.anchorMin = new Vector2(0f, 0.5f);
      panelTransform.anchorMax = new Vector2(0f, 0.5f);
      panelTransform.pivot = new Vector2(0f, 0.5f);
      panelTransform.anchoredPosition = new Vector2(10f, 0f);

      VerticalLayoutGroup panelLayoutGroup = panel.AddComponent<VerticalLayoutGroup>();
      panelLayoutGroup.childControlHeight = true;
      panelLayoutGroup.childControlWidth = true;
      panelLayoutGroup.childForceExpandHeight = false;
      panelLayoutGroup.childForceExpandWidth = false;
      panelLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      panelLayoutGroup.padding = new RectOffset(left: 10, right: 10, top: 0, bottom: 0);
      panelLayoutGroup.spacing = 10f;

      ContentSizeFitter panelFitter = panel.AddComponent<ContentSizeFitter>();
      panelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      CreateHeader(panel.transform, "CarbonCopy");

      return panel;
    }

    static GameObject CreateHeader(Transform parentTransform, string headerLabel) {
      GameObject header = new("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
      header.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform headerTransform = header.GetComponent<RectTransform>();
      headerTransform.anchorMin = new Vector2(0.5f, 0.5f);
      headerTransform.anchorMax = new Vector2(0.5f, 0.5f);
      headerTransform.pivot = new Vector2(0.5f, 0.5f);

      HorizontalLayoutGroup headerLayoutGroup = header.GetComponent<HorizontalLayoutGroup>();
      headerLayoutGroup.childControlHeight = true;
      headerLayoutGroup.childControlWidth = true;
      headerLayoutGroup.childForceExpandHeight = false;
      headerLayoutGroup.childForceExpandWidth = false;
      headerLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      headerLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 10, bottom: 0);
      headerLayoutGroup.spacing = 10f;

      CreateIndicator(header.transform, new Color(123, 36, 28, 255));

      GameObject label = DefaultControls.CreateText(_resources);
      label.transform.SetParent(header.transform, worldPositionStays: false);
      label.name = "Label";

      Text labelText = label.GetComponent<Text>();
      labelText.alignment = TextAnchor.MiddleCenter;
      labelText.text = headerLabel;

      return header;
    }

    static GameObject CreateIndicator(Transform parentTransform, Color indicatorColor) {
      GameObject indicator = DefaultControls.CreateImage(_resources);
      indicator.transform.SetParent(parentTransform, worldPositionStays: false);
      indicator.name = "Indicator";

      RectTransform indicatorTransform = indicator.GetComponent<RectTransform>();
      indicatorTransform.anchorMin = new Vector2(0.5f, 0.5f);
      indicatorTransform.anchorMax = new Vector2(0.5f, 0.5f);
      indicatorTransform.pivot = new Vector2(0.5f, 0.5f);

      Image indicatorImage = indicator.GetComponent<Image>();
      indicatorImage.color = indicatorColor;
      indicatorImage.raycastTarget = true;
      indicatorImage.maskable = true;

      LayoutElement indicatorLayout = indicator.AddComponent<LayoutElement>();
      indicatorLayout.preferredWidth = 4;
      indicatorLayout.flexibleHeight = 1;

      return indicator;
    }
  }
}