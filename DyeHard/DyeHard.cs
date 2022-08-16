using BepInEx;

using HarmonyLib;

using System.Reflection;

using UnityEngine;

using static DyeHard.PluginConfig;

namespace DyeHard {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class DyeHard : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.dyehard";
    public const string PluginName = "DyeHard";
    public const string PluginVersion = "1.4.0";

    public static readonly int HairColorHashCode = "HairColor".GetStableHashCode();
    public static Player LocalPlayerCache { get; set; }

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static void SetCharacterPreviewPosition() {
      if (FejdStartup.m_instance && FejdStartup.m_instance.m_playerInstance) {
        FejdStartup.m_instance.m_playerInstance.transform.position =
            FejdStartup.m_instance.m_characterPreviewPoint.position
                + (IsModEnabled.Value ? OffsetCharacterPreviewPosition.Value : Vector3.zero);
      }
    }

    public static Vector3 GetPlayerHairColorVector() {
      Vector3 colorVector = Utils.ColorToVec3(PlayerHairColor.Value);

      if (colorVector != Vector3.zero) {
        colorVector *= PlayerHairGlow.Value / colorVector.magnitude;
      }

      return colorVector;
    }

    public static void SetPlayerZdoHairColor() {
      if (!LocalPlayerCache || !LocalPlayerCache.m_visEquipment) {
        return;
      }

      Vector3 color =
          IsModEnabled.Value && OverridePlayerHairColor.Value
              ? GetPlayerHairColorVector()
              : LocalPlayerCache.m_hairColor;

      LocalPlayerCache.m_visEquipment.m_hairColor = color;

      if (!LocalPlayerCache.m_nview || !LocalPlayerCache.m_nview.IsValid()) {
        return;
      }

      if (LocalPlayerCache.m_nview.m_zdo.m_vec3 == null
          || !LocalPlayerCache.m_nview.m_zdo.m_vec3.ContainsKey(HairColorHashCode)
          || LocalPlayerCache.m_nview.m_zdo.m_vec3[HairColorHashCode] != color) {
        LocalPlayerCache.m_nview.GetZDO().Set(HairColorHashCode, color);
      }
    }

    public static void SetPlayerHairItem() {
      if (!LocalPlayerCache || !LocalPlayerCache.m_visEquipment) {
        return;
      }

      string hairItem =
          IsModEnabled.Value && OverridePlayerHairItem.Value ? PlayerHairItem.Value : LocalPlayerCache.m_hairItem;

      if (LocalPlayerCache.m_nview) {
        LocalPlayerCache.m_visEquipment.SetHairItem(hairItem);
      }

      LocalPlayerCache.m_visEquipment.m_hairItem = hairItem;
    }

    public static void SetPlayerBeardItem() {
      if (!LocalPlayerCache || !LocalPlayerCache.m_visEquipment) {
        return;
      }

      string beardItem =
          IsModEnabled.Value && OverridePlayerBeardItem.Value ? PlayerBeardItem.Value : LocalPlayerCache.m_beardItem;

      if (LocalPlayerCache.m_nview) {
        LocalPlayerCache.m_visEquipment.SetBeardItem(beardItem);
      }

      LocalPlayerCache.m_visEquipment.m_beardItem = beardItem;
    }
  }
}