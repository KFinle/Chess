using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;
using System.IO;

public class SaveStateManager : MonoBehaviour
{
    public class GameSaveData
    {
        public string fenString;
        public string hasMovedString;
        public bool isAiGame;
        public Vector2Int lastPieceMovedOccupiedSquare;
        public Vector2Int lastPieceMovedPreviousLocation;
        public string turnHistoryTextBox;
        public string turnPrintText;
    }

    static string filePath;
    private const int BOARD_SIZE = 8;

    [SerializeField] private GameController gameController;
    [SerializeField] private Board board;

    /// <summary>
    /// Called when activated 
    /// 
    /// Sets the file path for storing and loading game data
    /// </summary>
    void Awake()
    {
        filePath = Application.persistentDataPath + "/SaveFile.json";
    }

    /// <summary>
    /// Debugging
    /// 
    /// Displays message if something goes wrong
    /// </summary>
    /// <param name="ex">Exception to get message from</param>
    private void HandleException(Exception ex)
    {
        Debug.Log($"Something terrible happened: {ex.Message}");
    }

    /// <summary>
    /// Saves the game to a JSON object containing a FEN string,
    /// stores in a file at the filepath
    /// </summary>
    public void SaveGameToJson()
    {
        string fenString = GenerateFenString();
        string hasMovedString = GenerateHasMovedString();
        bool isAiGame = GetIsAiGame();
        Vector2Int lastPieceMovedOccupiedSquare = new Vector2Int(0, 0);
        Vector2Int lastPieceMovedPreviousLocation = new Vector2Int(0, 0);

        if (board.lastPieceMoved != null)
        {
            lastPieceMovedOccupiedSquare = board.lastPieceMoved.occupiedSquare;
            lastPieceMovedPreviousLocation = board.lastPieceMoved.previousLocation;
        }

        string turnHistoryTextBox = board.turnHistoryTextBox.text;
        string turnPrintText = board.turnPrintText.text;

        WriteToJsonFile(fenString, hasMovedString, isAiGame, lastPieceMovedOccupiedSquare, lastPieceMovedPreviousLocation, turnHistoryTextBox, turnPrintText);

        // Hide pause menu
        gameController.pauseScreen.SetActive(false);
    }

    /// <summary>
    /// Generates the FENstring to save
    /// </summary>
    /// <returns></returns>
    private string GenerateFenString()
    {
        string fenString = (GetFenPieces()
            + " " + GetFenActivePlayer()
            + " " + GetFenCastlingAvailability()
            + " " + GetFenEnPassant()
            + " " + GetFenHalfMoveClock()
            + " " + GetFenFullMoveNumber());

        return fenString;
    }

    /// <summary>
    /// Get the partial FEN string containing piece data
    /// </summary>
    /// <returns>String</returns>
    private string GetFenPieces()
    {
        string fenString = "";
        int emptyTilesCount = 0;

        for (int rank = 1; rank <= BOARD_SIZE; rank++)
        {
            for (int file = 1; file <= BOARD_SIZE; file++)
            {
                // Get the ChessPiece on the current square if it exists
                Vector2Int coordinates = new Vector2Int(file - 1, rank - 1);
                Piece piece = board.GetPieceOnSquare(coordinates);

                // If there is a piece on the square, update the FEN string and the empty tiles count
                if (piece != null)
                {
                    if (emptyTilesCount > 0)
                    {
                        fenString += emptyTilesCount;
                        emptyTilesCount = 0;
                    }

                    fenString += piece.GetFENString();
                }
                // Otherwise, increment the empty tiles count
                else
                {
                    emptyTilesCount++;
                }
            }

            // If there are empty tiles at the end of the rank, append their count
            if (emptyTilesCount > 0)
            {
                fenString += emptyTilesCount;
                emptyTilesCount = 0;
            }

            // Append a slash to separate ranks
            fenString += '/';
        }
        // Remove the final slash from the FEN string
        fenString = fenString.Remove(fenString.Length - 1, 1);
        return fenString;
    }

    /// <summary>
    /// Gets partial FEN string containing player data
    /// </summary>
    /// <returns>String</returns>
    private string GetFenActivePlayer()
    {
        return (gameController.IsColourTurnActive(Colour.WHITE) ? "w" : "b");
    }

    /// <summary>
    /// Gets partial FEN string contianing Castling data
    /// </summary>
    /// <returns>String</returns>
    private string GetFenCastlingAvailability()
    {
        string castlingAvailability = "";

        // Starting location constants
        Vector2Int WHITE_KING_START = new Vector2Int(4, 0);
        Vector2Int BLACK_KING_START = new Vector2Int(4, 7);
        Vector2Int WHITE_QS_ROOK_START = new Vector2Int(0, 0);
        Vector2Int WHITE_KS_ROOK_START = new Vector2Int(7, 0);
        Vector2Int BLACK_QS_ROOK_START = new Vector2Int(0, 7);
        Vector2Int BLACK_KS_ROOK_START = new Vector2Int(7, 7);

        // Try to get pieces based on starting locations
        Piece whiteKing = board.GetPieceOnSquare(WHITE_KING_START);
        Piece blackKing = board.GetPieceOnSquare(BLACK_KING_START);
        Piece whiteQsRook = board.GetPieceOnSquare(WHITE_QS_ROOK_START);
        Piece whiteKsRook = board.GetPieceOnSquare(WHITE_KS_ROOK_START);
        Piece blackQsRook = board.GetPieceOnSquare(BLACK_QS_ROOK_START);
        Piece blackKsRook = board.GetPieceOnSquare(BLACK_KS_ROOK_START);

        // Update castling availability string
        if (whiteKing != null && !whiteKing.hasMoved)
        {
            castlingAvailability += (whiteKsRook != null && !whiteKsRook.hasMoved) ? "K" : "";
            castlingAvailability += (whiteQsRook != null && !whiteQsRook.hasMoved) ? "Q" : "";
        }

        if (blackKing != null && !blackKing.hasMoved)
        {
            castlingAvailability += (blackKsRook != null && !blackKsRook.hasMoved) ? "k" : "";
            castlingAvailability += (blackQsRook != null && !blackQsRook.hasMoved) ? "q" : "";
        }

        if (castlingAvailability == "")
        {
            castlingAvailability = "-";
        }
        return castlingAvailability;
    }

    /// <summary>
    /// Gets partial FEN string contianing EnPassant data
    /// </summary>
    /// <returns></returns>
    private string GetFenEnPassant()
    {
        // Iterate through each square on the board
        for (int rank = 1; rank <= BOARD_SIZE; rank++)
        {
            for (int file = 1; file <= BOARD_SIZE; file++)
            {
                // Get the ChessPiece on the current square if it exists
                Vector2Int coordinates = new Vector2Int(file - 1, rank - 1);
                Piece piece = board.GetPieceOnSquare(coordinates);

                // If there is a pawn on the square, check for En Passant
                if (piece != null && piece.type == PieceType.Pawn)
                {
                    int enPassantRow = piece.colour == Colour.WHITE ? 5 : 4;
                    Piece lastPieceMoved = board.lastPieceMoved;

                    int takeDirectionY = piece.colour == Colour.WHITE ? 1 : -1;

                    if (lastPieceMoved != null && lastPieceMoved.type == PieceType.Pawn)
                    {
                        int lastMoveDistance = Mathf.Abs(lastPieceMoved.occupiedSquare.y - lastPieceMoved.previousLocation.y);
                        if (lastMoveDistance == 2)
                        {
                            if (lastPieceMoved.occupiedSquare.y == piece.occupiedSquare.y)
                            {
                                string enPassantRank = "";
                                string enPassantFile = "";
                                if (lastPieceMoved.occupiedSquare.x == piece.occupiedSquare.x + 1)
                                {
                                    int fileIndex = piece.occupiedSquare.x + 1;
                                    char fileChar = (char)('a' + fileIndex);
                                    enPassantFile = fileChar.ToString();
                                    enPassantRank = (piece.occupiedSquare.y + takeDirectionY + 1).ToString();

                                    return enPassantFile + enPassantRank;
                                }

                                if (lastPieceMoved.occupiedSquare.x == piece.occupiedSquare.x - 1)
                                {
                                    int fileIndex = piece.occupiedSquare.x - 1;
                                    char fileChar = (char)('a' + fileIndex);
                                    enPassantFile = fileChar.ToString();
                                    enPassantRank = (piece.occupiedSquare.y + takeDirectionY + 1).ToString();
                                    return enPassantFile + enPassantRank;
                                }
                            }
                        }
                    }
                }
            }
        }
        return "-";
    }

    /// <summary>
    /// Gets partial FEN string contianing turn clock data
    /// </summary>
    /// <returns></returns>
    private string GetFenHalfMoveClock()
    {
        return gameController.halfmoveClock.ToString();
    }

    /// <summary>
    /// Gets partial FEN string contianing full turn clock data
    /// </summary>
    /// <returns></returns>
    private string GetFenFullMoveNumber()
    {
        return gameController.fullmoveNumber.ToString();
    }

    /// <summary>
    /// Gets string containing hasMoves data
    /// </summary>
    /// <returns></returns>
    private string GenerateHasMovedString()
    {
        string hasMovedString = "";

        // Iterate through each square on the board
        for (int rank = 1; rank <= BOARD_SIZE; rank++)
        {
            for (int file = 1; file <= BOARD_SIZE; file++)
            {
                // Get the ChessPiece on the current square if it exists
                Vector2Int coordinates = new Vector2Int(file - 1, rank - 1);
                Piece piece = board.GetPieceOnSquare(coordinates);

                // If there is a piece on the square, update the FEN string and the empty tiles count
                if (piece != null)
                {
                    hasMovedString += (piece.hasMoved) ? "1" : "0";
                }
            }
        }
        return hasMovedString;
    }

    /// <summary>
    /// Write JSON object containing all game info to file at filepath
    /// </summary>
    /// <param name="fenString"></param>
    /// <param name="hasMovedString"></param>
    private void WriteToJsonFile(
        string fenString,
        string hasMovedString,
        bool isAiGame,
        Vector2Int lastPieceMovedOccupiedSquare,
        Vector2Int lastPieceMovedPreviousLocation,
        string turnHistoryTextBox,
        string turnPrintText
    )
    {
        GameSaveData saveData = new GameSaveData();

        saveData.fenString = fenString;
        saveData.hasMovedString = hasMovedString;
        saveData.isAiGame = isAiGame;
        saveData.lastPieceMovedOccupiedSquare = lastPieceMovedOccupiedSquare;
        saveData.lastPieceMovedPreviousLocation = lastPieceMovedPreviousLocation;
        saveData.turnHistoryTextBox = turnHistoryTextBox;
        saveData.turnPrintText = turnPrintText;

        string saveDataJson = JsonUtility.ToJson(saveData);

        File.WriteAllText(filePath, saveDataJson);
    }

    /// <summary>
    /// Loads new game by reading the data stored in a JSON file at filepath
    /// </summary>
    public void LoadGameFromJson()
    {
        // End the function if no save file exists
        if (!File.Exists(filePath))
        {
            return;
        }

        // Get the load data from the save file
        string saveFileContents = File.ReadAllText(filePath);
        GameSaveData loadData = JsonUtility.FromJson<GameSaveData>(saveFileContents);

        // Create new BoardLayout
        BoardLayout boardLayout = GenerateBoardLayout(loadData);

        // Get active player colour
        string activePlayerFen = GetActivePlayerFen(loadData);

        // Load the game
        gameController.LoadGame(boardLayout, activePlayerFen);

        // Update hasMoved statuses for all pieces
        UpdateHasMovedStatus(loadData);

        // Update last piece moved
        board.lastPieceMoved = board.GetPieceOnSquare(loadData.lastPieceMovedOccupiedSquare);

        // Update last piece moved previous location
        board.lastPieceMoved.previousLocation = loadData.lastPieceMovedPreviousLocation;

        // Generate all possible moves
        gameController.GenerateAllPossibleMoves(gameController.activePlayer);

        // Hide pause menu
        gameController.pauseScreen.SetActive(false);

        // Update is AI game status
        gameController.isAiGame = loadData.isAiGame;

        // Load turn history text
        board.turnHistoryTextBox.text = loadData.turnHistoryTextBox;

        // Load current turn text
        board.turnPrintText.text = loadData.turnPrintText;

        // Update Halfmove Clock
        gameController.halfmoveClock = GetHalfmoveClock(loadData);

        // Update Fullmove number
        gameController.fullmoveNumber = GetFullmoveNumber(loadData);
    }

    /// <summary>
    /// Creates new board layout containing initialization data for a loaded game
    /// </summary>
    /// <param name="loadData"></param>
    /// <returns>BoardLayout</returns>
    private BoardLayout GenerateBoardLayout(GameSaveData loadData)
    {
        BoardLayout boardLayout = ScriptableObject.CreateInstance<BoardLayout>();

        // Parse the FEN string to extract piece positions
        // and store in a BoardLayout object
        string[] fenParts = loadData.fenString.Split(' ');
        string[] ranks = fenParts[0].Split('/');
        List<BoardLayout.BoardSquareSetup> boardSquares = new List<BoardLayout.BoardSquareSetup>();
        int squareIndex = 0;

        for (int rankIndex = 0; rankIndex < ranks.Length; rankIndex++)
        {
            string rank = ranks[rankIndex];
            int fileIndex = 0;

            for (int charIndex = 0; charIndex < rank.Length; charIndex++)
            {
                char pieceChar = rank[charIndex];

                if (char.IsDigit(pieceChar))
                {
                    squareIndex += int.Parse(pieceChar.ToString());
                    fileIndex += int.Parse(pieceChar.ToString());
                }
                else
                {
                    PieceType pieceType = GetPieceTypeFromChar(pieceChar);
                    Colour colour = GetColourFromChar(pieceChar);
                    Vector2Int position = new Vector2Int(fileIndex + 1, rankIndex + 1);

                    boardSquares.Add(new BoardLayout.BoardSquareSetup
                    {
                        position = position,
                        pieceType = pieceType,
                        colour = colour
                    });

                    squareIndex++;
                    fileIndex++;
                }
            }
        }

        boardLayout.boardSquares = boardSquares.ToArray();
        return boardLayout;
    }

    /// <summary>
    /// Returns piece type based on FEN character
    /// </summary>
    /// <param name="pieceChar"></param>
    /// <returns>PieceType</returns>
    private PieceType GetPieceTypeFromChar(char pieceChar)
    {
        switch (pieceChar)
        {
            case 'K': return PieceType.King;
            case 'Q': return PieceType.Queen;
            case 'R': return PieceType.Rook;
            case 'B': return PieceType.Bishop;
            case 'N': return PieceType.Knight;
            case 'P': return PieceType.Pawn;
            case 'k': return PieceType.King;
            case 'q': return PieceType.Queen;
            case 'r': return PieceType.Rook;
            case 'b': return PieceType.Bishop;
            case 'n': return PieceType.Knight;
            case 'p': return PieceType.Pawn;
            default: return new PieceType();
        }
    }

    /// <summary>
    /// Returns piece colour based on FEN character
    /// </summary>
    /// <param name="pieceChar"></param>
    /// <returns>Colour</returns>
    private Colour GetColourFromChar(char pieceChar)
    {
        return ("KQRBNP".Contains(pieceChar)) ? Colour.BLACK : Colour.WHITE;
    }

    /// <summary>
    /// Returns active player based on FEN data
    /// </summary>
    /// <param name="loadData">Data to read</param>
    /// <returns>String</returns>
    private string GetActivePlayerFen(GameSaveData loadData)
    {
        string[] fenParts = loadData.fenString.Split(' ');
        return fenParts[1];
    }

    /// <summary>
    /// Returns hasMoved for each piece based on FEN data
    /// </summary>
    /// <param name="loadData">Data to read</param>
    private void UpdateHasMovedStatus(GameSaveData loadData)
    {
        int pieceIndex = 0;
        for (int rank = 1; rank <= BOARD_SIZE; rank++)
        {
            for (int file = 1; file <= BOARD_SIZE; file++)
            {
                Vector2Int coordinates = new Vector2Int(file - 1, rank - 1);
                Piece piece = board.GetPieceOnSquare(coordinates);

                if (piece != null)
                {
                    piece.hasMoved = loadData.hasMovedString[pieceIndex] == '1';

                    pieceIndex++;
                }
            }
        }
    }

    /// <summary>
    /// Returns if the game is Ai
    /// </summary>
    private bool GetIsAiGame()
    {
        return gameController.isAiGame;
    }

    /// <summary>
    /// Returns Halfmove Clock
    /// </summary>
    private int GetHalfmoveClock(GameSaveData loadData)
    {
        return int.Parse(loadData.fenString.Split(' ')[4]);
    }

    /// <summary>
    /// Returns Fullmove number
    /// </summary>
    private int GetFullmoveNumber(GameSaveData loadData)
    {
        return int.Parse(loadData.fenString.Split(' ')[5]);
    }
}