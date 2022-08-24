using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static LetMePlay.PluginConfig;

namespace LetMePlay {
  [HarmonyPatch(typeof(ItemDrop.ItemData))]
  public class ItemDataPatch {
    static readonly Dictionary<string, Sprite> SpriteCache = new();

    static Sprite GetSprite(string spriteName) {
      if (!SpriteCache.TryGetValue(spriteName, out Sprite sprite)) {
        sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(obj => obj.name == spriteName);
        SpriteCache[spriteName] = sprite;
      }

      return sprite;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemDrop.ItemData.GetIcon))]
    static void ItemDataGetIcon(ref ItemDrop.ItemData __instance) {
      if (!IsModEnabled.Value) {
        return;
      }

      if (__instance.m_variant < 0 || __instance.m_variant >= __instance.m_shared.m_icons.Length) {
        Array.Resize(ref __instance.m_shared.m_icons, __instance.m_variant + 1);

        __instance.m_shared.m_icons[__instance.m_variant] = GetSprite("hammer_icon_small");
        __instance.m_shared.m_name = __instance.m_dropPrefab.name;
        __instance.m_shared.m_description = $"Non-player item: {__instance.m_dropPrefab.name}";
        __instance.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Misc;
        __instance.m_crafterID = 12345678L;
        __instance.m_crafterName = "redseiko.valheim.letmeplay";
      }
    }
  }
}
