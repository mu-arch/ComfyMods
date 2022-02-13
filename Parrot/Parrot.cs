﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using BetterZeeRouter;

using HarmonyLib;

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Parrot {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  [BepInDependency(BetterZeeRouter.BetterZeeRouter.PluginGuid, BepInDependency.DependencyFlags.HardDependency)]
  public class Parrot : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.parrot";
    public const string PluginName = "Parrot";
    public const string PluginVersion = "1.2.0";

    static ManualLogSource _logger;

    static ConfigEntry<string> _chatMessageLogFilename;
    static StreamWriter _chatMessageLogWriter;

    static ConfigEntry<string> _chatMessageLogDiscordUrl;
    static DiscordWebhookClient _chatMessageLogDiscordClient;

    static ConfigEntry<string> _chatMessageShoutDiscordUrl;
    static DiscordWebhookClient _chatMessageShoutDiscordClient;

    static readonly int _sayHashCode = "Say".GetStableHashCode();
    static readonly int _chatMessageHashCode = "ChatMessage".GetStableHashCode();
    static readonly Regex _htmlTagsRegex = new("<.*?>");

    static readonly SayHandler _sayHandler = new();
    static readonly ChatMessageHandler _chatMessageHandler = new();

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

      RoutedRpcManager.Instance.AddHandler(_sayHashCode, _sayHandler);
      RoutedRpcManager.Instance.AddHandler(_chatMessageHashCode, _chatMessageHandler);
    }

    public void OnDestroy() {
      _chatMessageLogWriter?.Dispose();

      _chatMessageLogDiscordClient?.Stop();
      _chatMessageShoutDiscordClient?.Stop();

      _harmony?.UnpatchSelf();
    }

    class SayHandler : RpcMethodHandler {
      public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
        routedRpcData.m_parameters.SetPos(0);

        Talker.Type messageType = (Talker.Type) routedRpcData.m_parameters.ReadInt();
        string playerName = _htmlTagsRegex.Replace(routedRpcData.m_parameters.ReadString(), string.Empty);
        string messageText = routedRpcData.m_parameters.ReadString();

        routedRpcData.m_parameters.SetPos(0);
        ProcessChatMessage(routedRpcData.m_senderPeerID, playerName, messageType, messageText, Vector3.zero);

        return true;
      }
    }

    class ChatMessageHandler : RpcMethodHandler {
      public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
        routedRpcData.m_parameters.SetPos(0);

        Vector3 targetPosition = routedRpcData.m_parameters.ReadVector3();
        Talker.Type messageType = (Talker.Type) routedRpcData.m_parameters.ReadInt();
        string playerName = _htmlTagsRegex.Replace(routedRpcData.m_parameters.ReadString(), string.Empty);
        string messageText = routedRpcData.m_parameters.ReadString();

        routedRpcData.m_parameters.SetPos(0);
        ProcessChatMessage(routedRpcData.m_senderPeerID, playerName, messageType, messageText, targetPosition);

        return true;
      }
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