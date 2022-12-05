using System;
using System.IO;

namespace BetterZeeRouter {
  public sealed class TeleportToHandler : RpcMethodHandler, IDisposable {
    readonly StreamWriter _teleportToWriter;

    public TeleportToHandler(string teleportToLogFilename) {
      _teleportToWriter =
          File.AppendText(Path.Combine(Utils.GetSaveDataPath(FileHelpers.FileSource.Local), teleportToLogFilename));
    }

    public void Dispose() {
      _teleportToWriter.Dispose();
    }

    public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
      long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      long senderId = routedRpcData.m_senderPeerID;
      long targetId = routedRpcData.m_targetPeerID;

      _teleportToWriter.WriteLine($"{timestamp},{senderId},{targetId}");
      _teleportToWriter.Flush();

      ZLog.Log($"[{DateTimeOffset.Now}] RPC_TeleportTo attempted by {senderId} targeting {targetId}.");

      return false;
    }
  }
}
