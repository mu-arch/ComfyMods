using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static ColorfulPortals.PluginConfig;

namespace ColorfulPortals {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulPortals : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfulportals";
    public const string PluginName = "ColorfulPortals";
    public const string PluginVersion = "1.6.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
      StartCoroutine(RemovedDestroyedTeleportWorldsCoroutine());
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    public static readonly Color NoColor = new(-1f, -1f, -1f);
    public static readonly Vector3 NoColorVector3 = new(-1f, -1f, -1f);

    public static readonly int ColorShaderId = Shader.PropertyToID("_Color");

    public static readonly int TeleportWorldColorHashCode = "TeleportWorldColor".GetStableHashCode();
    public static readonly int TeleportWorldColorAlphaHashCode = "TeleportWorldColorAlpha".GetStableHashCode();
    public static readonly int PortalLastColoredByHashCode = "PortalLastColoredBy".GetStableHashCode();

    static readonly Dictionary<TeleportWorld, TeleportWorldData> TeleportWorldDataCache = new();

    static IEnumerator RemovedDestroyedTeleportWorldsCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 30f);
      List<KeyValuePair<TeleportWorld, TeleportWorldData>> existingCache = new();

      while (true) {
        yield return waitInterval;

        long count = TeleportWorldDataCache.Count;

        if (count == 0) {
          continue;
        }

        existingCache.AddRange(TeleportWorldDataCache.Where(entry => entry.Key));

        if (count == existingCache.Count) {
          _logger.LogInfo($"TeleportWorldData cache size: {count}");
          existingCache.Clear();
          continue;
        }

        TeleportWorldDataCache.Clear();

        foreach (KeyValuePair<TeleportWorld, TeleportWorldData> entry in existingCache) {
          if (entry.Key) {
            TeleportWorldDataCache[entry.Key] = entry.Value;
          }
        }

        _logger.LogInfo($"Removed {count - existingCache.Count}/{count} TeleportWorldData cache references.");
        existingCache.Clear();
      }
    }

    static bool TryGetTeleportWorld(TeleportWorld key, out TeleportWorldData value) {
      if (key) {
        return TeleportWorldDataCache.TryGetValue(key, out value);
      }

      value = default;
      return false;
    }

    public static void ChangePortalColor(TeleportWorld targetPortal) {
      if (!targetPortal) {
        return;
      }

      if (!targetPortal.m_nview || !targetPortal.m_nview.IsValid()) {
        _logger.LogWarning("TeleportWorld does not have a valid ZNetView.");
        return;
      }

      if (!PrivateArea.CheckAccess(targetPortal.transform.position, flash: true)) {
        _logger.LogWarning("TeleportWorld is within a PrivateArea.");
        return;
      }

      targetPortal.m_nview.ClaimOwnership();

      targetPortal.m_nview.m_zdo.Set(TeleportWorldColorHashCode, Utils.ColorToVec3(TargetPortalColor.Value));
      targetPortal.m_nview.m_zdo.Set(TeleportWorldColorAlphaHashCode, TargetPortalColor.Value.a);
      targetPortal.m_nview.m_zdo.Set(PortalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);

      if (TeleportWorldDataCache.TryGetValue(targetPortal, out TeleportWorldData teleportWorldData)) {
        teleportWorldData.TargetColor = TargetPortalColor.Value;
        SetTeleportWorldColors(teleportWorldData);
      } else if (targetPortal.TryGetComponent(out TeleportWorldColor teleportWorldColor)) {
        teleportWorldColor.UpdatePortalColors(true);
      }
    }

    [HarmonyPatch(typeof(TeleportWorld))]
    class TeleportWorldPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(TeleportWorld.Awake))]
      static void TeleportWorldAwakePostfix(ref TeleportWorld __instance) {
        if (!IsModEnabled.Value || !__instance) {
          return;
        }

        if (__instance.m_proximityRoot) {
          __instance.gameObject.AddComponent<TeleportWorldColor>();
          return;
        }

        // Stone 'portal' prefab does not set this property.
        if (!__instance.m_proximityRoot) {
          __instance.m_proximityRoot = __instance.transform;
        }

        // Stone 'portal' prefab does not set this property.
        if (!__instance.m_target_found) {
          // The prefab does not have '_target_found_red' but instead '_target_found'.
          GameObject targetFoundObject = __instance.gameObject.transform.Find("_target_found").gameObject;

          // Disable the GameObject first, as adding component EffectFade calls its Awake() before being attached.
          targetFoundObject.SetActive(false);
          __instance.m_target_found = targetFoundObject.AddComponent<EffectFade>();
          targetFoundObject.SetActive(true);
        }

        TeleportWorldDataCache.Add(__instance, new TeleportWorldData(__instance));
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(TeleportWorld.GetHoverText))]
      static void TeleportWorldGetHoverTextPostfix(ref TeleportWorld __instance, ref string __result) {
        if (!IsModEnabled.Value || !ShowChangeColorHoverText.Value || !__instance) {
          return;
        }

        __result =
            string.Format(
                "{0}\n[<color={1}>{2}</color>] Change color to: <color={3}>{3}</color>",
                __result,
                "#FFA726",
                ChangePortalColorShortcut.Value,
                TargetPortalColorHex.Value);
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(TeleportWorld.UpdatePortal))]
      static void TeleportWorldUpdatePortalPostfix(ref TeleportWorld __instance) {
        if (IsModEnabled.Value && __instance.TryGetComponent(out TeleportWorldColor teleportWorldColor)) {
          teleportWorldColor.UpdatePortalColors();
          return;
        }

        if (!IsModEnabled.Value
            || !__instance
            || !__instance.m_nview
            || __instance.m_nview.m_zdo == null
            || __instance.m_nview.m_zdo.IsValid()
            || __instance.m_nview.m_zdo.GetVec3(TeleportWorldColorHashCode, Vector3.zero) == Vector3.zero
            || !TeleportWorldDataCache.TryGetValue(__instance, out TeleportWorldData teleportWorldData)) {
          return;
        }

        Color portalColor =
            Utils.Vec3ToColor(__instance.m_nview.m_zdo.GetVec3(TeleportWorldColorHashCode, Vector3.zero));
        portalColor.a = __instance.m_nview.m_zdo.GetFloat(TeleportWorldColorAlphaHashCode, defaultValue: 1f);

        teleportWorldData.TargetColor = portalColor;
        SetTeleportWorldColors(teleportWorldData);
      }
    }

    static void SetTeleportWorldColors(TeleportWorldData teleportWorldData) {
      foreach (Light light in teleportWorldData.Lights) {
        light.color = teleportWorldData.TargetColor;
      }

      foreach (ParticleSystem system in teleportWorldData.Systems) {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(teleportWorldData.TargetColor);

        ParticleSystem.MainModule main = system.main;
        main.startColor = teleportWorldData.TargetColor;
      }

      foreach (Material material in teleportWorldData.Materials) {
        material.color = teleportWorldData.TargetColor;
      }
    }
  }

  internal static class TeleportWorldExtension {
    public static IEnumerable<T> GetComponentsInNamedChild<T>(this TeleportWorld teleportWorld, string childName) {
      return teleportWorld.GetComponentsInChildren<Transform>(includeInactive: true)
          .Where(transform => transform.name == childName)
          .Select(transform => transform.GetComponent<T>())
          .Where(component => component != null);
      }
  }
}