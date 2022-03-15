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
    public const string PluginVersion = "1.0.1";

    static ConfigEntry<bool> _isModEnabled;
    static ConfigEntry<KeyboardShortcut> _toggleShowContentsShortcut;

    static bool _showContainerContents = true;
    static bool _isInventoryInUse = false;

    static Container _lastHoverContainer = null;
    static GameObject _lastHoverObject = null;

    static InventoryGui _inventoryGui;
    static GameObject _inventoryPanel;
    static GameObject _containerPanel;
    static GameObject _infoPanel;
    static GameObject _craftingPanel;
    static GameObject _takeAllButton;

    Harmony _harmony;

    public void Awake() {
      _isModEnabled = Config.Bind("_Global", "isModEnabled", true, "Globally enable or disable this mod.");

      _isModEnabled.SettingChanged +=
          (_, _) => Player.m_localPlayer.Ref()?.StartCoroutine(ToggleShowContainerContents(_isModEnabled.Value));

      _toggleShowContentsShortcut =
          Config.Bind(
              "Hotkeys",
              "toggleShowContentsShortcut",
              new KeyboardShortcut(KeyCode.P, KeyCode.RightShift),
              "Shortcut to toggle on/off the 'show container contents' feature.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Player.Update))]
      static IEnumerable<CodeInstruction> UpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(Character), nameof(Character.TakeInput))),
                new CodeMatch(OpCodes.Stloc_0))
            .Advance(offset: 2)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool, bool>>(TakeInputDelegate))
            .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0))
            .InstructionEnumeration();
      }

      static bool TakeInputDelegate(bool takeInputResult) {
        if (!_isModEnabled.Value || !_toggleShowContentsShortcut.Value.IsDown()) {
          return takeInputResult;
        }

        Player.m_localPlayer.Ref()?.StartCoroutine(ToggleShowContainerContents(!_showContainerContents));
        return false;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.UpdateHover))]
      static void UpdateHoverPostfix(ref Player __instance) {
        if (!_isModEnabled.Value || _lastHoverObject == __instance.m_hovering) {
          return;
        }

        _lastHoverObject = __instance.m_hovering;
        Player.m_localPlayer.Ref()?.StartCoroutine(ShowContainerContents(_lastHoverObject));
      }
    }

    static IEnumerator ToggleShowContainerContents(bool toggle) {
      yield return null;

      _showContainerContents = toggle;

      MessageHud.m_instance.ShowMessage(
          MessageHud.MessageType.Center, $"ShowContainerContents: {_showContainerContents}");

      if (_showContainerContents) {
        _isInventoryInUse = _inventoryPanel.activeInHierarchy;
        yield return ShowContainerContents(Player.m_localPlayer?.m_hovering);
      } else {
        if (_containerPanel.activeInHierarchy && !_isInventoryInUse && !_inventoryPanel.activeInHierarchy) {
          _inventoryGui.Hide();
        }
      }
    }

    static IEnumerator ShowContainerContents(GameObject hoverObject) {
      yield return null;

      Container container = hoverObject.Ref()?.GetComponentInParent<Container>();
      _lastHoverContainer = container;


      if (!_showContainerContents || _isInventoryInUse) {
        yield break;
      }

      if (container
          && PrivateArea.CheckAccess(container.transform.position, 0f, false, false)
          && container.CheckAccess(Game.m_instance.m_playerProfile.m_playerID)) {
        _inventoryGui.Show(container);
      } else if (_containerPanel && _containerPanel.activeSelf) {
        _inventoryGui.Hide();
      }
    }

    [HarmonyPatch(typeof(InventoryGui))]
    class InventoryGuiPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.Awake))]
      static void AwakePostfix(ref InventoryGui __instance) {
        _inventoryGui = __instance;
        _inventoryPanel = __instance.m_player.Ref()?.gameObject;
        _containerPanel = __instance.m_container.Ref()?.gameObject;
        _infoPanel = __instance.m_infoPanel.Ref()?.gameObject;
        _craftingPanel = __instance.m_inventoryRoot.Find("Crafting").Ref()?.gameObject;
        _takeAllButton = __instance.m_takeAllButton.Ref()?.gameObject;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.IsVisible))]
      static void IsVisiblePostfix(ref bool __result) {
        if (_isModEnabled.Value && _lastHoverContainer && _showContainerContents && !_isInventoryInUse) {
          __result = false;
        }
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(InventoryGui.Show))]
      static void ShowPostfix(ref InventoryGui __instance, ref Container container) {
        if (_isModEnabled.Value) {
          _inventoryPanel.Ref()?.SetActive(!_showContainerContents || _isInventoryInUse || !container);
          _craftingPanel.Ref()?.SetActive(container ? false : true);
          _infoPanel.Ref()?.SetActive(container ? false : true);
          _takeAllButton.Ref()?.SetActive(!_showContainerContents || _isInventoryInUse);
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

        if (_showContainerContents && !_isInventoryInUse && _lastHoverContainer) {
          if (_lastHoverContainer.IsOwner()) {
            _isInventoryInUse = true;
            inventoryGui.Show(_lastHoverContainer);
          } else {
            _lastHoverContainer.Interact(Player.m_localPlayer, false, false);
          }
        } else {
          inventoryGui.Hide();
          _isInventoryInUse = false;
        }
      }

      static void UpdateShowTranspiler(InventoryGui inventoryGui, Container container) {
        if (_isModEnabled.Value) {
          _isInventoryInUse = true;
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
        if (_isModEnabled.Value && _showContainerContents && !_isInventoryInUse) {
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
        if (_isModEnabled.Value && _lastHoverContainer && _showContainerContents && !_isInventoryInUse) {
          return false;
        }

        return true;
      }

      [HarmonyPrefix]
      [HarmonyPatch(nameof(Container.RPC_OpenRespons))]
      static void RpcOpenResponse(ref bool granted) {
        if (_isModEnabled.Value && Player.m_localPlayer) {
          _isInventoryInUse = granted;
        }
      }
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
