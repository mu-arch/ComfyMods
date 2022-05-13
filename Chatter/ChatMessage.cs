using System;

using UnityEngine;

namespace Chatter {
  public class ChatMessage {
    public DateTime Timestamp { get; set; }
    public long SenderId { get; set; }
    public Vector3 Position { get; set; }
    public Talker.Type Type { get; set; }
    public string User { get; set; }
    public string Text { get; set; }
  }
}