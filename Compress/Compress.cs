using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Reflection.Emit;

namespace Compress {
  [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
  public class Compress : BaseUnityPlugin {
    public const string PluginGUID = "redseiko.valheim.compress";
    public const string PluginName = "Compress";
    public const string PluginVersion = "1.0.0";

    static ManualLogSource _logger;
    Harmony _harmony;

    public void Awake() {
      _logger = Logger;
      _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
    }

    public void OnDestroy() {
      _harmony?.UnpatchSelf();
    }

    class CompressConfig {
      public bool CompressZdoData { get; set; }
      public long UncompressedBytesSent { get; set; }
      public long CompressedBytesSent { get; set; }
      public long UncompressedBytesRecv { get; set; }
      public long CompressedBytesRecv { get; set; }
    }

    static readonly ConcurrentDictionary<ZRpc, CompressConfig> _rpcCompressConfigCache = new();

    static void RPC_CompressHandshake(ZRpc rpc, bool isEnabled) {
      LogInfo($"Received CompressHandshake from: {rpc.m_socket.GetHostName()}, isEnabled: {isEnabled}");

      CompressConfig config = _rpcCompressConfigCache.GetOrAdd(rpc, key => new());
      config.CompressZdoData = isEnabled;

      if (ZNet.m_isServer) {
        rpc.Invoke("CompressHandshake", true);
      }
    }

    [HarmonyPatch(typeof(ZDOMan))]
    class ZDOManPatch {
      [HarmonyPostfix]
      [HarmonyPatch(nameof(ZDOMan.AddPeer))]
      static void AddPeerPostfix(ref ZDOMan __instance, ref ZNetPeer netPeer) {
        netPeer.m_rpc.Register("CompressedZDOData", new Action<ZRpc, ZPackage>(RPC_CompressedZDOData));
        netPeer.m_rpc.Register("CompressHandshake", new Action<ZRpc, bool>(RPC_CompressHandshake));

        if (ZNet.m_isServer) {
          return;
        }

        LogInfo($"Sending CompressHandshake to server...");
        netPeer.m_rpc.Invoke("CompressHandshake", true);
      }

      [HarmonyTranspiler]
      [HarmonyPatch(nameof(ZDOMan.SendZDOs))]
      static IEnumerable<CodeInstruction> SendZDOsTranspiler(IEnumerable<CodeInstruction> instructions) {
        return new CodeMatcher(instructions)
          .MatchForward(useEnd: false, new CodeMatch(OpCodes.Callvirt, typeof(ZRpc).GetMethod(nameof(ZRpc.Invoke))))
          .SetAndAdvance(
              OpCodes.Call,
              Transpilers.EmitDelegate<Action<ZRpc, string, object[]>>(SendZDOsInvokeDelegate).operand)
          .InstructionEnumeration();
      }
    }

    static readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    static readonly MemoryStream _compressStream = new();
    static readonly MemoryStream _decompressStream = new();

    static void SendZDOsInvokeDelegate(ZRpc rpc, string method, params object[] parameters) {
      if (_rpcCompressConfigCache.TryGetValue(rpc, out CompressConfig config) && config.CompressZdoData) {
        ZPackage package = (ZPackage) parameters[0];
        package.m_writer.Flush();

        int uncompressedLength = (int) package.m_stream.Length;
        config.UncompressedBytesSent += uncompressedLength;

        _compressStream.SetLength(0);

        using (GZipStream gzipStream = new(_compressStream, CompressionLevel.Fastest, leaveOpen: true)) {
          gzipStream.Write(package.m_stream.GetBuffer(), 0, uncompressedLength);
        }

        int compressedLength = (int) _compressStream.Length;
        config.CompressedBytesSent += compressedLength;

        package.Clear();
        package.m_writer.Write(compressedLength);
        package.m_writer.Write(_compressStream.GetBuffer(), 0, compressedLength);

        _compressStream.SetLength(0);
        LogCompressStats(config);

        rpc.Invoke("CompressedZDOData", package);
      } else {
        rpc.Invoke(method, parameters);
      }
    }

    static void RPC_CompressedZDOData(ZRpc rpc, ZPackage package) {
      _rpcCompressConfigCache.TryGetValue(rpc, out CompressConfig config);

      int compressedLength = package.m_reader.ReadInt32();
      config.CompressedBytesRecv += compressedLength;

      _decompressStream.SetLength(0);

      using (GZipStream gzipStream = new(package.m_stream, CompressionMode.Decompress, leaveOpen: true)) {
        gzipStream.CopyTo(_decompressStream);
      }

      int uncompressedLength = (int) _decompressStream.Length;
      config.UncompressedBytesRecv += uncompressedLength;

      package.Clear();
      package.m_stream.Write(_decompressStream.GetBuffer(), 0, uncompressedLength);
      package.m_stream.Position = 0;

      _decompressStream.SetLength(0);
      LogCompressStats(config);

      ZDOMan.m_instance.RPC_ZDOData(rpc, package);
    }

    static void LogCompressStats(CompressConfig config) {
      if (_stopwatch.ElapsedMilliseconds < 10000) {
        return;
      }

      LogInfo(
          string.Format(
              "Compress ...\n  Sent ... C/U: {0:N} KB / {1:N} KB ({2:P})\n  Recv ... C/U: {3:N} KB / {4:N} KB ({5:P})",
              config.CompressedBytesSent / 1024d,
              config.UncompressedBytesSent / 1024d,
              (double) config.CompressedBytesSent / config.UncompressedBytesSent,
              config.CompressedBytesRecv / 1024d,
              config.UncompressedBytesRecv / 1024d,
              (double) config.CompressedBytesRecv / config.UncompressedBytesRecv));

      _stopwatch.Restart();
    }

    static void LogInfo(string message) {
      _logger.LogInfo($"[{DateTime.Now.ToString(DateTimeFormatInfo.InvariantInfo)}] {message}");
    }
  }
}