using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulLights : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfullights";
    public const string PluginName = "ColorfulLights";
    public const string PluginVersion = "1.7.0";

    static readonly int _fireplaceColorHashCode = "FireplaceColor".GetStableHashCode();
    static readonly int _fireplaceColorAlphaHashCode = "FireplaceColorAlpha".GetStableHashCode();
    static readonly int _lightLastColoredByHashCode = "LightLastColoredBy".GetStableHashCode();

    static ManualLogSource _logger;
    static readonly Dictionary<Fireplace, FireplaceData> _fireplaceDataCache = new();

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      Configure(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);

      StartCoroutine(RemoveDestroyedFireplacesCoroutine());
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static IEnumerator RemoveDestroyedFireplacesCoroutine() {
      WaitForSeconds waitThirtySeconds = new(seconds: 30f);
      List<KeyValuePair<Fireplace, FireplaceData>> existingFireplaces = new();
      int fireplaceCount = 0;

      while (true) {
        yield return waitThirtySeconds;
        fireplaceCount = _fireplaceDataCache.Count;

        existingFireplaces.AddRange(_fireplaceDataCache.Where(entry => entry.Key));
        _fireplaceDataCache.Clear();

        foreach (KeyValuePair<Fireplace, FireplaceData> entry in existingFireplaces) {
          _fireplaceDataCache[entry.Key] = entry.Value;
        }

        existingFireplaces.Clear();

        if (fireplaceCount > 0) {
          _logger.LogInfo($"Removed {fireplaceCount - _fireplaceDataCache.Count}/{fireplaceCount} fireplace refs.");
        }
      }
    }

    static bool TryGetFireplace(Fireplace key, out FireplaceData value) {
      if (key) {
        return _fireplaceDataCache.TryGetValue(key, out value);
      }

      value = default;
      return false;
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

      static GameObject _hoverObject;

      static bool TakeInputDelegate(bool takeInputResult) {
        if (!IsModEnabled.Value) {
          return takeInputResult;
        }

        _hoverObject = Player.m_localPlayer.Ref()?.m_hovering;

        if (!_hoverObject) {
          return takeInputResult;
        }

        if (ChangeColorActionShortcut.Value.IsDown()) {
          Player.m_localPlayer.StartCoroutine(ChangeFireplaceColorCoroutine(_hoverObject));
          return false;
        }

        return takeInputResult;
      }
    }

    static IEnumerator ChangeFireplaceColorCoroutine(GameObject target) {
      yield return null;

      Fireplace targetFireplace = target.Ref()?.GetComponentInParent<Fireplace>();

      if (!targetFireplace) {
        yield break;
      }

      if (!targetFireplace.m_nview || !targetFireplace.m_nview.IsValid()) {
        _logger.LogWarning("Fireplace does not have a valid ZNetView.");
        yield break;
      }

      if (!PrivateArea.CheckAccess(targetFireplace.transform.position, radius: 0f, flash: true, wardCheck: false)) {
        _logger.LogWarning("Fireplace is within private area with no access.");
        yield break;
      }

      targetFireplace.m_nview.ClaimOwnership();

      targetFireplace.m_nview.m_zdo.Set(_fireplaceColorHashCode, Utils.ColorToVec3(TargetFireplaceColor.Value));
      targetFireplace.m_nview.m_zdo.Set(_fireplaceColorAlphaHashCode, TargetFireplaceColor.Value.a);
      targetFireplace.m_nview.m_zdo.Set(_lightLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());

      targetFireplace.m_fuelAddedEffects?.Create(
          targetFireplace.transform.position, targetFireplace.transform.rotation);

      if (TryGetFireplace(targetFireplace, out FireplaceData fireplaceData)) {
        SetParticleColors(
            fireplaceData.Lights, fireplaceData.Systems, fireplaceData.Renderers, TargetFireplaceColor.Value);

        fireplaceData.TargetColor = TargetFireplaceColor.Value;
      }
    }

    [HarmonyPatch(typeof(Fireplace))]
    class FireplacePatch {


      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static void FireplaceAwakePostfix(ref Fireplace __instance) {
        if (!IsModEnabled.Value || !__instance) {
          return;
        }

        _fireplaceDataCache.Add(__instance, new(__instance));
      }

      static readonly string _changeColorHoverTextTemplate =
          "{0}\n<size={4}>[<color={1}>{2}</color>] Change fire color to: <color=#{3}>#{3}</color></size>";

      static readonly string _clearColorHoverTextTemplate =
          "{0}\n<size={3}>[<color={1}>{2}</color>] Clear existing fire color</size>";

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.GetHoverText))]
      static void FireplaceGetHoverTextPostfix(ref Fireplace __instance, ref string __result) {
        if (!IsModEnabled.Value || !ShowChangeColorHoverText.Value || !__instance) {
          return;
        }

        __result =
            Localization.instance.Localize(
                ClearExistingFireplaceColor.Value
                    ? string.Format(
                          _clearColorHoverTextTemplate,
                          __result,
                          "#FFA726",
                          ChangeColorActionShortcut.Value,
                          ColorPromptFontSize.Value)
                    : string.Format(
                          _changeColorHoverTextTemplate,
                          __result,
                          "#FFA726",
                          ChangeColorActionShortcut.Value,
                          TargetFireplaceColor.Value.GetColorHtmlString(),
                          ColorPromptFontSize.Value));
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(Fireplace.UseItem))]
      static IEnumerable<CodeInstruction> UseItemTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(OpCodes.Call, typeof(Component).GetProperty(nameof(transform))),
                new CodeMatch(OpCodes.Callvirt, typeof(Transform).GetProperty(nameof(Transform.position))),
                new CodeMatch(OpCodes.Call, typeof(Quaternion).GetProperty(nameof(Quaternion.identity))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, typeof(Fireplace).GetField(nameof(Fireplace.m_fireworks))),
                new CodeMatch(OpCodes.Callvirt, typeof(ZNetScene).GetMethod(nameof(ZNetScene.SpawnObject))))
            .Advance(offset: 3)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
            .InsertAndAdvance(
                Transpilers.EmitDelegate<Func<Quaternion, Fireplace, Quaternion>>(SpawnObjectQuaternionDelegate))
            .InstructionEnumeration();
      }

      static Quaternion SpawnObjectQuaternionDelegate(Quaternion rotation, Fireplace fireplace) {
        if (IsModEnabled.Value && TryGetFireplace(fireplace, out FireplaceData fireplaceData)) {
          rotation = fireplace.transform.rotation;
          rotation.x = fireplaceData.TargetColor.r;
          rotation.y = fireplaceData.TargetColor.g;
          rotation.z = fireplaceData.TargetColor.b;
        }

        return rotation;
      }

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.UpdateFireplace))]
      static void FireplaceUpdateFireplacePostfix(ref Fireplace __instance) {
        if (!IsModEnabled.Value
            || !__instance.m_nview
            || __instance.m_nview.m_zdo == null
            || __instance.m_nview.m_zdo.m_zdoMan == null
            || __instance.m_nview.m_zdo.m_vec3 == null
            || !__instance.m_nview.m_zdo.m_vec3.ContainsKey(_fireplaceColorHashCode)
            || !TryGetFireplace(__instance, out FireplaceData fireplaceData)) {
          return;
        }

        Color fireplaceColor = Utils.Vec3ToColor(__instance.m_nview.m_zdo.m_vec3[_fireplaceColorHashCode]);
        fireplaceColor.a = __instance.m_nview.m_zdo.GetFloat(_fireplaceColorAlphaHashCode, 1f);

        SetParticleColors(fireplaceData.Lights, fireplaceData.Systems, fireplaceData.Renderers, fireplaceColor);
        fireplaceData.TargetColor = fireplaceColor;
      }
    }

    [HarmonyPatch(typeof(ZNetScene))]
    class ZNetScenePatch {
      static readonly int _vfxFireWorkTestHashCode = "vfx_FireWorkTest".GetStableHashCode();

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZNetScene.RPC_SpawnObject))]
      static bool ZNetSceneRPC_SpawnObjectPrefix(
          ref ZNetScene __instance, Vector3 pos, Quaternion rot, int prefabHash) {
        if (!IsModEnabled.Value || prefabHash != _vfxFireWorkTestHashCode || rot == Quaternion.identity) {
          return true;
        }

        Color fireworksColor = Utils.Vec3ToColor(new Vector3(rot.x, rot.y, rot.z));

        _logger.LogInfo($"Spawning fireworks with color: {fireworksColor}");
        GameObject fireworksClone = Instantiate(__instance.GetPrefab(prefabHash), pos, rot);

        SetParticleColors(
            Enumerable.Empty<Light>(),
            fireworksClone.GetComponentsInChildren<ParticleSystem>(includeInactive: true),
            fireworksClone.GetComponentsInChildren<ParticleSystemRenderer>(includeInactive: true),
            fireworksColor);

        return false;
      }
    }

    static void SetParticleColors(
        IEnumerable<Light> lights,
        IEnumerable<ParticleSystem> systems,
        IEnumerable<ParticleSystemRenderer> renderers,
        Color targetColor) {
      var targetColorGradient = new ParticleSystem.MinMaxGradient(targetColor);

      foreach (ParticleSystem system in systems) {
        var colorOverLifetime = system.colorOverLifetime;

        if (colorOverLifetime.enabled) {
          colorOverLifetime.color = targetColorGradient;
        }

        var sizeOverLifetime = system.sizeOverLifetime;

        if (sizeOverLifetime.enabled) {
          var main = system.main;
          main.startColor = targetColor;
        }
      }

      foreach (ParticleSystemRenderer renderer in renderers) {
        renderer.material.color = targetColor;
      }

      foreach (Light light in lights) {
        light.color = targetColor;
      }
    }
  }

  public static class ObjectExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
