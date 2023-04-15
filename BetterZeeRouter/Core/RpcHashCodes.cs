namespace BetterZeeRouter {
  public static class RpcHashCodes {
    public static readonly int RpcWntHealthChangedHashCode = "WNTHealthChanged".GetStableHashCode();
    public static readonly int RpcDamageTextHashCode = "DamageText".GetStableHashCode();

    // Yes, these ones are actually prefixed `RPC_` in vanilla code.
    public static readonly int RpcTeleportToHashCode = "RPC_TeleportTo".GetStableHashCode();
    public static readonly int RpcSetTargetHashCode = "RPC_SetTarget".GetStableHashCode();
  }
}
