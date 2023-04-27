using System.Reflection;

using BepInEx;

using ComfyLib;

using HarmonyLib;

using UnityEngine;

using static Inventorious.PluginConfig;

namespace Inventorious {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Inventorious : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.inventorious";
    public const string PluginName = "Inventorious";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      if (IsModEnabled.Value) {
        _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
      }
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static PanelFader RootPanelFader { get; private set; }

    public static ComfyPanel CraftingPanel { get; private set; }

    public static void SetupInventoryGui(InventoryGui inventoryGui) {
      if (!inventoryGui) {
        return;
      }

      if (IsModEnabled.Value) {
        inventoryGui.m_animator.enabled = false;

        RootPanelFader = inventoryGui.m_inventoryRoot.GetOrAddComponent<PanelFader>();
        RootPanelFader.FadePanel(inventoryGui.m_animator.GetBool("visible") ? 1f : 0f, 0f);
      } else {
        RootPanelFader.Ref()?.Show();

        inventoryGui.m_animator.enabled = true;
        inventoryGui.m_inventoryRoot.gameObject.SetActive(true);
      }

      //if (!CraftingPanel) {
      //  GameObject panel = new("CraftingPanel", typeof(RectTransform));
      //  panel.transform.SetParent(inventoryGui.m_inventoryRoot, worldPositionStays: false);

      //  CraftingPanel = ComfyPanel.CreatePanel(panel);

      //  CraftingPanel.RectTransform
      //      .SetAnchorMin(new(0.5f, 0.5f))
      //      .SetAnchorMax(new(0.5f, 0.5f))
      //      .SetPivot(new(0.5f, 0.5f))
      //      .SetSizeDelta(new(400f, 400f));

      //  panel.SetActive(true);
      //}
    }
  }
}