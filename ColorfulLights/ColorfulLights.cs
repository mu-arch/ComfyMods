using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using UnityEngine;

using static ColorfulLights.PluginConfig;

namespace ColorfulLights {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class ColorfulLights : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.colorfullights";
    public const string PluginName = "ColorfulLights";
    public const string PluginVersion = "1.7.0";

    public static readonly int FirePlaceColorHashCode = "FireplaceColor".GetStableHashCode();
    public static readonly int FireplaceColorAlphaHashCode = "FireplaceColorAlpha".GetStableHashCode();
    static readonly int _lightLastColoredByHashCode = "LightLastColoredBy".GetStableHashCode();

    static ManualLogSource _logger;

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      BindConfig(Config);

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
          Fireplace targetFireplace = _hoverObject.Ref()?.GetComponentInParent<Fireplace>();

          if (targetFireplace) {
            Player.m_localPlayer.StartCoroutine(ChangeFireplaceColorCoroutine(targetFireplace));
            return false;
          }
        }

        return takeInputResult;
      }
    }

    static IEnumerator ChangeFireplaceColorCoroutine(Fireplace targetFireplace) {
      yield return null;

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

      Vector3 colorVec3 = Utils.ColorToVec3(TargetFireplaceColor.Value);
      float colorAlpha = TargetFireplaceColor.Value.a;

      targetFireplace.m_nview.ClaimOwnership();
      targetFireplace.m_nview.m_zdo.Set(FirePlaceColorHashCode, colorVec3);
      targetFireplace.m_nview.m_zdo.Set(FireplaceColorAlphaHashCode, colorAlpha);
      targetFireplace.m_nview.m_zdo.Set(_lightLastColoredByHashCode, Player.m_localPlayer.GetPlayerID());

      targetFireplace.m_fuelAddedEffects?.Create(
          targetFireplace.transform.position, targetFireplace.transform.rotation);

      if (targetFireplace.TryGetComponent(out FireplaceColor fireplaceColor)) {
        fireplaceColor.SetColors(colorVec3, colorAlpha);
      }
    }

    [HarmonyPatch(typeof(Fireplace))]
    class FireplacePatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.Awake))]
      static void AwakePostfix(ref Fireplace __instance) {
        if (!IsModEnabled.Value || !__instance) {
          return;
        }

        if (!__instance.TryGetComponent(out FireplaceColor _)) {
          __instance.gameObject.AddComponent<FireplaceColor>();
        }
      }

      static readonly string _changeColorHoverTextTemplate =
          "{0}\n<size={4}>[<color={1}>{2}</color>] Change fire color to: <color=#{3}>#{3}</color></size>";

      [HarmonyPostfix]
      [HarmonyPatch(nameof(Fireplace.GetHoverText))]
      static void GetHoverTextPostfix(ref Fireplace __instance, ref string __result) {
        if (!IsModEnabled.Value || !ShowChangeColorHoverText.Value || !__instance) {
          return;
        }

        __result =
            Localization.instance.Localize(
                string.Format(
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
        if (IsModEnabled.Value && fireplace.TryGetComponent(out FireplaceColor fireplaceColor)) {
          Color color = fireplaceColor.TargetColor;
          rotation = fireplace.transform.rotation;
          rotation.x = color.r;
          rotation.y = color.g;
          rotation.z = color.b;
        }

        return rotation;
      }
    }

    [HarmonyPatch(typeof(ZNetScene))]
    class ZNetScenePatch {
      static readonly int _vfxFireWorkTestHashCode = "vfx_FireWorkTest".GetStableHashCode();

      [HarmonyPrefix]
      [HarmonyPatch(nameof(ZNetScene.RPC_SpawnObject))]
      static bool RPC_SpawnObjectPrefix(
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
