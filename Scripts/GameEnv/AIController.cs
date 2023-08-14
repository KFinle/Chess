using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PieceCreator))]

public class AIController : MonoBehaviour
{
    [SerializeField] private BoardLayout initialLayout;

    public Board aiboard;
    public bool isAiGame;
    public PieceCreator pieceCreator;
    public Player whitePlayer;
    public Player blackPlayer;
    public Player activePlayer;
    
    private GameState gameState;

    public AISaveStateManager aiSaveState;
    private enum GameState
    {
        INIT,
        PLAY,
        FINISHED
    }

    /// <summary>
    /// Called on activation
    /// </summary>
    private void Awake()
    {
        SetDependancies();
        CreatePlayers();
    }

    /// <summary>
    /// Set the PieceCreator dependancy
    /// </summary>
    private void SetDependancies()
    {
        pieceCreator = GetComponent<PieceCreator>();
    }

    /// <summary>
    /// Creates both players
    /// </summary>
    private void CreatePlayers()
    {
        whitePlayer = new Player(Colour.WHITE, aiboard);
        blackPlayer = new Player(Colour.BLACK, aiboard);

        whitePlayer.name = "WhiteSIM";
        blackPlayer.name = "BlackSIM";
    }

    /// <summary>
    /// Called after Awake() resolves
    /// </summary>
    void Start()
    {
        StartNewGame();
    }

    /// <summary>
    /// Calls methods required for starting a new game
    /// </summary>
    private void StartNewGame()
    {

        SetGameState(GameState.INIT);
        //aiboard.SetDependancies(this);
        CreatePiecesFromLayout(initialLayout);
        activePlayer = whitePlayer;
        GenerateAllPossibleMoves(activePlayer);
        SetGameState(GameState.PLAY);
    }

    /// <summary>
    /// Initialize game based on passed fenstring
    /// </summary>
    /// <param name="boardLayout">The aiboard layout ot use for loading</param>
    /// <param name="activePlayerFen">The FEN string to load</param>
    public void LoadGame(BoardLayout boardLayout, string activePlayerFen)
    {
        DestroyPieces();
        aiboard.OnGameRestart();
        whitePlayer.OnGameRestart();
        blackPlayer.OnGameRestart();
        CreatePiecesFromLayout(boardLayout);

        if (activePlayerFen == "w")
        {
            activePlayer = whitePlayer;
        }
        else if (activePlayerFen == "b")
        {
            activePlayer = blackPlayer;
        }
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        DestroyPieces();
        aiboard.OnGameRestart();
        whitePlayer.OnGameRestart();
        blackPlayer.OnGameRestart();
        StartNewGame();
    }

    /// <summary>
    /// Destroys all active game pieces
    /// </summary>
    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(piece => Destroy(piece.gameObject));
        blackPlayer.activePieces.ForEach(piece => Destroy(piece.gameObject));

    }

    /// <summary>
    /// Checks if a game is running
    /// </summary>
    public bool GameInProgress()
    {
        return gameState == GameState.PLAY;
    }

    /// <summary>
    /// Sets the gamestate
    /// </summary>
    /// <param name="gameState"></param>
    private void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }

    /// <summary>
    /// Generates all pieces required by the layout passed
    /// </summary>
    /// <param name="layout">Piece data</param>
    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetNumPieces(); i++)
        {
            Vector2Int squareCoordinates = layout.GetSquareCoordinates(i);
            Colour colour = layout.GetColourAtSquare(i);
            String pieceName = layout.GetPieceNameAtSquare(i);
            Type type = Type.GetType(pieceName);
            CreatePieceAndInitialize(squareCoordinates, colour, type);
        }
    }

    /// <summary>
    /// Checks if the current active player is the player passed by the function
    /// </summary>
    /// <param name="colour">The colour to check</param>
    /// <returns>Bool</returns>
    public bool IsColourTurnActive(Colour colour)
    {
        return activePlayer.colour == colour;
    }


    /// <summary>
    /// Create a new piece and initialize its location and data
    /// </summary>
    /// <param name="squareCoordinates">Where to place the piece</param>
    /// <param name="colour">Colour of the piece</param>
    /// <param name="type">Type of piece</param>
    public void CreatePieceAndInitialize(Vector2Int squareCoordinates, Colour colour, Type type)
    {
        Piece piece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        piece.SetData(squareCoordinates, colour, aiboard);
        Material colourMaterial = pieceCreator.GetColourMaterial(colour);
        Material selectedMaterial = pieceCreator.GetSelectedMat();
        piece.SetMaterial(colourMaterial);
        piece.RotatePieceIfBlack(piece);
        aiboard.SetPieceOnBoard(squareCoordinates, piece);
        Player currentPlayer = colour == Colour.WHITE ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(piece);
    }

    public void RestoreTestedPieceToOriginal(Piece originalPiece, Piece pieceToRestore)
    {

        CreatePieceAndInitialize(originalPiece.occupiedSquare, originalPiece.colour, originalPiece.GetType());
        Destroy(pieceToRestore);
    }

    /// <summary>
    /// Calls player.GenerateAllPossibleMoves()
    /// </summary>
    /// <param name="player"></param>
    public void GenerateAllPossibleMoves(Player player)
    {
        player.GenerateAllPossibleMoves();
    }

  
    /// <summary>
    /// Caleld when a piece is destroyed
    /// </summary>
    /// <param name="piece"></param>
    public void OnPieceRemoved(Piece piece)
    {
        Player piecePlayer = (piece.colour == Colour.WHITE) ? whitePlayer : blackPlayer;
        piecePlayer.RemovePiece(piece);
        Destroy(piece.gameObject);
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    public void EndGame()
    {
        //Debug.Log("Game ended");
        Debug.Log(GameState.FINISHED);
    }

    /// <summary>
    /// Checks if the game is ending based on the most recent action taken.
    /// This is the function used to check for Checkmate
    /// </summary>
    public bool CheckGameFinished()
    {
        Piece[] piecesAttackingKing = activePlayer.GetAttackingPieceOfType<King>();
        if (piecesAttackingKing.Length > 0)
        {   activePlayer.inCheck = true;
            Player opponent = GetOppenentToPlayer(activePlayer);
            Piece defendingKing = opponent.GetPiecesOfType<King>().FirstOrDefault();
            opponent.RemoveMovesAllowingAttackOnPiece<King>(activePlayer, defendingKing);
            int availableMovesForKing = defendingKing.availableMoves.Count;
            if (availableMovesForKing == 0)
            {
                bool canProtectKing = opponent.CanProtectPiece<King>(activePlayer);
                if (!canProtectKing)
                {
                    return true;
                }
            }
        }
        activePlayer.inCheck = false;
        return false;
    }

    /// <summary>
    /// Get the opponent of the player passed 
    /// </summary>
    /// <param name="player">Player to check opponent of</param>
    /// <returns>Opponent</returns>
    public Player GetOppenentToPlayer(Player player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    

    /// <summary>
    /// Calls RemoveMovesAllowingAttackOnPieceType<T>() method from the Player class
    /// </summary>
    /// <typeparam name="T">Type of piece to check</typeparam>
    /// <param name="piece">Piece to check</param>
    public void RemoveMovesAllowingAttackOnPieceType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesAllowingAttackOnPiece<T>(GetOppenentToPlayer(activePlayer), piece);
    }

    public void ChangeAIActivePlayer()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

}
