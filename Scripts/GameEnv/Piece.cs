using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]
[RequireComponent(typeof(Light))]
[Serializable]
public abstract class Piece : MonoBehaviour
{
    private MaterialSetter materialSetter;

    [SerializeField] private Board _board;

    public Board board {get {return _board; } set { _board = value;} }
    public Vector2Int occupiedSquare { get; set; }
    public Colour colour { get; set; }
    public bool hasMoved = false;
    public List<Vector2Int> availableMoves;

    public Vector2Int previousLocation { get; set;}
    public PieceType type { get; set; }


    private IObjectTweener tweener;

    private Light light;

    public PieceValue pieceValue;

    public int[,] bestPositionsOnBoard;

    public int GetBestPositionsOnBoardScore(int x, int y)
    {
        switch (this)
        {
            case Pawn:
                bestPositionsOnBoard = this.colour == Colour.BLACK ?
                new int[,]
                {
                    {100,  100,  100,  100,  100,  100,  100,  100},
                    {50, 50, 50, 50, 50, 50, 50, 50},
                    {0, 10, 20, 30, 30, 20, 10, 0},
                    {0,  5, 10, 25, 25, 10,  5,  0},
                    {0,  0,  0, 20, 20,  0,  0,  0},
                    {0, -5,-10,  0,  0,-10, -5,  0},
                    {0, 10, 10,-20,-20, 10, 10,  0},
                    {0,  0,  0,  0,  0,  0,  0,  0}
                } : 
                new int[,]
                {
                    {0,  0,  0,  0,  0,  0,  0,  0},
                    {5, 10, 10,-20,-20, 10, 10,  5},
                    {5, -5,-10,  0,  0,-10, -5,  5},
                    {0,  0,  0, 20, 20,  0,  0,  0},
                    {5,  5, 10, 25, 25, 10,  5,  5},
                    {10, 10, 20, 30, 30, 20, 10, 10},
                    {50, 50, 50, 50, 50, 50, 50, 50},
                    {0,  0,  0,  0,  0,  0,  0,  0}
                };
                break;
                case Knight:
                bestPositionsOnBoard = this.colour == Colour.BLACK ?
                new int[,]
                {
                    {-50,-40,-30,-30,-30,-30,-40,-50},
                    {-40,-20,  0,  0,  0,  0,-20,-40},
                    {-30,  0, 10, 15, 15, 10,  0,-30},
                    {-30,  0, 5, 20, 20, 5,  0,-30},
                    {-30,  0, 5, 20, 20, 5,  0,-30},
                    {-30,  0, 5, 5, 15, 5,  0,-30},
                    {-40,-20,  0,  5,  5,  0,-20,-40},
                    {-50,-40,-30,-30,-30,-30,-40,-50}
                } : 
                new int [,]
                {
                    {-50,-40,-30,-30,-30,-30,-40,-50},
                    {-40,-20,  0,  5,  5,  0,-20,-40},
                    {-30,  5, 10, 15, 15, 10,  5,-30},
                    {-30,  0, 15, 20, 20, 15,  0,-30},
                    {-30,  5, 15, 20, 20, 15,  5,-30},
                    {-30,  0, 10, 15, 15, 10,  0,-30},
                    {-40,-20,  0,  0,  0,  0,-20,-40},
                    {-50,-40,-30,-30,-30,-30,-40,-50}
                };
                break;
                case Bishop:
                bestPositionsOnBoard = this.colour == Colour.BLACK ?
                new int[,]
                {
                    {-20,-10,-10,-10,-10,-10,-10,-20},
                    {-10,  0,  0,  0,  0,  0,  0,-10},
                    {-10,  0,  5, 5, 5,  5,  0,-10},
                    {-10,  5,  5, 5, 5,  5,  5,-10},
                    {-10,  0, 5, 5, 5, 5,  0,-10},
                    {-10, 0, 5, 5, 5, 5, 0,-10},
                    {-10,  5,  0,  0,  0,  0,  5,-10},
                    {-20,-10,-10,-10,-10,-10,-10,-20}
                } : 
                new int [,]
                {
                    {-20,-10,-10,-10,-10,-10,-10,-20},
                    {-10,  5,  0,  0,  0,  0,  5,-10},
                    {-10, 10, 10, 10, 10, 10, 10,-10},
                    {-10,  0, 10, 10, 10, 10,  0,-10},
                    {-10,  5,  5, 10, 10,  5,  5,-10},
                    {-10,  0,  5, 10, 10,  5,  0,-10},
                    {-10,  0,  0,  0,  0,  0,  0,-10},
                    {-20,-10,-10,-10,-10,-10,-10,-20}

                };
                break;
                case Rook:
                bestPositionsOnBoard = this.colour == Colour.BLACK ?
                new int[,]
                {
                    { 0,  0,  0,  0,  0,  0,  0,  0},
                    { 5, 10, 10, 10, 10, 10, 10,  5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    { 0,  0,  0,  5,  5,  0,  0,  0}
                } :
                new int[,]
                {
                    { 0,  0,  0,  5,  5,  0,  0,  0},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    {-5,  0,  0,  0,  0,  0,  0, -5},
                    { 5, 10, 10, 10, 10, 10, 10,  5},
                    { 0,  0,  0,  0,  0,  0,  0,  0}

                };
                break;
            case Queen:
                bestPositionsOnBoard = this.colour == Colour.BLACK ?
                new int[,]
                {
                    {-20,-10,-10, -5, -5,-10,-10,-20},
                    {-10,  0,  0,  0,  0,  0,  0,-10},
                    {-10,  0,  5,  5,  5,  5,  0,-10},
                    { -5,  0,  5,  5,  5,  5,  0, -5},
                    {  0,  0,  5,  5,  5,  5,  0, -5},
                    {-10,  5,  10,  5,  5,  10,  0,-10},
                    {-10,  0,  5,  0,  0,  0,  0,-10},
                    {-20,-10,-10, -5, -5,-10,-10,-20}
                } :
                new int[,]
                {
                    {-20,-10,-10, -5, -5,-10,-10,-20},
                    {-10,  0,  5,  0,  0,  0,  0,-10},
                    {-10,  5,  5,  5,  5,  5,  0,-10},
                    { -5,  0,  5,  5,  5,  5,  0,  0},
                    { -5,  0,  5,  5,  5,  5,  0, -5},
                    {-10,  0,  5,  5,  5,  5,  0,-10},
                    {-10,  0,  0,  0,  0,  0,  0,-10},
                    {-20,-10,-10, -5, -5,-10,-10,-20},
                };
                break;
            case King:
            bestPositionsOnBoard = this.colour == Colour.BLACK ?
            new int[,]
            {
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-20,-30,-30,-40,-40,-30,-30,-20},
                {-10,-20,-20,-20,-20,-20,-20,-10},
                {0, 0,  0,  0,  0,  0, 0, 0},
                {0, 30, 0,  0,  0, 0, 30, 0}
            } : 
            new int[,]
            {
                {20, 30, 10,  0,  0, 10, 30, 20},
                {20, 20,  0,  0,  0,  0, 20, 20},
                {-10,-20,-20,-20,-20,-20,-20,-10},
                {-20,-30,-30,-40,-40,-30,-30,-20},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30},
                {-30,-40,-40,-50,-50,-40,-40,-30}
            };
                break;
            default:
                break;
        }

        return bestPositionsOnBoard[y, x];
    }

    /// <summary>
    /// Gets the Piece value
    /// </summary>
    /// <returns></returns>
    public PieceValue GetPieceValue()
    {
        return this.pieceValue;
    }

    /// <summary>
    /// Gets the squares available for a piece to move 
    /// </summary>
    /// <returns></returns>
    public abstract List<Vector2Int> SelectAvailableSquares();

    /// <summary>
    /// Called when piece is activated
    /// 
    /// Sets the necessary components and values for the piece
    /// </summary>
    public void Awake()
    {
        availableMoves = new List<Vector2Int>();
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();
        light = GetComponent<Light>();
        SetupLight();
    }

    /// <summary>
    /// Attaches the appropriate material to a piece
    /// </summary>
    /// <param name="material"></param>
    public void SetMaterial(Material material)
    {
        if (materialSetter == null)
        {
            materialSetter = GetComponent<MaterialSetter>();
        }
        materialSetter.SetSingleMaterial(material);
    }

    /// <summary>
    /// Attached a Light component to a piece
    /// </summary>
    public void SetupLight()
    {
        light.type = LightType.Point;
        light.color = Color.white;
        light.intensity = 0;
        light.range = 1;
        light.transform.position = new Vector3(light.transform.position.x, light.transform.position.y + 2, light.transform.position.z);
    }

    /// <summary>
    /// If the piece is black, rotate the piece 180 degrees
    /// </summary>
    /// <param name="piece"></param>
    public void RotatePieceIfBlack(Piece piece)
    {
        if (piece.colour == Colour.BLACK)
        {
            piece.transform.Rotate(0, 180, 0);
        }
    }

    /// <summary>
    /// Checks if this piece is attacking a piece of a given type
    /// </summary>
    /// <typeparam name="T">Piece type</typeparam>
    public bool IsAttackingPieceOfType<T>() where T : Piece
    {
        foreach (var square in availableMoves)
        {
            if (board.GetPieceOnSquare(square) is T)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the given piece is the same colour as this piece
    /// </summary>
    /// <param name="piece"></param>
    /// <returns>Bool</returns>
    public bool IsSameColour(Piece piece)
    {
        return colour == piece.colour;
    }

    /// <summary>
    /// Checks if this piece can move to the given coordinates
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Bool</returns>
    public bool CanMoveTo(Vector2Int coordinates)
    {
        return availableMoves.Contains(coordinates);
    }

    /// <summary>
    /// Move this piece to the given coordinates
    /// </summary>
    /// <param name="coordinates"></param>
    public virtual void MovePiece(Vector2Int coordinates)
    {
        previousLocation = occupiedSquare;
        Vector3 targetPosition = board.CalculatePositionFromCoordinates(coordinates);
        occupiedSquare = coordinates;
        hasMoved = true;
        tweener.MoveTo(transform, targetPosition);
        board.lastPieceMoved = this;
    }


    /// <summary>
    /// Attempt to add the given coordinates to the availableMoves list
    /// </summary>
    /// <param name="coordinates"></param>
    protected void TryToAddMove(Vector2Int coordinates)
    {
        availableMoves.Add(coordinates);
    }

    /// <summary>
    /// Initialize piece data
    /// </summary>
    /// <param name="coordinates">Location of the piece</param>
    /// <param name="colour">Colour of the piece</param>
    /// <param name="board">Board the piece is on</param>
    public void SetData(Vector2Int coordinates, Colour colour, Board board)
    {
        this.colour = colour;
        occupiedSquare = coordinates;
        this.board = board;
        transform.position = board.CalculatePositionFromCoordinates(coordinates);
        SetPieceType();
    }

    private void SetPieceType()
    {
        switch (this)
        {
            case Pawn:
                this.type = PieceType.Pawn;
                this.pieceValue = PieceValue.Pawn;
                break;

            case Knight:
                this.type = PieceType.Knight;
                this.pieceValue = PieceValue.Knight;
                break;

            case Bishop:
                this.type = PieceType.Bishop;
                this.pieceValue = PieceValue.Bishop;
                break;

            case Rook:
                this.type = PieceType.Rook;
                this.pieceValue = PieceValue.Rook;
                break;

            case Queen:
                this.type = PieceType.Queen;
                this.pieceValue = PieceValue.Queen;
                break;

            case King:
                this.type = PieceType.King;
                this.pieceValue = PieceValue.King;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Gets the next piece available from the given direction if the piece is of type T
    /// </summary>
    /// <typeparam name="T">Type to look for</typeparam>
    /// <param name="colour">Colour to look for</param>
    /// <param name="direction">Direction to check</param>
    /// <returns></returns>
    protected Piece GetPieceInDirection<T>(Colour colour, Vector2Int direction) where T : Piece
    {
        for (int i = 1; i <= board.BOARD_SIZE; i++)
        {
            Vector2Int nextCoordinates = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoordinates);
            if (!board.CoordinatesOnBoard(nextCoordinates))
            {
                return null;
            }
            if (piece != null)
            {
                if (piece.colour != colour || !(piece is T))
                {
                    return null;
                }
                else if (piece.colour == colour && piece is T)
                {
                    return piece;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Activate the light component
    /// </summary>
    public void TurnLightOn()
    {
        light.intensity = 5;
    }

    /// <summary>
    /// Deactivate the light component
    /// </summary>
    public void TurnLightOff()
    {
        light.intensity = 0;
    }

    /// <summary>
    /// Get the action performed by this piece
    /// </summary>
    /// <returns></returns>
    public TurnAction GetTurnAction()
    {
        TurnAction turnAction = new TurnAction(this);
        return turnAction;
    }

    /// <summary>
    /// Get the FEN notation of this piece
    /// </summary>
    /// <returns></returns>
    public string GetFENString()
    {
        string fenString = "";
        switch (this)
        {
            case Pawn:
                fenString = "p";
                break;

            case Knight:
                fenString = "n";
                break;

            case Bishop:
                fenString = "b";
                break;

            case Rook:
                fenString = "r";
                break;

            case Queen:
                fenString = "q";
                break;

            case King:
                fenString = "k";
                break;
                
            default:
                fenString = "Invalid piece";
                break;
        }

        if (colour == Colour.BLACK)
        {
            fenString = fenString.ToUpper();
        }
        return fenString;
    }
}



