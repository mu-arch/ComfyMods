using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace SectorDisplay {
  [BepInPlugin(SectorDisplay.Package, SectorDisplay.ModName, SectorDisplay.Version)]
  public class SectorDisplay : BaseUnityPlugin {
    public const string Package = "redseiko.valheim.sectordisplay";
    public const string Version = "0.0.1";
    public const string ModName = "Sector Display";

    private static ConfigEntry<bool> isModEnabled;

    private Harmony _harmony;

    private void Awake() {
      isModEnabled = Config.Bind("Global", "isModEnabled", true, "Whether the mod should be enabled.");

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
    }

    private void OnDestroy() {
      if (_harmony != null) {
        _harmony.UnpatchAll(null);
      }
    }

    private static GameObject sectorInfoObject;
    private static Text sectorInfoText;
    private static Vector2i savedSector;

    [HarmonyPatch(typeof(Hud))]
    class HudPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Hud.Awake))]
      private static void HudAwakePostfix() {
        if (sectorInfoObject == null) {
          sectorInfoObject = new GameObject();
          sectorInfoObject.transform.SetParent(Hud.instance.m_statusEffectListRoot.transform.parent);
          sectorInfoObject.AddComponent<RectTransform>();

          MessageHud messageHud = MessageHud.instance;

          sectorInfoText = sectorInfoObject.AddComponent<Text>();
          sectorInfoText.color = Color.white;
          sectorInfoText.font = messageHud.m_messageCenterText.font;
          sectorInfoText.fontSize = messageHud.m_messageCenterText.fontSize;
          sectorInfoText.enabled = true;
          sectorInfoText.alignment = TextAnchor.MiddleCenter;
          sectorInfoText.horizontalOverflow = HorizontalWrapMode.Overflow;
          sectorInfoText.text = "SectorDisplay";
        }
      }
    }

    [HarmonyPatch(typeof(Player))]
    class PlayerPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Player.Update))]
      private static void PlayerUpdatePostfix(Player __instance) {
        if (__instance == null) {
          return;
        }

        Vector2i sector = ZoneSystem.instance.GetZone(ZNet.instance.GetReferencePosition());

        /*
        if (sector == savedSector) {
          return;
        }
        */
        // TODO(redseiko): also cache the List from ZDOMan.

        int sectorIndex = ZDOMan.instance.SectorToIndex(sector);
        long sectorCount =
            sectorIndex >= 0 && ZDOMan.instance.m_objectsBySector[sectorIndex] != null
                ? ZDOMan.instance.m_objectsBySector[sectorIndex].Count
                : -1;

        var staminaBarTransform = Hud.instance.m_staminaBar2Root.transform as RectTransform;
        var statusEffectListTransform = Hud.instance.m_statusEffectListRoot.transform as RectTransform;

        sectorInfoText.text = "Sector: " + sector + " (" + sectorCount + ")";
        sectorInfoObject.GetComponent<RectTransform>().position =
            new Vector2(staminaBarTransform.position.x, statusEffectListTransform.position.y);
        sectorInfoObject.SetActive(true);

        savedSector = sector;
      }
    }
  }
}
