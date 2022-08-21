using ComfyLib;

using System;

using UnityEngine;
using UnityEngine.UI;

namespace SearsCatalog {
  public class PieceListRow {
    public GameObject Row { get; private set; }

    public Image PieceIcon { get; private set; }
    public Text PieceName { get; private set; }

    public PieceListRow(Transform parentTransform) {
      Row = CreateChildRow(parentTransform);

      PieceIcon = CreateChildPieceIcon(Row.transform).Image();
      PieceName = CreateChildPieceName(Row.transform).Text();

      CreateChildRowSpacer(Row.transform);
    }

    public void SetContent(Piece piece) {
      PieceIcon.SetSprite(piece.m_icon);
      PieceName.SetText(Localization.m_instance.Localize(piece.m_name));
    }

    GameObject CreateChildRow(Transform parentTransform) {
      GameObject row = new("PieceListRow", typeof(RectTransform));
      row.SetParent(parentTransform);

      row.AddComponent<HorizontalLayoutGroup>()
          .SetChildControl(width: true, height: true)
          .SetChildForceExpand(width: false, height: false)
          .SetChildAlignment(TextAnchor.MiddleCenter)
          .SetPadding(left: 5, right: 5, top: 5, bottom: 5)
          .SetSpacing(5f);

      row.AddComponent<Image>()
          .SetType(Image.Type.Sliced)
          .SetSprite(UIBuilder.CreateRoundedCornerSprite(400, 400, 5));

      row.AddComponent<Button>()
          .SetNavigationMode(Navigation.Mode.None)
          .SetTargetGraphic(row.Image())
          .SetColors(ButtonColorBlock.Value);

      row.AddComponent<ContentSizeFitter>()
          .SetHorizontalFit(ContentSizeFitter.FitMode.Unconstrained)
          .SetVerticalFit(ContentSizeFitter.FitMode.PreferredSize);

      row.AddComponent<ParentSizeFitter>();

      return row;
    }

    GameObject CreateChildPieceIcon(Transform parentTransform) {
      GameObject icon = new("Piece.Icon", typeof(RectTransform));
      icon.SetParent(parentTransform);

      icon.AddComponent<LayoutElement>()
          .SetPreferred(width: 20f, height: 20f);

      icon.AddComponent<Image>()
          .SetType(Image.Type.Simple);

      return icon;
    }

    GameObject CreateChildPieceName(Transform parentTransform) {
      GameObject name = UIBuilder.CreateLabel(parentTransform);
      name.SetName("Piece.Name");

      return name;
    }

    GameObject CreateChildRowSpacer(Transform parentTransform) {
      GameObject spacer = new($"Spacer", typeof(RectTransform));
      spacer.SetParent(parentTransform);

      spacer.AddComponent<LayoutElement>()
          .SetFlexible(width: 1f);

      return spacer;
    }

    static readonly Lazy<ColorBlock> ButtonColorBlock =
        new(() =>
          new() {
            normalColor = new Color(0f, 0f, 0f, 0.01f),
            highlightedColor = new Color32(50, 161, 217, 128),
            disabledColor = new Color(0f, 0f, 0f, 0.1f),
            pressedColor = new Color32(50, 161, 217, 192),
            selectedColor = new Color32(50, 161, 217, 248),
            colorMultiplier = 1f,
            fadeDuration = 0f,
          });
  }
}
