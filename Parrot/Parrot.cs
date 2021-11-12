using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

using UnityEngine;

namespace Parrot {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Parrot : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.parrot";
    public const string PluginName = "Parrot";
    public const string PluginVersion = "1.0.0";

    static ManualLogSource _logger;

    static ConfigEntry<string> _chatMessageLogFilename;
    static StreamWriter _chatMessageLogWriter;

    static ConfigEntry<string> _chatMessageDiscordEndPoint;
    static WebClient _chatMessageDiscordWebClient;
    static Uri _discordEndPointUri;

    static readonly ConcurrentQueue<NameValueCollection> _chatMessageUploadQueue = new();

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

      _chatMessageDiscordEndPoint =
          Config.Bind(
              "ChatMessage",
              "ChatMessageDiscordEndPoint",
              string.Empty,
              "If set, will send ChatMessages to the target Discord WebHook end-point.");

      if (!_chatMessageDiscordEndPoint.Value.IsNullOrWhiteSpace()) {
        _discordEndPointUri = new(_chatMessageDiscordEndPoint.Value);
        LogInfo($"Sending ChatMessages to Discord WebHook at: {_discordEndPointUri}");

        _chatMessageDiscordWebClient = new();
        StartCoroutine(SendChatMessagesToDiscordCoroutine());
      }

      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _chatMessageLogWriter?.Dispose();
      _chatMessageDiscordWebClient?.Dispose();
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
      string logText;

      switch (messageType) {
        case Talker.Type.Normal:
        case Talker.Type.Shout:
        case Talker.Type.Whisper:
          logText = $"{senderId} | {playerName} ({messageType}): {messageText}";
          break;

        case Talker.Type.Ping:
          logText = $"{senderId} | {playerName} ({messageType}): {targetPosition}";
          break;

        default:
          LogError($"Unexpected Talker.Type: {messageType}");
          return;
      }

      LogInfo(logText);

      if (_chatMessageLogWriter != null) {
        _chatMessageLogWriter.WriteLine($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {logText}");
        _chatMessageLogWriter.Flush();
      }

      if (_chatMessageDiscordWebClient != null) {
        SendChatMessageToDiscord(senderId, playerName, messageType, messageText, targetPosition);
      }
    }

    static void SendChatMessageToDiscord(
        long senderId, string playerName, Talker.Type messageType, string messageText, Vector3 targetPosition) {
      NameValueCollection values = new();
      values["username"] = $"{playerName} ({senderId})";

      switch (messageType) {
        case Talker.Type.Normal:
          values["content"] = $":speech_balloon:  {messageText}";
          break;

        case Talker.Type.Shout:
          values["content"] = $":loudspeaker: {messageText}";
          break;

        case Talker.Type.Whisper:
          values["content"] = $":eye_in_speech_bubble: {messageText}";
          break;

        case Talker.Type.Ping:
          values["content"] = $":dart: {targetPosition}";
          break;

        default:
          LogError($"Unexpected Talker.Type: {messageType}");
          return;
      }

      _chatMessageUploadQueue.Enqueue(values);
    }

    static IEnumerator SendChatMessagesToDiscordCoroutine() {
      WaitForSeconds waitInterval = new(seconds: 0.25f);

      while (true) {
        yield return waitInterval;

        if (!_chatMessageUploadQueue.TryDequeue(out NameValueCollection values)) {
          continue;
        }

        try {
          _chatMessageDiscordWebClient.UploadValues(_discordEndPointUri, values);
        } catch (WebException exception) {
          LogError($"Failed to upload ChatMessage to Discord: {exception}");
        }
      }
    }

    static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }

    static void LogError(string message) {
      _logger.LogError($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}