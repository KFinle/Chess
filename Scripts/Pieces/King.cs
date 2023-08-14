using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
 private Vector2Int[] directions = new Vector2Int[] 
    {
      new Vector2Int(0, 1),
      new Vector2Int(0, -1),
      new Vector2Int(1, 0),
      new Vector2Int(1, 1),
      new Vector2Int(1, -1),
      new Vector2Int(-1, 0),
      new Vector2Int(-1, 1),
      new Vector2Int(-1, -1)
    };

    //Castling
    private Vector2Int? leftCastlingMove;
    private Vector2Int? rightCastlingMove;
    private Piece leftRook;
    private Piece rightRook;

    /// <summary>
    /// Generates and updates the availableMoves list
    /// </summary>
    /// <returns>availableMoves</returns>
    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        StandardKingMoves();
        CastlingMoves();
        return availableMoves;
    }

    /// <summary>
    /// Defines the normal moves a King can make
    /// </summary>
    private void StandardKingMoves()
    {
    float range = 1; //board.BOARD_SIZE;
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
    }

    /// <summary>
    /// Defines Castling moves a King can make
    /// </summary>
    private void CastlingMoves()
    {
        if(hasMoved)
        {
            leftCastlingMove = null;
            rightCastlingMove = null;
            return;
        }
        leftRook = GetPieceInDirection<Rook>(colour, Vector2Int.left);
        if(leftRook && !leftRook.hasMoved)
        {
            leftCastlingMove = occupiedSquare + Vector2Int.left * 2;
            availableMoves.Add((Vector2Int)leftCastlingMove);
        }

        rightRook = GetPieceInDirection<Rook>(colour, Vector2Int.right);
        if(rightRook && !rightRook.hasMoved)
        {
            rightCastlingMove = occupiedSquare + Vector2Int.right * 2;
            availableMoves.Add((Vector2Int)rightCastlingMove);
        }
    } 
    /// <summary>
    /// Overrides the MovePiece() method to include Castling
    /// </summary>
    /// <param name="coordinates"></param>
    public override void MovePiece(Vector2Int coordinates)
    {
        base.MovePiece(coordinates);
        if(coordinates == leftCastlingMove)
        {
            board.UpdateBoardOnPieceMoved(coordinates + Vector2Int.right, leftRook.occupiedSquare, leftRook, null);
            leftRook.MovePiece(coordinates + Vector2Int.right);
        }

        else if(coordinates == rightCastlingMove)
        {
            board.UpdateBoardOnPieceMoved(coordinates + Vector2Int.left, rightRook.occupiedSquare, rightRook, null);
            rightRook.MovePiece(coordinates + Vector2Int.left);
        }
    }

    
}
