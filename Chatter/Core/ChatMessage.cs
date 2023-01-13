using System;

using UnityEngine;

namespace Chatter {
  [Flags]
  public enum ChatMessageType {
    None = 0,
    Text = 1,
    HudCenter = 2,
    Say = 4,
    Shout = 8,
    Whisper = 16,
    Ping = 32
  }

  public class ChatMessage {
    public ChatMessageType MessageType { get; init; } = ChatMessageType.Text;
    public DateTime Timestamp { get; init; } = DateTime.MinValue;
    public long SenderId { get; init; } = 0L;
    public Vector3 Position { get; init; } = Vector3.zero;
    public Talker.Type TalkerType { get; init; } = Talker.Type.Normal;
    public string Username { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
  }
}