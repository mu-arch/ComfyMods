using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Parrot {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Parrot : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.parrot";
    public const string PluginName = "Parrot";
    public const string PluginVersion = "1.1.1";

    static ManualLogSource _logger;

    static ConfigEntry<string> _chatMessageLogFilename;
    static StreamWriter _chatMessageLogWriter;

    static ConfigEntry<string> _chatMessageLogDiscordUrl;
    static DiscordUploadClient _chatMessageLogDiscordClient;

    static ConfigEntry<string> _chatMessageShoutDiscordUrl;
    static DiscordUploadClient _chatMessageShoutDiscordClient;

    static readonly int _sayHashCode = "Say".GetStableHashCode();
    static readonly int _chatMessageHashCode = "ChatMessage".GetStableHashCode();
    static readonly Regex _htmlTagsRegex = new("<.*?>");

    Harmony _harmony;

    public void Awake() {
      _logger = Logger;

      _chatMessageLogFilename =
          Config.Bind(
              "ChatMessage",
              "ChatMessageLogFilename",
              "ChatMessageLog.txt",
              "If set, will log all ChatMessages to a textfile with this filename.");

      if (!_chatMessageLogFilename.Value.IsNullOrWhiteSpace()) {
        string logFilename = Path.Combine(Utils.GetSaveDataPath(), _chatMessageLogFilename.Value);
        LogInfo($"Logging ChatMessages to: {logFilename}");

        _chatMessageLogWriter = File.AppendText(logFilename);
      }

      _chatMessageLogDiscordUrl =
          Config.Bind(
              "ChatMessage",
              "ChatMessageLogDiscordUrl",
              string.Empty,
              "If set, will log all ChatMessages to the Discord Webhook at the specified url.");

      if (!_chatMessageLogDiscordUrl.Value.IsNullOrWhiteSpace()) {
        LogInfo($"Logging all ChatMessages to Discord Webhook at: {_chatMessageLogDiscordUrl.Value}");
        _chatMessageLogDiscordClient = new(_chatMessageLogDiscordUrl.Value);
        _chatMessageLogDiscordClient.Start();
      }

      _chatMessageShoutDiscordUrl =
          Config.Bind(
              "ChatMessage",
              "ChatMessageShoutDiscordUrl",
              string.Empty,
              "If set, will log only Shout-type ChatMessages to the Discord Webhook at the specified url.");

      if (!_chatMessageShoutDiscordUrl.Value.IsNullOrWhiteSpace()) {
        LogInfo($"Logging Shout-type ChatMessages to Discord Webhook at: {_chatMessageShoutDiscordUrl.Value}");
        _chatMessageShoutDiscordClient = new(_chatMessageShoutDiscordUrl.Value);
        _chatMessageShoutDiscordClient.Start();
      }

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _chatMessageLogWriter?.Dispose();

      _chatMessageLogDiscordClient?.Stop();
      _chatMessageShoutDiscordClient?.Stop();

      _harmony?.UnpatchSelf();
    }

    [HarmonyPatch(typeof(ZRoutedRpc))]
    class ZRoutedRpcPatch {
      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZRoutedRpc.RPC_RoutedRPC))]
      static IEnumerable<CodeInstruction> RPC_RoutedRPCTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
            .MatchForward(
                useEnd: false,
                new CodeMatch(
                    OpCodes.Callvirt,
                    AccessTools.Method(
                        typeof(ZRoutedRpc.RoutedRPCData), nameof(ZRoutedRpc.RoutedRPCData.Deserialize))))
            .Advance(offset: 1)
            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
            .InsertAndAdvance(Transpilers.EmitDelegate<Action<ZRoutedRpc.RoutedRPCData>>(ProcessRoutedRpcData))
            .InstructionEnumeration();
      }
    }

    static void ProcessRoutedRpcData(ZRoutedRpc.RoutedRPCData routedRpcData) {
      if (routedRpcData.m_methodHash == _sayHashCode) {
        ProcessRpcSayData(routedRpcData);
      } else if (routedRpcData.m_methodHash == _chatMessageHashCode) {
        ProcessRpcChatMessageData(routedRpcData);
      }
    }

    static void ProcessRpcSayData(ZRoutedRpc.RoutedRPCData routedRpcData) {
      Talker.Type messageType = (Talker.Type) routedRpcData.m_parameters.ReadInt();
      string playerName = _htmlTagsRegex.Replace(routedRpcData.m_parameters.ReadString(), string.Empty);
      string messageText = routedRpcData.m_parameters.ReadString();
      routedRpcData.m_parameters.SetPos(0);

      ProcessChatMessage(routedRpcData.m_senderPeerID, playerName, messageType, messageText, Vector3.zero);
    }

    static void ProcessRpcChatMessageData(ZRoutedRpc.RoutedRPCData routedRpcData) {
      Vector3 targetPosition = routedRpcData.m_parameters.ReadVector3();
      Talker.Type messageType = (Talker.Type) routedRpcData.m_parameters.ReadInt();
      string playerName = _htmlTagsRegex.Replace(routedRpcData.m_parameters.ReadString(), string.Empty);
      string messageText = routedRpcData.m_parameters.ReadString();
      routedRpcData.m_parameters.SetPos(0);

      ProcessChatMessage(routedRpcData.m_senderPeerID, playerName, messageType, messageText, targetPosition);
    }

    static void ProcessChatMessage(
        long senderId, string playerName, Talker.Type messageType, string messageText, Vector3 targetPosition) {
      string logText =
          messageType switch {
            Talker.Type.Ping => $"{playerName} ({senderId}) {messageType}: {targetPosition}",
            _ => $"{playerName} ({senderId}) {messageType}: {messageText}",
          };

      LogInfo(logText);

      if (_chatMessageLogWriter != null) {
        _chatMessageLogWriter.WriteLine($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {logText}");
        _chatMessageLogWriter.Flush();
      }

      if (_chatMessageLogDiscordClient != null) {
        SendChatMessageToDiscord(senderId, playerName, messageType, messageText, targetPosition);
      }

      if (messageType == Talker.Type.Shout && _chatMessageShoutDiscordClient != null) {
        SendShoutChatMessageToDiscord(senderId, playerName, messageText);
      }
    }

    static void SendChatMessageToDiscord(
        long senderId, string playerName, Talker.Type messageType, string messageText, Vector3 targetPosition) {
      string contentText =
          messageType switch {
            Talker.Type.Normal => $":speech_balloon:  {messageText}",
            Talker.Type.Shout => $":loudspeaker:  {messageText}",
            Talker.Type.Whisper => $":eye_in_speech_bubble:  {messageText}",
            Talker.Type.Ping => $":dart:  {targetPosition}",
            _ => $":question:  {messageText}",
          };

      _chatMessageLogDiscordClient.Upload(
          new NameValueCollection() {
            { "username", $"{playerName} ({senderId})" },
            { "content", contentText },
          });
    }

    static void SendShoutChatMessageToDiscord(long senderId, string playerName, string messageText) {
      _chatMessageShoutDiscordClient.Upload(
          new NameValueCollection() {
            { "username", $"{playerName} ({senderId})" },
            { "content", messageText },
          });
    }

    static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }

    static void LogError(string message) {
      _logger.LogError($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}