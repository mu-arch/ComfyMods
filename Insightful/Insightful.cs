using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Collections;
using System.Reflection;

using UnityEngine;

using static Insightful.PluginConfig;

namespace Insightful {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Insightful : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.insightful";
    public const string PluginName = "Insightful";
    public const string PluginVersion = "1.2.0";

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

    public static readonly int InscriptionTopicHashCode = "InscriptionTopic".GetStableHashCode();
    public static readonly int InscriptionTextHashCode = "InscriptionText".GetStableHashCode();
    public static readonly int InscriptionStyleHashCode = "InscriptionStyle".GetStableHashCode();

    public static IEnumerator ReadHiddenTextCoroutine(GameObject runeStone) {
      yield return null;

      ZDO zdo = runeStone.Ref()?.GetComponentInParent<ZNetView>().Ref()?.m_zdo;

      if (zdo == null) {
        _logger.LogInfo("No ZNetView/ZDO found for RuneStone.");
        yield break;
      }

      if (!zdo.TryGetString(InscriptionTopicHashCode, out string inscriptionTopic)
          || !zdo.TryGetString(InscriptionTextHashCode, out string inscriptionText)) {
        _logger.LogInfo("RuneStone does not have custom Inscription Topic or Text.");
        yield break;
      }

      _logger.LogInfo($"Found hidden Inscription on RuneStone: {zdo.m_uid}");

      if (!zdo.TryGetEnum(InscriptionStyleHashCode, out TextViewer.Style style)) {
        style = TextViewer.Style.Rune;
      }

      TextViewer.m_instance.ShowText(style, inscriptionTopic, inscriptionText, autoHide: true);
    }
  }
}