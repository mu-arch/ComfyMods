using BepInEx;

using HarmonyLib;

using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEngine.UI;

using static Pinnacle.PluginConfig;

namespace Pinnacle {
  [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
  public class Pinnacle : BaseUnityPlugin {
    public const string PluginGuid = "redseiko.valheim.pinnacle";
    public const string PluginName = "Pinnacle";
    public const string PluginVersion = "1.0.0";

    Harmony _harmony;

    public void Awake() {
      BindConfig(Config);

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGuid);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    static PinEditPanel _pinEditPanel;

    public static void TogglePinEditPanel(Minimap.PinData pin) {
      if (!_pinEditPanel?.Panel) {
        _pinEditPanel = new(Minimap.m_instance.m_largeRoot.transform);
        _pinEditPanel.Panel.RectTransform()
            .SetAnchorMin(new(0.5f, 0f))
            .SetAnchorMax(new(0.5f, 0f))
            .SetPivot(new(0.5f, 0f))
            .SetPosition(new(0f, 25f))
            .SetSizeDelta(new(200f, 200f));
      }

      if (pin == null) {
        _pinEditPanel.Panel.SetActive(false);
      } else {
        _pinEditPanel.PinName.Value.InputField.text = pin.m_name;
        _pinEditPanel.PinIconSelector.SetTargetPin(pin);

        _pinEditPanel.PinType.Value.InputField.text = pin.m_type.ToString();

        _pinEditPanel.PositionX.InputField.text = $"{pin.m_pos.x:F0}";
        _pinEditPanel.PositionY.InputField.text = $"{pin.m_pos.y:F0}";
        _pinEditPanel.PositionZ.InputField.text = $"{pin.m_pos.z:F0}";

        _pinEditPanel.Panel.SetActive(true);
      }
    }
  }
}