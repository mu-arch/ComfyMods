using System;

using UnityEngine;
using UnityEngine.UI;

namespace Chatter {
  public static class ContentSizeFitterExtensions {
    public static ContentSizeFitter SetHorizontalFit(
        this ContentSizeFitter fitter, ContentSizeFitter.FitMode fitMode) {
      fitter.horizontalFit = fitMode;
      return fitter;
    }

    public static ContentSizeFitter SetVerticalFit(this ContentSizeFitter fitter, ContentSizeFitter.FitMode fitMode) {
      fitter.verticalFit = fitMode;
      return fitter;
    }
  }

  public static class HorizontalLayoutGroupExtensions {
    public static HorizontalLayoutGroup SetChildControl(
        this HorizontalLayoutGroup layoutGroup, bool? width = null, bool? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutGroup.childControlWidth = width.Value;
      }

      if (height.HasValue) {
        layoutGroup.childControlHeight = height.Value;
      }

      return layoutGroup;
    }

    public static HorizontalLayoutGroup SetChildForceExpand(
        this HorizontalLayoutGroup layoutGroup, bool? width = null, bool? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutGroup.childForceExpandWidth = width.Value;
      }

      if (height.HasValue) {
        layoutGroup.childForceExpandHeight = height.Value;
      }

      return layoutGroup;
    }

    public static HorizontalLayoutGroup SetChildAlignment(
        this HorizontalLayoutGroup layoutGroup, TextAnchor alignment) {
      layoutGroup.childAlignment = alignment;
      return layoutGroup;
    }

    public static HorizontalLayoutGroup SetPadding(
        this HorizontalLayoutGroup layoutGroup,
        int? left = null,
        int? right = null,
        int? top = null,
        int? bottom = null) {
      if (!left.HasValue && !right.HasValue && !top.HasValue && !bottom.HasValue) {
        throw new ArgumentException("Value for left, right, top or bottom must be provided.");
      }

      if (left.HasValue) {
        layoutGroup.padding.left = left.Value;
      }

      if (right.HasValue) {
        layoutGroup.padding.right = right.Value;
      }

      if (top.HasValue) {
        layoutGroup.padding.top = top.Value;
      }

      if (bottom.HasValue) {
        layoutGroup.padding.bottom = bottom.Value;
      }

      return layoutGroup;

    }

    public static HorizontalLayoutGroup SetSpacing(this HorizontalLayoutGroup layoutGroup, float spacing) {
      layoutGroup.spacing = spacing;
      return layoutGroup;
    }
  }

  public static class ImageExtensions {
    public static Image SetColor(this Image image, Color color) {
      image.color = color;
      return image;
    }

    public static Image SetMaskable(this Image image, bool maskable) {
      image.maskable = maskable;
      return image;
    }

    public static Image SetRaycastTarget(this Image image, bool raycastTarget) {
      image.raycastTarget = raycastTarget;
      return image;
    }
  }

  public static class LayoutElementExtensions {
    public static LayoutElement SetPreferred(
        this LayoutElement layoutElement, float? width = null, float? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutElement.preferredWidth = width.Value;
      }

      if (height.HasValue) {
        layoutElement.preferredHeight = height.Value;
      }

      return layoutElement;
    }

    public static LayoutElement SetFlexible(
        this LayoutElement layoutElement, float? width = null, float? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutElement.flexibleWidth = width.Value;
      }

      if (height.HasValue) {
        layoutElement.flexibleHeight = height.Value;
      }

      return layoutElement;
    }
  }

  public static class OutlineExtensions {
    public static Outline SetEffectColor(this Outline outline, Color effectColor) {
      outline.effectColor = effectColor;
      return outline;
    }

    public static Outline SetEffectDistance(this Outline outline, Vector2 effectDistance) {
      outline.effectDistance = effectDistance;
      return outline;
    }
  }

  public static class RectTransformExtensions {
    public static RectTransform SetAnchorMin(this RectTransform rectTransform, Vector2 anchorMin) {
      rectTransform.anchorMin = anchorMin;
      return rectTransform;
    }

    public static RectTransform SetAnchorMax(this RectTransform rectTransform, Vector2 anchorMax) {
      rectTransform.anchorMax = anchorMax;
      return rectTransform;
    }

    public static RectTransform SetPivot(this RectTransform rectTransform, Vector2 pivot) {
      rectTransform.pivot = pivot;
      return rectTransform;
    }

    public static RectTransform SetPosition(this RectTransform rectTransform, Vector2 position) {
      rectTransform.anchoredPosition = position;
      return rectTransform;
    }
  }

  public static class ScrollRectExtensions {
    public static ScrollRect SetScrollSensitivity(this ScrollRect scrollRect, float sensitivity) {
      scrollRect.scrollSensitivity = sensitivity;
      return scrollRect;
    }
  }

  public static class TextExtensions {
    public static Text SetAlignment(this Text text, TextAnchor alignment) {
      text.alignment = alignment;
      return text;
    }

    public static Text SetFont(this Text text, Font font) {
      text.font = font;
      return text;
    }

    public static Text SetFontSize(this Text text, int fontSize) {
      text.fontSize = fontSize;
      return text;
    }

    public static Text SetSupportRichText(this Text text, bool supportRichText) {
      text.supportRichText = supportRichText;
      return text;
    }

    public static Text SetText(this Text text, string value) {
      text.text = value;
      return text;
    }
  }

  public static class VerticalLayoutGroupExtensions {
    public static VerticalLayoutGroup SetChildControl(
        this VerticalLayoutGroup layoutGroup, bool? width = null, bool? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutGroup.childControlWidth = width.Value;
      }

      if (height.HasValue) {
        layoutGroup.childControlHeight = height.Value;
      }

      return layoutGroup;
    }

    public static VerticalLayoutGroup SetChildForceExpand(
        this VerticalLayoutGroup layoutGroup, bool? width = null, bool? height = null) {
      if (!width.HasValue && !height.HasValue) {
        throw new ArgumentException("Value for width or height must be provided.");
      }

      if (width.HasValue) {
        layoutGroup.childForceExpandWidth = width.Value;
      }

      if (height.HasValue) {
        layoutGroup.childForceExpandHeight = height.Value;
      }

      return layoutGroup;
    }

    public static VerticalLayoutGroup SetChildAlignment(this VerticalLayoutGroup layoutGroup, TextAnchor alignment) {
      layoutGroup.childAlignment = alignment;
      return layoutGroup;
    }

    public static VerticalLayoutGroup SetPadding(
        this VerticalLayoutGroup layoutGroup,
        int? left = null,
        int? right = null,
        int? top = null,
        int? bottom = null) {
      if (!left.HasValue && !right.HasValue && !top.HasValue && !bottom.HasValue) {
        throw new ArgumentException("Value for left, right, top or bottom must be provided.");
      }

      if (left.HasValue) {
        layoutGroup.padding.left = left.Value;
      }

      if (right.HasValue) {
        layoutGroup.padding.right = right.Value;
      }

      if (top.HasValue) {
        layoutGroup.padding.top = top.Value;
      }

      if (bottom.HasValue) {
        layoutGroup.padding.bottom = bottom.Value;
      }

      return layoutGroup;
    }

    public static VerticalLayoutGroup SetSpacing(this VerticalLayoutGroup layoutGroup, float spacing) {
      layoutGroup.spacing = spacing;
      return layoutGroup;
    }
  }
}