using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{


    Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(2, 1),
        new Vector2Int(2, -1),
        new Vector2Int(1, 2),
        new Vector2Int(1, -2),
        new Vector2Int(-1, 2),
        new Vector2Int(-1, -2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1)
    };

    /// <summary>
    /// Generates and updates the availableMoves list
    /// </summary>
    /// <returns>availableMoves</returns>
    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int nextSquare = occupiedSquare + offsets[i];
            Piece piece = board.GetPieceOnSquare(nextSquare);

            if (!board.CoordinatesOnBoard(nextSquare))
            {
                continue;
            }
            if (piece == null || !piece.IsSameColour(this))
            {
                TryToAddMove(nextSquare);
            }
        }
        return availableMoves;

    }
}
