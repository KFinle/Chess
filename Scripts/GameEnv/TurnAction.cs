using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnAction 
{
     public Player activePlayer;
    public Piece _pieceMoved;
    public Vector2Int _pieceStartLocation;
    public Vector2Int _pieceEndLocation;
    public int _turnNumber;
    public static float globalTurnNum = 1.0f;
    public TurnAction(Piece piece)
    {
        _pieceMoved = piece;
        _pieceStartLocation = piece.previousLocation;
        _pieceEndLocation = piece.occupiedSquare;
        _turnNumber = Mathf.FloorToInt(globalTurnNum);
        globalTurnNum += 0.5f;
    }

    /// <summary>
    /// Displays the most recent turn action
    /// </summary>
    /// <returns></returns>
    public string PrintTurnAction()
    {
        string colour = _pieceMoved.colour == Colour.WHITE ? "White" : "Black";
        return "Turn #" + _turnNumber + " " + colour + " move: " + _pieceMoved.GetType() + " " + NotatedSquare.PrintSquare(_pieceStartLocation) +
        " → " + NotatedSquare.PrintSquare(_pieceEndLocation) + "\n";
    }

    /// <summary>
    /// Resets turn counter back to 1
    /// </summary>
    public void ResetTurnCounter()
    {
        globalTurnNum = 1.0f;
    }
}