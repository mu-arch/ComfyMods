using UnityEngine;
using UnityEngine.UI;

using static PartyRock.UIBuilder;

namespace PartyRock {
  public class ProgressBar {
    public GameObject Background { get; }
    public GameObject Bar { get; }
    public GameObject CurrentValue { get; }
    public GameObject MaxValue { get; }

    public ProgressBar(Transform parentTransform) {
      Background = CreateBarBackground(parentTransform);
      Bar = CreateBar(Background.transform);
      (CurrentValue, MaxValue) = CreateValueLabels(Bar.transform);
    }

    GameObject CreateBarBackground(Transform parentTransform) {
      GameObject barBackground = CreateRow(parentTransform);
      barBackground.SetName("Bar.Background");

      barBackground.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 2, right: 2, top: 2, bottom: 2);

      barBackground.AddComponent<Image>()
          .SetColor(new Color(0f, 0f, 0f, 0.4f));

      return barBackground;
    }

    GameObject CreateBar(Transform parentTransform) {
      GameObject bar = CreateRow(parentTransform);
      bar.SetName("Bar");

      bar.GetComponent<HorizontalLayoutGroup>()
          .SetPadding(left: 10, right: 10, top: 3, bottom: 3)
          .SetChildAlignment(TextAnchor.MiddleLeft)
          .SetSpacing(10f);

      bar.AddComponent<LayoutElement>()
          .SetPreferred(width: 150f);

      bar.AddComponent<Image>()
          .SetType(Image.Type.Filled)
          .SetFillMethod(Image.FillMethod.Horizontal)
          .SetFillOrigin(Image.OriginHorizontal.Left)
          .SetFillAmount(1f)
          .SetSprite(CreateGradientSprite())
          .SetColor(new(0f, 0.6f, 0f, 0.95f));

      return bar;
    }

    (GameObject, GameObject) CreateValueLabels(Transform parentTransform) {
      GameObject currentValueLabel = CreateLabel(parentTransform);
      currentValueLabel.SetName("Value.Current");

      currentValueLabel.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      currentValueLabel.Text()
          .SetAlignment(TextAnchor.MiddleRight)
          .SetFontStyle(FontStyle.Italic)
          .SetFontSize(UIResources.AveriaSerifLibre.fontSize - 1)
          .SetText("25");

      GameObject dividerLabel = CreateLabel(parentTransform);
      dividerLabel.SetName("Value.Divider");

      dividerLabel.Text()
          .SetAlignment(TextAnchor.MiddleCenter)
          .SetFontStyle(FontStyle.Italic)
          .SetFontSize(UIResources.AveriaSerifLibre.fontSize - 1)
          .SetText("/");

      GameObject maxValueLabel = CreateLabel(parentTransform);
      maxValueLabel.SetName("Value.Max");

      maxValueLabel.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      maxValueLabel.Text()
          .SetAlignment(TextAnchor.MiddleLeft)
          .SetFontStyle(FontStyle.Italic)
          .SetFontSize(UIResources.AveriaSerifLibre.fontSize - 1)
          .SetText("25");

      return (currentValueLabel, maxValueLabel);
    }
  }
}
