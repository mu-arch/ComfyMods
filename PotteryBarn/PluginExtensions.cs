using System;
using System.Linq;

namespace PotteryBarn {
  public static class PieceExtensions {
    public static Piece SetName(this Piece piece, string name) {
      piece.m_name = name;
      return piece;
    }

    public static Piece SetResources(this Piece piece, params Piece.Requirement[] requirements) {
      piece.m_resources = requirements;
      return piece;
    }

    public static Piece SetResource(this Piece piece, string resourceName, Action<Piece.Requirement> resourceFunc) {
      resourceFunc?.Invoke(piece.GetResource(resourceName));
      return piece;
    }

    public static Piece.Requirement GetResource(this Piece piece, string resourceName) {
      return piece.Ref()?.m_resources?.Where(req => req?.m_resItem.Ref()?.name == resourceName).FirstOrDefault();
    }
  }

  public static class PieceRequirementExtensions {
    public static Piece.Requirement SetAmount(this Piece.Requirement requirement, int amount) {
      requirement.m_amount = amount;
      return requirement;
    }

    public static Piece.Requirement SetRecover(this Piece.Requirement requirement, bool recover) {
      requirement.m_recover = recover;
      return requirement;
    }
  }

  public static class PieceTableExtensions {
    public static bool AddPiece(this PieceTable pieceTable, Piece piece) {
      if (!piece || !pieceTable || pieceTable.m_pieces == null || pieceTable.m_pieces.Contains(piece.gameObject)) {
        return false;
      }

      pieceTable.m_pieces.Add(piece.gameObject);
      ZLog.Log($"Added Piece {piece.m_name} to PieceTable {pieceTable.name}");

      return true;
    }
  }

  public static class PluginExtensions {
    public static T Ref<T>(this T o) where T : UnityEngine.Object {
      return o ? o : null;
    }
  }
}
