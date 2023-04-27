using UnityEngine;

namespace Chatter {
  public class ContentRow {
    public GameObject Row { get; init; }
    public ChatMessage ChatMessage { get; init; }
    public GameObject Divider { get; init; }

    public ContentRow(GameObject row, ChatMessage message, GameObject divider) {
      Row = row;
      ChatMessage = message;
      Divider = divider;
    }
  }
}
