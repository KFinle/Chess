using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string name;
    public int playerScore;
    public bool inCheck = false;
    public Colour colour { get; set; }
    public Board board { get; set; }
    public List<Piece> activePieces { get; private set; }

    public Player(Colour colour, Board board)
    {
        this.board = board;
        this.colour = colour;
        activePieces = new List<Piece>();
    }

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        CalculateCurrentPlayerScore();
    }

    /// <summary>
    /// Add given piece to activePieces
    /// </summary>
    /// <param name="piece"></param>
    public void AddPiece(Piece piece)
    {
        if (!activePieces.Contains(piece))
        {
            //Debug.Log("Created piece " + piece + " at activePieces index " + activePieces.IndexOf(piece));
            activePieces.Add(piece);
        }
    }

    /// <summary>
    /// Remove given piece from activePieces
    /// </summary>
    /// <param name="piece"></param>
    public void RemovePiece(Piece piece)
    {
        if (activePieces.Contains(piece))
        {
            activePieces.Remove(piece);
        }
    }

    /// <summary>
    /// Generate all possible moves
    /// </summary>
    public void GenerateAllPossibleMoves()
    {
        foreach (var piece in activePieces)
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvailableSquares();
            }
        }

    }

    /// <summary>
    /// Gets a list of all pieces attacking a piece of type T
    /// </summary>
    /// <typeparam name="T">piece type</typeparam>
    public Piece[] GetAttackingPieceOfType<T>() where T : Piece
    {
        return activePieces.Where(piece => piece.IsAttackingPieceOfType<T>()).ToArray();
    }

    /// <summary>
    /// Returns a list of pieces with type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Piece[] GetPiecesOfType<T>() where T : Piece
    {
        return activePieces.Where(piece => piece is T).ToArray();
    }

    /// <summary>
    /// Prevents all moves that would allow oppenent to attack a piece of type T
    /// 
    /// This is how we prevent illegal moves that endanger the King
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="opponent">Player opponent</param>
    /// <param name="piece">Piece to check</param>
    public void RemoveMovesAllowingAttackOnPiece<T>(Player opponent, Piece piece) where T : Piece
    {
        List<Vector2Int> coordinatesToRemove = new List<Vector2Int>();
        foreach (var coordinates in piece.availableMoves.ToList())
        {
            Piece pieceOnSquare = board.GetPieceOnSquare(coordinates);
            board.UpdateBoardOnPieceMoved(coordinates, piece.occupiedSquare, piece, null);
            opponent.GenerateAllPossibleMoves();
            if (opponent.IsAttackingPiece<T>())
            {
                coordinatesToRemove.Add(coordinates);
            }
            board.UpdateBoardOnPieceMoved(piece.occupiedSquare, coordinates, piece, pieceOnSquare);
        }
        foreach (var coordinates in coordinatesToRemove)
        {
            piece.availableMoves.Remove(coordinates);
        }
    }

    /// <summary>
    /// Checks if the player is attacking piece of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private bool IsAttackingPiece<T>() where T : Piece
    {
        foreach (var piece in activePieces.ToList())
        {
            if (board.HasPiece(piece) && piece.IsAttackingPieceOfType<T>())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the player can protect a piece of type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="opponent"></param>
    /// <returns></returns>
    public bool CanProtectPiece<T>(Player opponent) where T : Piece
    {
        foreach (var piece in activePieces)
        {
            foreach (var coordinates in piece.availableMoves)
            {
                Piece pieceOnCoordinates = board.GetPieceOnSquare(coordinates);
                board.UpdateBoardOnPieceMoved(coordinates, piece.occupiedSquare, piece, null);
                opponent.GenerateAllPossibleMoves();
                if (!opponent.IsAttackingPiece<T>())
                {
                    board.UpdateBoardOnPieceMoved(piece.occupiedSquare, coordinates, piece, pieceOnCoordinates);
                    return true;
                }
                board.UpdateBoardOnPieceMoved(piece.occupiedSquare, coordinates, piece, pieceOnCoordinates);
            }
        }
        return false;
    }

    /// <summary>
    /// Clears active pieces when restarting game
    /// </summary>
    internal void OnGameRestart()
    {
        activePieces.Clear();
    }
    public void CalculateCurrentPlayerScore()
    {
        playerScore = 0;
        foreach(Piece piece in activePieces)
        {

            switch (piece.type)
            {
                case PieceType.Pawn:
                    playerScore += (int)PieceValue.Pawn;

                    break;
                case PieceType.Bishop:
                    playerScore += (int)PieceValue.Bishop;


                    break;
                case PieceType.Knight:
                    playerScore += (int)PieceValue.Knight;

                    break;
                case PieceType.Rook:
                    playerScore += (int)PieceValue.Rook;

                    break;
                case PieceType.Queen:
                    playerScore += (int)PieceValue.Queen;

                    break;
                case PieceType.King:
                    playerScore += (int)PieceValue.King;

                    break;
                default:
                    break;
            }
        }
    }
}
