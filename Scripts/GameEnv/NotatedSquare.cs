using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NotatedSquare
{
    public static string PrintSquare(Vector2Int position)
    {
        return Enum.GetName(typeof(BoardLetter), (position.x + 1)) + (position.y + 1);
    }
}