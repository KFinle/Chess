using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Text;
using System.IO;

public class AISaveStateManager : MonoBehaviour
{
    public class GameSaveData
    {
        public string fenString;
        public string hasMovedString;
        public bool isAiGame;
    }

    static string filePath;
    private const int BOARD_SIZE = 8;

    [SerializeField] private AIController aiController;
    [SerializeField] private Board aiboard;
    [SerializeField] private Board mainBoard;

    /// <summary>
    /// Called when activated 
    /// 
    /// Sets the file path for storing and loading game data
    /// </summary>
    void Awake()
    {
        filePath = Application.persistentDataPath + "/AISaveFile.json";
    }

    /// <summary>
    /// Called once per frame
    /// 
    /// Checks for Save/Load key hotkeys
    /// </summary>
    private void Update()
    {
        // Save game on pressing "S" key
        if (Input.GetKeyDown(KeyCode.Z))
        {
            
            SaveGameToJson();
        }
  
        

        // Load game on pressing "L" key
        if (Input.GetKeyDown(KeyCode.X))
        {
            try
            {
                LoadGameFromJson();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
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
        WriteToJsonFile(fenString, hasMovedString, isAiGame);


    }

    /// <summary>
    /// Generates the FENstring to save
    /// </summary>
    /// <returns></returns>
    public string GenerateFenString()
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
                Piece piece = mainBoard.GetPieceOnSquare(coordinates);

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
        return (aiController.IsColourTurnActive(Colour.WHITE) ? "w" : "b");
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
        Piece whiteKing = mainBoard.GetPieceOnSquare(WHITE_KING_START);
        Piece blackKing = mainBoard.GetPieceOnSquare(BLACK_KING_START);
        Piece whiteQsRook = mainBoard.GetPieceOnSquare(WHITE_QS_ROOK_START);
        Piece whiteKsRook = mainBoard.GetPieceOnSquare(WHITE_KS_ROOK_START);
        Piece blackQsRook = mainBoard.GetPieceOnSquare(BLACK_QS_ROOK_START);
        Piece blackKsRook = mainBoard.GetPieceOnSquare(BLACK_KS_ROOK_START);

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
        // TODO: implement logic to update en passant square
        return "-";
    }

    /// <summary>
    /// Gets partial FEN string contianing turn clock data
    /// </summary>
    /// <returns></returns>
    private string GetFenHalfMoveClock()
    {
        // TODO: implement logic to update halfmove clock
        return "0";
    }

    /// <summary>
    /// Gets partial FEN string contianing full turn clock data
    /// </summary>
    /// <returns></returns>
    private string GetFenFullMoveNumber()
    {
        // TODO: implement logic to update fullmove number
        return "1";
    }

    /// <summary>
    /// Gets string containing hasMoves data
    /// </summary>
    /// <returns></returns>
    private string GenerateHasMovedString()
    {
        string hasMovedString = "";

        // Iterate through each square on the mainBoard
        for (int rank = 1; rank <= BOARD_SIZE; rank++)
        {
            for (int file = 1; file <= BOARD_SIZE; file++)
            {
                // Get the ChessPiece on the current square if it exists
                Vector2Int coordinates = new Vector2Int(file - 1, rank - 1);
                Piece piece = mainBoard.GetPieceOnSquare(coordinates);

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
    private void WriteToJsonFile(string fenString, string hasMovedString, bool isAiGame)
    {
        GameSaveData saveData = new GameSaveData();

        saveData.fenString = fenString;
        saveData.hasMovedString = hasMovedString;
        saveData.isAiGame = isAiGame;

        string saveDataJson = JsonUtility.ToJson(saveData);

        File.WriteAllText(filePath, saveDataJson);

        //Debug.Log($"Save data: {saveDataJson}\n Saved to: {filePath}");
    }

    /// <summary>
    /// Loads new game by reading the data stored in a JSON file at filepath
    /// </summary>
    public void LoadGameFromJson()
    {
        // End the function if no save file exists
        if (!File.Exists(filePath))
        {
            //Debug.Log("No save file exists!");
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
        aiController.LoadGame(boardLayout, activePlayerFen);

        // Update hasMoved statuses for all pieces
        UpdateHasMovedStatus(loadData);

        // Generate all possible moves
        aiController.GenerateAllPossibleMoves(aiController.activePlayer);


        // Update is AI game status
        aiController.isAiGame = loadData.isAiGame;

        //debug
        //Debug.Log("Loaded from: " + filePath);

    }

    /// <summary>
    /// Creates new aiboard layout containing initialization data for a loaded game
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
                Piece piece = aiboard.GetPieceOnSquare(coordinates);

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
        return aiController.isAiGame;
    }

    public string GetCurrentFenFromSave()
    {
        string saveFileContents = File.ReadAllText(filePath);
        GameSaveData loadData = JsonUtility.FromJson<GameSaveData>(saveFileContents);
        return loadData.fenString;

    }
}