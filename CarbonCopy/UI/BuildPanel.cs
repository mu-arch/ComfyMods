using UnityEngine;
using UnityEngine.UI;

namespace CarbonCopy {
  public class BuildPanel {
    DefaultControls.Resources _resources;

    public GameObject Panel { get; private set; }
    public GameObject Header { get; private set; }
    public GameObject Content { get; private set; }

    public BuildPanel(Transform parentTransform) {
      _resources = UIResources.CreateResources();
      Panel = CreatePanel(parentTransform);
      Header = CreateHeader(Panel.transform, "CarbonCopy");
      Content = CreateContent(Panel.transform);
      CreateInputFieldButtonRow(Content.transform, "Filename", "Save Build");
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
      panelLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 10, bottom: 10);
      panelLayoutGroup.spacing = 10f;

      ContentSizeFitter panelFitter = panel.AddComponent<ContentSizeFitter>();
      panelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
      panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      panelTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400f);

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
      headerLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 0, bottom: 0);
      headerLayoutGroup.spacing = 10f;

      CreateIndicator(header.transform, new Color32(123, 36, 28, 255));

      GameObject label = DefaultControls.CreateText(_resources);
      label.transform.SetParent(header.transform, worldPositionStays: false);
      label.name = "Label";

      Text labelText = label.GetComponent<Text>();
      labelText.fontSize += 2;
      labelText.alignment = TextAnchor.MiddleCenter;
      labelText.text = headerLabel;

      CreateSpacer(header.transform, flexibleWidth: 1f);

      return header;
    }

    GameObject CreateIndicator(Transform parentTransform, Color indicatorColor) {
      GameObject indicator = DefaultControls.CreateImage(_resources);
      indicator.transform.SetParent(parentTransform, worldPositionStays: false);
      indicator.name = "Indicator";

      Image indicatorImage = indicator.GetComponent<Image>();
      indicatorImage.color = indicatorColor;
      indicatorImage.raycastTarget = true;
      indicatorImage.maskable = true;

      LayoutElement indicatorLayout = indicator.AddComponent<LayoutElement>();
      indicatorLayout.preferredWidth = 4;
      indicatorLayout.flexibleHeight = 1;

      return indicator;
    }

    GameObject CreateSpacer(Transform parentTransform, float flexibleWidth = 0, float flexibleHeight = 0) {
      GameObject spacer = new("Spacer", typeof(RectTransform), typeof(LayoutElement));
      spacer.transform.SetParent(parentTransform, worldPositionStays: false);

      LayoutElement spacerLayout = spacer.GetComponent<LayoutElement>();
      spacerLayout.flexibleWidth = flexibleWidth;
      spacerLayout.flexibleHeight = flexibleHeight;

      return spacer;
    }

    GameObject CreateContent(Transform parentTransform) {
      GameObject content = new("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
      content.transform.SetParent(parentTransform, worldPositionStays: false);

      VerticalLayoutGroup contentLayoutGroup = content.GetComponent<VerticalLayoutGroup>();
      contentLayoutGroup.childControlHeight = true;
      contentLayoutGroup.childControlWidth = true;
      contentLayoutGroup.childForceExpandHeight = false;
      contentLayoutGroup.childForceExpandWidth = false;
      contentLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      contentLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 0, bottom: 0);
      contentLayoutGroup.spacing = 10f;

      CreateDivider(content.transform, new Color32(123, 36, 28, 255));

      return content;
    }

    GameObject CreateDivider(Transform parentTransform, Color dividerColor) {
      GameObject divider = DefaultControls.CreateImage(_resources);
      divider.transform.SetParent(parentTransform, worldPositionStays: false);
      divider.name = "Divider";

      Image indicatorImage = divider.GetComponent<Image>();
      indicatorImage.color = dividerColor;
      indicatorImage.raycastTarget = true;
      indicatorImage.maskable = true;

      LayoutElement indicatorLayout = divider.AddComponent<LayoutElement>();
      indicatorLayout.flexibleWidth = 2;
      indicatorLayout.preferredHeight = 2;

      return divider;
    }

    GameObject CreateInputFieldButtonRow(Transform parentTransform, string inputFieldLabel, string buttonLabel) {
      GameObject row = new("InputFieldButtonRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
      row.transform.SetParent(parentTransform, worldPositionStays: false);

      HorizontalLayoutGroup rowLayoutGroup = row.GetComponent<HorizontalLayoutGroup>();
      rowLayoutGroup.childControlWidth = true;
      rowLayoutGroup.childControlHeight = false;
      rowLayoutGroup.childForceExpandWidth = false;
      rowLayoutGroup.childForceExpandHeight = false;
      rowLayoutGroup.padding = new RectOffset(left: 5, right: 5, top: 0, bottom: 0);
      rowLayoutGroup.spacing = 10f;

      GameObject label = DefaultControls.CreateText(_resources);
      label.transform.SetParent(row.transform, worldPositionStays: false);
      label.name = inputFieldLabel;

      Text labelText = label.GetComponent<Text>();
      labelText.alignment = TextAnchor.MiddleLeft;
      labelText.text = inputFieldLabel;

      CreateInputField(row.transform);
      CreateButton(row.transform, buttonLabel);

      return row;
    }

    GameObject CreateInputField(Transform parentTransform) {
      GameObject inputFieldControl = DefaultControls.CreateInputField(_resources);
      inputFieldControl.transform.SetParent(parentTransform, worldPositionStays: false);

      LayoutElement inputFieldLayout = inputFieldControl.AddComponent<LayoutElement>();
      inputFieldLayout.flexibleWidth = 1;

      InputField inputField = inputFieldControl.GetComponent<InputField>();

      Text inputFieldPlaceholderText = inputField.placeholder.GetComponent<Text>();
      inputFieldPlaceholderText.font = inputField.textComponent.font;

      return inputFieldControl;
    }

    GameObject CreateButton(Transform parentTransform, string buttonLabel) {
      GameObject buttonControl = DefaultControls.CreateButton(_resources);
      buttonControl.transform.SetParent(parentTransform, worldPositionStays: false);

      HorizontalLayoutGroup buttonLayoutGroup = buttonControl.AddComponent<HorizontalLayoutGroup>();
      buttonLayoutGroup.childControlWidth = true;
      buttonLayoutGroup.childControlHeight = true;
      buttonLayoutGroup.childForceExpandWidth = false;
      buttonLayoutGroup.childForceExpandHeight = false;
      buttonLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
      buttonLayoutGroup.padding = new RectOffset(left: 10, right: 10, top: 5, bottom: 5);

      Text buttonText = buttonControl.GetComponentInChildren<Text>(includeInactive: false);
      buttonText.text = buttonLabel;

      return buttonControl;
    }
  }
}
