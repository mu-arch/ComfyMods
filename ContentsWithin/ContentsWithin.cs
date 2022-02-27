using BepInEx;
using BepInEx.Configuration;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

namespace ContentsWithin {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ContentsWithin : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.contentswithin";
    public const string PluginName = "ContentsWithin";
    public const string PluginVersion = "1.0.0";

    static ConfigEntry<bool> _isModEnabled;

    static bool _showContainerContent = true;
    static Container _lastHoverContainer = null;
    static GameObject _lastHoverObject = null;

    static GameObject _inventoryPanel;
    static GameObject _containerPanel;
    static GameObject _infoPanel;
    static GameObject _craftingPanel;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.UpdateHover))]
      static void UpdateHoverPostfix(ref Player __instance) {
        if (!_isModEnabled.Value || _lastHoverObject == __instance.m_hovering) {
          return;
        }

        _lastHoverObject = __instance.m_hovering;
        Player.m_localPlayer?.StartCoroutine(ToggleShowContainerContents(_lastHoverObject));
      }
    }

    static IEnumerator ToggleShowContainerContents(GameObject hoverObject) {
      yield return null;

      Container container = hoverObject?.GetComponentInParent<Container>();
      _lastHoverContainer = container;

      if (!_showContainerContent) {
        yield break;
      }

      if (container
          && container.m_checkGuardStone
          && PrivateArea.CheckAccess(container.transform.position, 0f, false, false)) {
        InventoryGui.m_instance.Show(container);
      } else if (_containerPanel && _containerPanel.activeSelf) {
        InventoryGui.m_instance.Hide();
      }
    }

    [HarmonyPatch(typeof(InventoryGui))]
    class InventoryGuiPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.Awake))]
      static void AwakePostfix(ref InventoryGui __instance) {
        _inventoryPanel = __instance.m_player?.gameObject;
        _containerPanel = __instance.m_container?.gameObject;
        _infoPanel = __instance.m_infoPanel?.gameObject;
        _craftingPanel = __instance.m_inventoryRoot.Find("Crafting")?.gameObject;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.IsVisible))]
      static void IsVisiblePostfix(ref bool __result) {
        if (_isModEnabled.Value && _lastHoverContainer && _showContainerContent) {
          __result = false;
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.Show))]
      [HarmonyAfter(new string[] { "virtuacode.valheim.trashitem" })]
      static void ShowPostfix(ref InventoryGui __instance, ref Container container) {
        if (_isModEnabled.Value) {
          _inventoryPanel?.SetActive(!_showContainerContent || !container);
          _craftingPanel?.SetActive(container ? false : true);
          _infoPanel?.SetActive(container ? false : true);
        }
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(InventoryGui.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(
                    OpCodes.Call, typeof(ZInput).GetMethod(nameof(ZInput.ResetButtonStatus), BindingFlags.Static)),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, typeof(InventoryGui).GetMethod(nameof(InventoryGui.Hide))))
            .Advance(offset: 2)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Action<InventoryGui>>(UpdateHideTranspiler))
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Call, typeof(InventoryGui).GetMethod(nameof(InventoryGui.Show))))
            .Advance(offset: 2)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Action<InventoryGui, Container>>(UpdateShowTranspiler))
            .InstructionEnumeration();
      }

      static void UpdateHideTranspiler(InventoryGui inventoryGui) {
        if (!_isModEnabled.Value) {
          inventoryGui.Hide();
          return;
        }

        if (_showContainerContent) {
          _showContainerContent = false;
          inventoryGui.Show(_lastHoverContainer);
        } else {
          inventoryGui.Hide();
          _showContainerContent = true;
        }
      }

      static void UpdateShowTranspiler(InventoryGui inventoryGui, Container container) {
        if (_isModEnabled.Value && _showContainerContent) {
          _showContainerContent = false;
        }

        inventoryGui.Show(container);
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(InventoryGui.UpdateContainer))]
      static IEnumerable<CodeInstruction> UpdateContainerTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(
                    OpCodes.Ldfld,
                    typeof(InventoryGui).GetField(
                        nameof(InventoryGui.m_currentContainer), BindingFlags.Instance | BindingFlags.NonPublic)),
                new CodeMatch(OpCodes.Callvirt, typeof(Container).GetMethod(nameof(Container.IsOwner))))
            .Advance(offset: 2)
            .SetInstructionAndAdvance(Transpilers.EmitDelegate<Func<Container, bool>>(UpdateContainerIsOwnerDelegate))
            .InstructionEnumeration();
      }

      static bool UpdateContainerIsOwnerDelegate(Container container) {
        if (_isModEnabled.Value && _showContainerContent) {
          return true;
        }
        
        return container.IsOwner();
      }
    }

    [HarmonyPatch(typeof(Container))]
    class ContainerPatch {
      [HarmonyPrefix]
      [HarmonyPatch(nameof(Container.SetInUse))]
      static bool SetInUsePrefix() {
        if (_isModEnabled.Value && _lastHoverContainer && _showContainerContent) {
          return false;
        }

        return true;
      }
    }
  }
}
