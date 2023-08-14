using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceCreator : MonoBehaviour
{
    [SerializeField] private GameObject[] piecePrefabs;
    [SerializeField] private Material blackMat;
    [SerializeField] private Material whiteMat;
    [SerializeField] private Material selectedMat;

    private Dictionary<string, GameObject> nameToPieceDict = new Dictionary<string, GameObject>();

    /// <summary>
    /// Called when the PieceCreator becomes active
    /// </summary>
    private void Awake() 
    {
        foreach (var piece in piecePrefabs)
        {
            nameToPieceDict.Add(piece.GetComponent<Piece>().GetType().ToString(), piece);
        }
    }

    /// <summary>
    /// Create piece of the given type
    /// </summary>
    /// <param name="type"></param>
    /// <returns>piece gameobject</returns>
    public GameObject CreatePiece(Type type)
    {
        GameObject prefab = nameToPieceDict[type.ToString()];
        if (prefab)
        {
            GameObject newPiece = Instantiate(prefab);

            return newPiece;
        }
        return null;
    }

    /// <summary>
    /// Gets the colour material needed for the piece
    /// </summary>
    /// <param name="colour"></param>
    /// <returns>Material</returns>
    public Material GetColourMaterial(Colour colour)
    {
        return colour == Colour.WHITE ? whiteMat : blackMat;
    }

    /// <summary>
    /// Gets the material used to highlight selected piece
    /// </summary>
    /// <returns></returns>
    public Material GetSelectedMat()
    {
        return selectedMat;
    }
}
