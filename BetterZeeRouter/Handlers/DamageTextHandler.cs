namespace BetterZeeRouter {
  public sealed class DamageTextHandler : RpcMethodHandler {
    public long DamageTextCount { get; private set; } = 0L;

    public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
      DamageTextCount++;
      return false;
    }
  }
}
