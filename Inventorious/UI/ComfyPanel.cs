using UnityEngine;
using UnityEngine.UI;

namespace ComfyLib {
  public class ComfyPanel : MonoBehaviour {
    public RectTransform RectTransform { get; private set; }
    public Image Background { get; private set; }
    public Text Title { get; private set; }

    public static ComfyPanel CreatePanel(GameObject gameObject) {
      ComfyPanel panel = gameObject.AddComponent<ComfyPanel>();
      panel.CreatePanel();

      return panel;
    }

    void CreatePanel() {
      RectTransform = GetComponent<RectTransform>();
      (_, _, Background) = CreateChildBackground(RectTransform);
      (_, _, Title, _) = CreateChildTitle(RectTransform);

      PanelDragger dragger = gameObject.AddComponent<PanelDragger>();
      dragger.TargetRectTransform = RectTransform;
    }

    static (GameObject, RectTransform, Image) CreateChildBackground(RectTransform parentTransform) {
      GameObject background = new("Background");
      background.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform rectTransform =
          background.AddComponent<RectTransform>()
              .SetAnchorMin(Vector2.zero)
              .SetAnchorMax(Vector2.one)
              .SetPivot(new(0.5f, 0.5f))
              .SetPosition(Vector2.zero)
              .SetSizeDelta(Vector2.zero);

      Image image =
          background.AddComponent<Image>()
              .SetSprite(UIResources.GetSprite("woodpanel_crafting_240"))
              .SetMaterial(UIResources.GetMaterial("litpanel"))
              .SetType(Image.Type.Sliced)
              .SetColor(Color.white);

      return (background, rectTransform, image);
    }

    static (GameObject, RectTransform, Text, Outline) CreateChildTitle(RectTransform parentTransform) {
      GameObject title = new("Title");
      title.transform.SetParent(parentTransform, worldPositionStays: false);

      RectTransform rectTransform =
          title.AddComponent<RectTransform>()
              .SetAnchorMin(Vector2.up)
              .SetAnchorMax(Vector2.one)
              .SetPivot(new(0.5f, 1f))
              .SetPosition(new(0f, -20f))
              .SetSizeDelta(new(0f, 40f));

      Text text =
          title.AddComponent<Text>()
              .SetFont(UIResources.Norsebold)
              .SetFontSize(32)
              .SetColor(UIResources.Orange)
              .SetAlignment(TextAnchor.UpperCenter)
              .SetText("Title");

      Outline outline =
          title.AddComponent<Outline>()
              .SetEffectColor(Color.black)
              .SetEffectDistance(Vector2.one)
              .SetUseGraphicAlpha(false);

      return (title, rectTransform, text, outline);
    }
  }
}
