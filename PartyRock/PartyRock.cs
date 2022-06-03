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

        _playerListPanel.Panel.SetActive(false);
      }

      bool toggle = !_playerListPanel.Panel.activeSelf;
      _playerListPanel.Panel.SetActive(toggle);

      if (toggle) {
        PopulatePlayerList();
      }
    }

    static void PopulatePlayerList() {
      _playerListPanel.ClearList();

      foreach (string name in ZNet.m_instance.m_players.Select(pi => pi.m_name).Take(5)) {
        _playerListPanel.CreatePlayerSlot(name);
      }
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}