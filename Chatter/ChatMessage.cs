using System;

using UnityEngine;

namespace Chatter {
  public enum ChatMessageType {
    Text,
    HudCenter,
    Say,
    Shout,
    Whisper,
    Ping
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