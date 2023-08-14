using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{

    private Vector2Int[] directions = new Vector2Int[] 
    {
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    /// <summary>
    /// Generates and updates the availableMoves list
    /// </summary>
    /// <returns>availableMoves</returns>
    public override List<Vector2Int> SelectAvailableSquares()
    {
       availableMoves.Clear();
       float range = board.BOARD_SIZE;
       foreach (var direction in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int nextSquare = occupiedSquare + direction * i;
                Piece piece = board.GetPieceOnSquare(nextSquare);
                if(!board.CoordinatesOnBoard(nextSquare))
                {
                    break;
                }
                if (piece == null)
                {
                    TryToAddMove(nextSquare);
                }
                else if (!piece.IsSameColour(this))
                {
                    TryToAddMove(nextSquare);
                    break;
                }
                else if (piece.IsSameColour(this))
                {
                    break;
                }
            }
        }
        return availableMoves;
    }
}
