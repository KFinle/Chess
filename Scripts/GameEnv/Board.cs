using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(SquareSelectorCreator))]
public class Board : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftSquarePos;
    [SerializeField] private float squareSize;
    [SerializeField] private Text turnPrintText;
    [SerializeField] private GameController? gameController;
    [SerializeField] private AIController? aiController;
    [SerializeField] private Text? turnHistoryTextBox;

    public bool isAIBoard;
    public bool isNetplay = false;
    public int BOARD_SIZE = 8;

    private int startX, startY, endX, endY;
    private float squareOffset = 0.5f;

    //turn display
    public Piece[,] grid;

    public Piece selectedPiece;
    private SquareSelectorCreator squareSelector;
    private SquareSelectorCreator hoverSquare;
    private Vector2Int previousHoverPos = new Vector2Int(-1, -1);

    private Vector3 hoverPos;
    private bool isFirstTurn = true;

    // Track previous moves
    public List<TurnAction> turnActions;
    public Piece lastPieceMoved;

    /// <summary>
    /// Called when the board object becomes active
    /// </summary>
    private void Awake()
    {
        squareSelector = GetComponent<SquareSelectorCreator>();
        hoverSquare = GetComponent<SquareSelectorCreator>();
        CreateGrid();
        if (Client.Instance.isAlive)
        {
            isNetplay = true;
            Register();
        }
    }

    /// <summary>
    /// Called once per frame
    /// </summary>
    private void Update()
    {
        if (!isAIBoard)
            Hover();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    /// <summary>
    /// detects the position of the mouse cursor and highlight 
    /// the square under the cursor if it's within the valid board boundaries. 
    /// It uses a Raycast to detect if the mouse is pointing at a valid square on 
    /// the board and then calls the HoverSelector method of the square to highlight it. 
    /// </summary>
    private void Hover()
    {
        RaycastHit hover;
        Ray rayHover = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(rayHover, out hover) && selectedPiece == null)
        {

            hoverPos = hover.point;
            if (CalculateCoordinatesFromPosition(hoverPos) != previousHoverPos)
            {
                hoverSquare.ClearSelection();
                hoverSquare.HoverSelector(hoverPos);
                previousHoverPos = CalculateCoordinatesFromPosition(hoverPos);
            }
            if (hoverPos.x < 1 || hoverPos.x > 9 || hoverPos.z > 9 || hoverPos.z < 1)
            {
                hoverSquare.ClearSelection();
            }
        }
    }


    /// <summary>
    /// Ensures that the gamecontroller passed is the gamecontroller assosiated with this object
    /// </summary>
    /// <param name="gameController"></param>
    public void SetDependancies(GameController gameController)
    {
        this.gameController = gameController;
    }

    /// <summary>
    /// Creates 8x8 grid to store array of pieces
    /// </summary>
    private void CreateGrid()
    {
        grid = new Piece[BOARD_SIZE, BOARD_SIZE];
    }

    /// <summary>
    /// Coverts a given Vector2Int board position into the corrosponding Vector3
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>Vector3Int of given coordinate</returns>
    public Vector3 CalculatePositionFromCoordinates(Vector2Int coordinates)
    {
        return bottomLeftSquarePos.position + new Vector3(coordinates.x * squareSize, 0f, coordinates.y * squareSize);
    }

    /// <summary>
    /// Calculates the board position of a given Vector3
    /// </summary>
    /// <param name="inputPosition"></param>
    /// <returns>Board coordinates</returns>
    public Vector2Int CalculateCoordinatesFromPosition(Vector3 inputPosition)
    {
        int x = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).x / squareSize + squareOffset);
        int y = Mathf.FloorToInt(transform.InverseTransformPoint(inputPosition).z / squareSize + squareOffset);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Fuction called when a square is selected.
    /// 
    /// If a square contains a piece, select that piece.
    /// Otherwise, deselect any currently selected piece
    /// </summary>
    /// <param name="inputPosition"></param>
    public void OnSquareSelected(Vector3 inputPosition)
    {
        if (!gameController.GameInProgress())
        {
            return;
        }

        Vector2Int coordinates = CalculateCoordinatesFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coordinates);

        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
            {
                DeselectPiece();
            }
            else if (piece != null && selectedPiece != piece && gameController.IsColourTurnActive(piece.colour))
            {
                if (isNetplay)
                {
                    if (NetPlaySelectCheck(piece)) SelectPiece(piece);
                }
                else
                {
                    SelectPiece(piece);
                }
            }
            else if (selectedPiece.CanMoveTo(coordinates))
            {
                OnSelectedPieceMoved(coordinates, selectedPiece);
            }
            else if (piece != null && gameController.IsColourTurnActive(piece.colour))
            {
                if (isNetplay)
                {
                    if (NetPlaySelectCheck(piece)) SelectPiece(piece);
                }
                else
                {
                    SelectPiece(piece);
                }
            }
        }

        else
        {
            if (piece != null && gameController.IsColourTurnActive(piece.colour))
            {
                if (isNetplay)
                {
                    if (NetPlaySelectCheck(piece)) SelectPiece(piece);
                }
                else
                {
                    SelectPiece(piece);
                }
            }
        }
    }

    /// <summary>
    /// Checks which netplay player is selecting a piece
    /// </summary>
    private bool NetPlaySelectCheck(Piece piece)
    {
        return (Client.Instance.player == 0 && piece.colour == Colour.WHITE) || (Client.Instance.player == 1 && piece.colour == Colour.BLACK);
    }

    /// <summary>
    /// Fuction called when a square is selected.
    /// 
    /// If a square contains a piece, select that piece.
    /// Otherwise, deselect any currently selected piece
    /// </summary>
    /// <param name="inputPosition"></param>
    public void OnAiSquareSelected(Vector2Int coordinates)
    {
        Piece piece = GetPieceOnSquare(coordinates);
        if (selectedPiece)
        {
            if (piece != null && selectedPiece == piece)
            {
                DeselectPiece();
            }
            else if (piece != null && selectedPiece != piece)
            {
                SelectPiece(piece);
            }
            else if (selectedPiece.CanMoveTo(coordinates))
            {
                OnSelectedPieceMoved(coordinates, selectedPiece);
            }
            else if (piece != null && gameController.IsColourTurnActive(piece.colour))
            {
                SelectPiece(piece);
            }
        }

        else
        {
            if (piece != null && gameController.IsColourTurnActive(piece.colour))
            {
                SelectPiece(piece);
            }
        }
    }

    /// <summary>
    /// Select a piece if the piece is not null and 
    /// show the possible moves that piece can move to
    /// </summary>
    /// <param name="piece"></param>
    public void SelectPiece(Piece piece)
    {
        if (selectedPiece != null)
        {
            selectedPiece.TurnLightOff();
            if (!isAIBoard) selectedPiece.SetMaterial(gameController.pieceCreator.GetColourMaterial(selectedPiece.colour));
        }
        if (!isAIBoard) gameController.RemoveMovesAllowingAttackOnPieceType<King>(piece);
        if (isAIBoard) aiController.RemoveMovesAllowingAttackOnPieceType<King>(piece);

        selectedPiece = piece;
        if (!isAIBoard) selectedPiece.TurnLightOn();
        selectedPiece.TurnLightOn();
        if (!isAIBoard) selectedPiece.SetMaterial(gameController.pieceCreator.GetSelectedMat());
        if (!isAIBoard) VarSelPiece.text = piece.GetType().ToString();
        List<Vector2Int> movelist = selectedPiece.availableMoves;
        ShowPossibleMoves(movelist);

    }

    /// <summary>
    /// Generates a dictionary of moves detailing if each possible move location 
    /// currently has a piece occupying the square then calls the square selector
    /// </summary>
    /// <param name="movelist"></param>
    private void ShowPossibleMoves(List<Vector2Int> movelist)
    {
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < movelist.Count; i++)
        {
            Vector3 position = CalculatePositionFromCoordinates(movelist[i]) - new Vector3(0.3f, 0, 0);
            bool isSquareFree = GetPieceOnSquare(movelist[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }

    /// <summary>
    /// Deselects a piece
    /// </summary>
    private void DeselectPiece()
    {
        selectedPiece.TurnLightOff();
        if (!isAIBoard) selectedPiece.SetMaterial(gameController.pieceCreator.GetColourMaterial(selectedPiece.colour));
        selectedPiece = null;
        squareSelector.ClearSelection();

    }

    /// <summary>
    /// Called when a piece is moved
    /// </summary>
    /// <param name="coordinates">Where the piece is moving</param>
    /// <param name="piece">The piece being moved</param>
    public void OnSelectedPieceMoved(Vector2Int coordinates, Piece piece, bool isForced = false)
    {
        gameController.UpdateHalfmoveClock(selectedPiece, coordinates);
        if (!isFirstTurn || !(gameController.fullmoveNumber == 1)) gameController.fullmoveNumber++;
        isFirstTurn = false;

        if (!isAIBoard) TakeOpponentPieceAttempt(coordinates);
        UpdateBoardOnPieceMoved(coordinates, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coordinates);
        DisplayMove(coordinates, piece);
        DeselectPiece();
        if (!isAIBoard) EndTurn();

        // Net handler
        if (isNetplay && !isForced)
        {
            startX = piece.previousLocation[0];
            startY = piece.previousLocation[1];
            endX = piece.occupiedSquare[0];
            endY = piece.occupiedSquare[1];
            NetMove nm = new NetMove();
            nm.currentX = startX;
            nm.currentY = startY;
            nm.finalX = endX;
            nm.finalY = endY;
            nm.player = Client.Instance.player;
            Client.Instance.ToServer(nm);
        }
    }

    /// <summary>
    /// Take opponent's piece if it exists
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    private void TakeOpponentPieceAttempt(Vector2Int coordinates)
    {
        Piece piece = GetPieceOnSquare(coordinates);

        if (piece != null && !selectedPiece.IsSameColour(piece))
        {
            TakePiece(piece);
        }
    }

    /// <summary>
    /// Destroys the given piece
    /// </summary>
    /// <param name="piece">Piece to destroy</param>
    public void TakePiece(Piece piece)
    {
        if (piece)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            gameController.OnPieceRemoved(piece);
        }
    }

    /// <summary>
    /// Calls the EndTurn() method on the gamecontroller
    /// </summary>
    private void EndTurn()
    {
        gameController.EndTurn();
    }

    /// <summary>
    /// Updated the grid state when a piece is moved
    /// </summary>
    /// <param name="newCoordinates"></param>
    /// <param name="oldCoordinates"></param>
    /// <param name="newPiece"></param>
    /// <param name="oldPiece"></param>
    public void UpdateBoardOnPieceMoved(Vector2Int newCoordinates, Vector2Int oldCoordinates, Piece newPiece, Piece oldPiece)
    {
        grid[oldCoordinates.x, oldCoordinates.y] = oldPiece;
        grid[newCoordinates.x, newCoordinates.y] = newPiece;

    }

    /// <summary>
    /// If there is a piece on a square, return the piece
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    /// <returns></returns>
    public Piece GetPieceOnSquare(Vector2Int coordinates)
    {
        if (CoordinatesOnBoard(coordinates))
        {
            return grid[coordinates.x, coordinates.y];
        }
        return null;
    }

    /// <summary>
    /// Checks if the given coordinates are on the board
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    public bool CoordinatesOnBoard(Vector2Int coordinates)
    {
        if (coordinates.x < 0 || coordinates.y < 0 || coordinates.x >= BOARD_SIZE || coordinates.y >= BOARD_SIZE)
        {
            return false;
        }
        return true;

    }

    /// <summary>
    /// Checks if a square has a specific piece
    /// </summary>
    /// <param name="piece">Piece to look for</param>
    /// <returns></returns>
    public bool HasPiece(Piece piece)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (grid[i, j] == piece)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Places the given piece on the given coordinates
    /// </summary>
    /// <param name="coordinates">Coordinates to place the piece</param>
    /// <param name="piece">Piece to place</param>
    public void SetPieceOnBoard(Vector2Int coordinates, Piece piece)
    {
        if (CoordinatesOnBoard(coordinates))
        {
            grid[coordinates.x, coordinates.y] = piece;
        }
    }

    /// <summary>
    /// Called when the game restarts
    /// 
    /// Calls the CreateGrid() method and removes selected piece
    /// </summary>
    internal void OnGameRestart()
    {
        isFirstTurn = true;
        selectedPiece = null;
        CreateGrid();
    }

    /// <summary>
    /// Changes the given piece to a Queen.
    /// Long term, this should be changed to allow for other piece types but for V1 we default to Queen
    /// </summary>
    /// <param name="piece">The pawn to promote</param>
    public void PromotePiece(Piece piece)
    {
        TakePiece(piece);
        gameController.CreatePieceAndInitialize(piece.occupiedSquare, piece.colour, typeof(Queen));
    }

    /// <summary>
    /// Displays the most recent turn action taken
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="piece"></param>
    public void DisplayMove(Vector2Int coordinates, Piece piece)
    {
        TurnAction turnAction = new TurnAction(piece);
        if (!isNetplay && !isAIBoard) turnHistoryTextBox.text += turnAction.PrintTurnAction();
        //turnAction.PrintTurnAction();

        if (!isAIBoard) turnPrintText.text = turnAction.PrintTurnAction();
    }

    

    /// <summary>
    /// Clears the turn text
    /// </summary>
    public void ClearTurnText()
    {
        turnPrintText.text = "";
    }

    private void Register()
    {
        NetUtil.SV_MOVE += ServMove;
        NetUtil.CL_MOVE += ClMove;
    }
    private void Unregister()
    {
        NetUtil.SV_MOVE -= ServMove;
        NetUtil.CL_MOVE -= ClMove;

    }

    private void ClMove(NetMessage msg)
    {
        NetMove move = msg as NetMove;
        if (move.player != Client.Instance.player)
        {
            Vector2Int vec = new Vector2Int(move.currentX, move.currentY);
            Piece piece = grid[vec.x, vec.y];
            SelectPiece(piece);
            OnSelectedPieceMoved(new Vector2Int(move.finalX, move.finalY), piece, true);
        }

    }
    private void ServMove(NetMessage msg, NetworkConnection conn)
    {
        NetMove move = msg as NetMove;
        Server.Instance.Broadcast(move);
    }



}
