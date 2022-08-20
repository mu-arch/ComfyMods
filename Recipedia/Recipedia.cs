using BepInEx;

using ComfyLib;

using HarmonyLib;

using System.Reflection;

using UnityEngine;

using static Recipedia.PluginConfig;

namespace Recipedia {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Recipedia : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.recipedia";
    public const string PluginName = "Recipedia";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static RecipeListPanel RecipeListPanel { get; private set; }

    public static void ToggleRecipeListPanel() {
      if (!RecipeListPanel?.Panel) {
        RecipeListPanel = new(Hud.m_instance.transform);

        RecipeListPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 0f))
            .SetAnchorMax(new(0.5f, 0f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(Vector2.zero)
            .SetSizeDelta(new(400f, 400f));

        RecipeListPanel.Panel.SetActive(false);
      }

      ToggleRecipeListPanel(!RecipeListPanel.Panel.activeSelf);
    }

    public static void ToggleRecipeListPanel(bool toggleOn) {
      RecipeListPanel?.Panel.Ref()?.SetActive(toggleOn);
    }
  }
}