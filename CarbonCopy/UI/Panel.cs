using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CarbonCopy {
  public class Panel {
    DefaultControls.Resources _resources;
    GameObject _panel;

    public Panel(Transform parentTransform) {
      _resources = UIResources.CreateResources();
      _panel = CreatePanel(parentTransform);
    }

    GameObject CreatePanel(Transform parentTransform) {
      GameObject panel = DefaultControls.CreatePanel(_resources);
      panel.transform.SetParent(parentTransform, worldPositionStays: false);
      panel.name = "Panel";

      RectTransform panelTransform = panel.GetComponent<RectTransform>();
      panelTransform.anchorMin = new Vector2(0f, 0.5f);
      panelTransform.anchorMax = new Vector2(0f, 0.5f);
      panelTransform.pivot = new Vector2(0f, 0.5f);
      panelTransform.anchoredPosition = new Vector2(10f, 0f);

      VerticalLayoutGroup panelLayoutGroup = panel.AddComponent<VerticalLayoutGroup>();
      panelLayoutGroup.childControlHeight = true;
      panelLayoutGroup.childControlWidth = true;
      panelLayoutGroup.childForceExpandHeight = false;
      panelLayoutGroup.childForceExpandWidth = false;
      panelLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      panelLayoutGroup.padding = new RectOffset(left: 10, right: 10, top: 0, bottom: 0);
      panelLayoutGroup.spacing = 10f;

      ContentSizeFitter panelFitter = panel.AddComponent<ContentSizeFitter>();
      panelFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      CreateHeader(panel.transform, "CarbonCopy");

      return panel;
    }

    GameObject CreateHeader(Transform parentTransform, string headerLabel) {
      GameObject header = new("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
      header.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform headerTransform = header.GetComponent<RectTransform>();
      headerTransform.anchorMin = new Vector2(0.5f, 0.5f);
      headerTransform.anchorMax = new Vector2(0.5f, 0.5f);
      headerTransform.pivot = new Vector2(0.5f, 0.5f);

      HorizontalLayoutGroup headerLayoutGroup = header.GetComponent<HorizontalLayoutGroup>();
      headerLayoutGroup.childControlHeight = true;
      headerLayoutGroup.childControlWidth = true;
      headerLayoutGroup.childForceExpandHeight = false;
      headerLayoutGroup.childForceExpandWidth = false;
      headerLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      headerLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 10, bottom: 0);
      headerLayoutGroup.spacing = 10f;

      CreateIndicator(header.transform, new Color(123, 36, 28, 255));

      GameObject label = DefaultControls.CreateText(_resources);
      label.transform.SetParent(header.transform, worldPositionStays: false);
      label.name = "Label";

      Text labelText = label.GetComponent<Text>();
      labelText.alignment = TextAnchor.MiddleCenter;
      labelText.text = headerLabel;

      return header;
    }

    GameObject CreateIndicator(Transform parentTransform, Color indicatorColor) {
      GameObject indicator = DefaultControls.CreateImage(_resources);
      indicator.transform.SetParent(parentTransform, worldPositionStays: false);
      indicator.name = "Indicator";

      RectTransform indicatorTransform = indicator.GetComponent<RectTransform>();
      indicatorTransform.anchorMin = new Vector2(0.5f, 0.5f);
      indicatorTransform.anchorMax = new Vector2(0.5f, 0.5f);
      indicatorTransform.pivot = new Vector2(0.5f, 0.5f);

      Image indicatorImage = indicator.GetComponent<Image>();
      indicatorImage.color = indicatorColor;
      indicatorImage.raycastTarget = true;
      indicatorImage.maskable = true;

      LayoutElement indicatorLayout = indicator.AddComponent<LayoutElement>();
      indicatorLayout.preferredWidth = 4;
      indicatorLayout.flexibleHeight = 1;

      return indicator;
    }
  }
}
