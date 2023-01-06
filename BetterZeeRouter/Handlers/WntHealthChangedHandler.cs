namespace BetterZeeRouter {
  public sealed class WntHealthChangedHandler : RpcMethodHandler {
    public long WntHealthChangedCount { get; private set; } = 0L;

    public override bool Process(ZRoutedRpc.RoutedRPCData routedRpcData) {
      WntHealthChangedCount++;
      return false;
    }
  }
}
