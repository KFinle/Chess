using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public bool enPassantAllowed = false;

    public Vector2Int enpassantCoordinates;



    /// <summary>
    /// Generates and updates the availableMoves list
    /// </summary>
    /// <returns>availableMoves</returns>
    public override List<Vector2Int> SelectAvailableSquares()
    {
        availableMoves.Clear();
        Vector2Int direction = colour == Colour.WHITE ? Vector2Int.up : Vector2Int.down;
        float range = hasMoved ? 1 : 2;
        for (int i = 1; i <= range; i++)
        {
            Vector2Int nextSquare = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextSquare);
            if (!board.CoordinatesOnBoard(nextSquare))
            {
                break;
            }
            if (piece == null)
            {
                TryToAddMove(nextSquare);
            }
            else 
            {
                break;
            }
        }

        Vector2Int[] killDirections = new Vector2Int[] 
        {
            new Vector2Int(-1, direction.y),
            new Vector2Int(1, direction.y)
        };

        for (int i = 0; i < killDirections.Length; i++)
        {
            Vector2Int nextSquare = occupiedSquare + killDirections[i];
            Piece piece = board.GetPieceOnSquare(nextSquare);
            if (!board.CoordinatesOnBoard(nextSquare))
            {
                continue;
            }
            if (piece != null && !piece.IsSameColour(this))
            {
                TryToAddMove(nextSquare);
            }
        }
        return availableMoves;
    }

    /// <summary>
    /// Overrides the MovePiece() method to allow promotion
    /// </summary>
    /// <param name="coordinates"></param>
    public override void MovePiece(Vector2Int coordinates)
    {
        if (IsDoingEnPassant(coordinates))
        {
            DoEnPassant();
        }

        base.MovePiece(coordinates);
        CheckPromotion();
    }

    /// <summary>
    /// Check if this piece will be promoted
    /// </summary>
    public void CheckPromotion()
    {
        int endOfBoardCoordinate = colour == Colour.WHITE ? board.BOARD_SIZE - 1 : 0;
        if (occupiedSquare.y == endOfBoardCoordinate)
        {
            board.PromotePiece(this);
        }
    }

    private bool CanPerformEnPassant()
    {
        int enPassantRow = colour == Colour.WHITE ? 5 : 4;
        Piece lastPieceMoved = board.lastPieceMoved;

        int takeDirectionY = colour == Colour.WHITE ? 1 : -1;

        if (lastPieceMoved != null && lastPieceMoved.type == PieceType.Pawn)
        {
            int lastMoveDistance = Mathf.Abs(lastPieceMoved.occupiedSquare.y - lastPieceMoved.previousLocation.y);
            if (lastMoveDistance == 2)
            {
                if (lastPieceMoved.occupiedSquare.y == this.occupiedSquare.y)
                {
                    if (lastPieceMoved.occupiedSquare.x == this.occupiedSquare.x + 1)
                    {
                        enpassantCoordinates = new Vector2Int(occupiedSquare.x + 1, occupiedSquare.y + takeDirectionY);
                        TryToAddMove(enpassantCoordinates);
                        return true;
                    }

                    if (lastPieceMoved.occupiedSquare.x == this.occupiedSquare.x - 1)
                    {
                        enpassantCoordinates = new Vector2Int(occupiedSquare.x - 1, occupiedSquare.y + takeDirectionY);
                        TryToAddMove(enpassantCoordinates);
                        return true;
                    }
                }
            }
        }
        enpassantCoordinates = new Vector2Int(-1,-1);
        return false;
    }

    public Piece GetEnPassantVictim()
    {
        if (enpassantCoordinates.x < this.occupiedSquare.x)
        {
            Piece pieceToTake =  board.GetPieceOnSquare( new Vector2Int(occupiedSquare.x - 1, occupiedSquare.y));
            return pieceToTake;
        }
        if (enpassantCoordinates.x > this.occupiedSquare.x)
        {
            Piece pieceToTake =  board.GetPieceOnSquare( new Vector2Int(occupiedSquare.x + 1, occupiedSquare.y));
            return pieceToTake;
        }
        return null;
    }

    public void DoEnPassant()
    {
        Piece piece = GetEnPassantVictim();
        board.TakePiece(piece);
    }

    public bool IsDoingEnPassant(Vector2Int coordinates)
    {
        if (coordinates == enpassantCoordinates) return true;
        return false;
    }
}




