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

    public static void SetupInventoryGui(InventoryGui inventoryGui) {
      if (!inventoryGui) {
        return;
      }

      if (IsModEnabled.Value) {
        inventoryGui.m_animator.enabled = false;
        inventoryGui.m_animator.SetBool("visible", false);

        RootPanelFader = inventoryGui.m_inventoryRoot.GetOrAddComponent<PanelFader>();
        RootPanelFader.Hide();
      } else {
        RootPanelFader.Ref()?.Show();

        inventoryGui.m_animator.enabled = true;
        inventoryGui.m_inventoryRoot.gameObject.SetActive(true);
      }
    }
  }
}