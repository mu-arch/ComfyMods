using BepInEx;

using HarmonyLib;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using UnityEngine;

namespace CriticalDice {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class CriticalDice : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.criticaldice";
    public const string PluginName = "CriticalDice";
    public const string PluginVersion = "1.1.1";

    static readonly int _rpcRoutedRpcHashCode = "RoutedRPC".GetStableHashCode();
    static readonly int _rpcSayHashCode = "Say".GetStableHashCode();
    static readonly Regex _htmlTagsRegex = new("<.*?>");
    static readonly System.Random _random = new();

    Harmony _harmony;

    public void Awake() {
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
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
      if (routedRpcData.m_methodHash == _rpcSayHashCode) {
        ZNet.m_instance.StartCoroutine(ParseRpcSayDataCoroutine(routedRpcData));
      }
    }

    static readonly int _intSize = sizeof(int);
    static readonly WaitForSeconds _waitInterval = new(seconds: 0.5f);

    static readonly ZPackage _package = new();
    static readonly ZRoutedRpc.RoutedRPCData _routedRpcData = new();

    static IEnumerator ParseRpcSayDataCoroutine(ZRoutedRpc.RoutedRPCData routedRpcData) {
      yield return _waitInterval;

      routedRpcData.m_parameters.SetPos(0);
      routedRpcData.m_parameters.ReadInt();
      string playerName = routedRpcData.m_parameters.ReadString();
      string messageText = routedRpcData.m_parameters.ReadString();
      routedRpcData.m_parameters.SetPos(0);

      if (!messageText.StartsWith("!roll")) {
        yield break;
      }

      long result = 0L;
      Task<bool> task = Task.Run(() => ParseDiceRoll(messageText, out result));

      while (!task.IsCompleted) {
        yield return null;
      }

      if (task.IsFaulted || !task.Result) {
        yield break;
      }

      SendDiceRollResponse(
          ZRoutedRpc.m_instance, routedRpcData, _htmlTagsRegex.Replace(playerName, string.Empty), result);
    }

    static void SendDiceRollResponse(
        ZRoutedRpc routedRpc, ZRoutedRpc.RoutedRPCData routedRpcData, string playerName, long result) {
      routedRpc.m_rpcMsgID++;

      _routedRpcData.m_msgID = routedRpc.m_id + routedRpc.m_rpcMsgID;
      _routedRpcData.m_senderPeerID = routedRpc.m_id;
      _routedRpcData.m_targetPeerID = ZRoutedRpc.Everybody;
      _routedRpcData.m_targetZDO = routedRpcData.m_targetZDO;
      _routedRpcData.m_methodHash = _rpcSayHashCode;

      _routedRpcData.m_parameters.Clear();
      _routedRpcData.m_parameters.Write((int) Talker.Type.Normal);
      _routedRpcData.m_parameters.Write("<color=#AEC6D3><b>Server</b></color>");
      _routedRpcData.m_parameters.Write($"{playerName} rolled... {result}");

      _package.Clear();
      _package.Write(_rpcRoutedRpcHashCode);
      _package.Write(0);

      _routedRpcData.Serialize(_package);

      int size = _package.Size() - _intSize - _intSize;
      _package.m_writer.Seek(_intSize, SeekOrigin.Begin);
      _package.Write(size);
      _package.m_writer.Flush();

      byte[] packageData = _package.GetArray();

      foreach (ZNetPeer netPeer in routedRpc.m_peers) {
        if (netPeer.IsReady() && netPeer.m_rpc.IsConnected()) {
          netPeer.m_rpc.m_sentPackages++;
          netPeer.m_rpc.m_sentData += packageData.Length;

          ((ZSteamSocket) netPeer.m_rpc.m_socket).m_sendQueue.Enqueue(packageData);
        }
      }
    }

    static readonly Regex _diceRollRegex =
        new(@"^!roll\s+(?:(?<simple>\d+)(?:\s+.*)?$|(?<count>\d*)d(?<faces>\d+)\s*(?<modifier>[\+-]\d+)?(?:\s+.*)?$)");

    static bool ParseDiceRoll(string input, out long result) {
      result = 0;

      MatchCollection matches = _diceRollRegex.Matches(input);

      if (matches.Count <= 0) {
        return false;
      }

      Match match = matches[0];

      if (match.Groups["simple"].Length > 0) {
        if (!int.TryParse(match.Groups["simple"].Value, out int simple) || simple < 2) {
          return false;
        }

        result += _random.Next(simple) + 1;
        return true;
      }

      if (!int.TryParse(match.Groups["count"].Value, out int diceCount)) {
        diceCount = 1;
      }

      if (!int.TryParse(match.Groups["faces"].Value, out int diceFaces) || diceFaces < 2) {
        return false;
      }

      if (int.TryParse(match.Groups["modifier"].Value, out int modifier)) {
        result += modifier;
      }

      diceCount = Math.Min(diceCount, 20);
      diceFaces = Math.Min(diceFaces, 1000);

      for (int i = 0; i < diceCount; i++) {
        result += _random.Next(diceFaces) + 1;
      }

      return true;
    }
  }
}