using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// Creates a new Unity menu option
[CreateAssetMenu(menuName = "Scriptable Objects/Board/Layout")]
public class BoardLayout : ScriptableObject
{
    [Serializable]
    public class BoardSquareSetup
    {
        public Vector2Int position;
        public PieceType pieceType;
        public Colour colour;
    }

    [SerializeField]
    public BoardSquareSetup[] boardSquares;

    public int GetNumPieces()
    {
        return boardSquares.Length;
    }

    public Vector2Int GetSquareCoordinates(int square)
    {
        if (boardSquares.Length <= square)
        {
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(boardSquares[square].position.x - 1, boardSquares[square].position.y - 1);
    }
    public string GetPieceNameAtSquare(int square)
    {
        if (boardSquares.Length <= square)
        {
            return "";
        }
        return boardSquares[square].pieceType.ToString();
    }

    public Colour GetColourAtSquare(int square)
    {
        if (boardSquares.Length <= square)
        {
            return Colour.BLACK;
        }
        return boardSquares[square].colour;
    }
}
