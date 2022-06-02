using BepInEx;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using static PartyRock.PlayerListPanel;
using static PartyRock.PluginConfig;

namespace PartyRock {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class PartyRock : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.partyrock";
    public const string PluginName = "PartyRock";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static PlayerListPanel _playerListPanel;

    public static void TogglePlayerListPanel() {
      if (!Hud.m_instance) {
        return;
      }

      if (!_playerListPanel?.Panel) {
        _playerListPanel = new(Hud.m_instance.m_rootObject.transform);

        _playerListPanel.Panel.RectTransform()
            .SetPosition(new(50, 125))
            .SetSizeDelta(new(350, 450));
      }

      bool toggle = !_playerListPanel.Panel.activeSelf;
      _playerListPanel.Panel.SetActive(toggle);
    }
  }

  [HarmonyPatch(typeof(Terminal))]
  public class TerminalPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    static void InitTerminalPostfix() {
      new Terminal.ConsoleCommand(
          "playerlist",
          "PartyRock: toggle the PlayerListPanel.",
          args => PartyRock.TogglePlayerListPanel());
      ;
    }
  }

  [HarmonyPatch(typeof(Hud))]
  public class HudPatch {
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Hud.Awake))]
    static void AwakePostfix(ref Hud __instance) {
      if (IsModEnabled.Value) {
        PartyRock.TogglePlayerListPanel();
      }
    }
  }

  [HarmonyPatch(typeof(ZDOMan))]
  public class ZDOManPatch {
    public static HashSet<ZDO> PlayerZdos { get; } = new();

    [HarmonyPostfix]
    [HarmonyPatch(
        nameof(ZDOMan.FindSectorObjects),
        typeof(Vector2i), typeof(int), typeof(int), typeof(List<ZDO>), typeof(List<ZDO>))]
    static void FindSectorObjectsPostfix(ref List<ZDO> sectorObjects) {
      sectorObjects.AddRange(PlayerZdos);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ZDOMan.DestroyZDO))]
    static void DestroyZDOPrefix(ref ZDO zdo) {
      if (PlayerZdos.Remove(zdo)) {
        ZLog.Log($"Destroying Player ZDO: {zdo?.m_uid}");
      }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(ZDOMan.RPC_ZDOData))]
    static IEnumerable<CodeInstruction> RPC_ZDODataTranspiler(IEnumerable<CodeInstruction> instructions) {
      return new CodeMatcher(instructions)
        .MatchForward(useEnd: true, new CodeMatch(OpCodes.Callvirt, typeof(ZDO).GetMethod(nameof(ZDO.Deserialize))))
        .InsertAndAdvance(
            new CodeInstruction(OpCodes.Ldloc_S, Convert.ToByte(13)),
            Transpilers.EmitDelegate<Action<ZDO>>(DeserializeZdoDelegate))
        .InstructionEnumeration();
    }

    static readonly int PlayerHashCode = "Player".GetStableHashCode();

    static void DeserializeZdoDelegate(ZDO zdo) {
      if (zdo.m_prefab == PlayerHashCode) {
        PlayerZdos.Add(zdo);
      }
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}