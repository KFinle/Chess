using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PieceCreator))]
[RequireComponent(typeof(Minimax))]
public class GameController : MonoBehaviour
{
    [SerializeField] private BoardLayout initialLayout;
    [SerializeField] private Board board;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private CameraController camera;
    [SerializeField] public GameObject pauseScreen;
    [SerializeField] public GameObject historyScreen;

    public Button restartButton;
    public GameObject? chatbarContainer;

    public ChatManager chatManager;

    public AISaveStateManager aiSaveState;
    public Minimax minimax;
    public PieceCreator pieceCreator;
    public Player whitePlayer;
    public Player blackPlayer;
    public Player activePlayer;
    private GameState gameState;
    [SerializeField] public bool isAiGame;
    private bool conceeded = false;
    public int halfmoveClock = 0;
    public int fullmoveNumber = 1;

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
        Register();
    }

    private void OnDestroy()
    {
        Client.Instance.ToServer(new NetForfeit());
        Unregister();
    }

    /// <summary>
    /// Set the PieceCreator dependancy
    /// </summary>
    private void SetDependancies()
    {
        pieceCreator = GetComponent<PieceCreator>();
        minimax = GetComponent<Minimax>();
    }

    /// <summary>
    /// Creates both players
    /// </summary>
    private void CreatePlayers()
    {
        whitePlayer = new Player(Colour.WHITE, board);
        blackPlayer = new Player(Colour.BLACK, board);

        whitePlayer.name = "White";
        blackPlayer.name = "Black";
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
        ResetText();
        uiManager.HideUI();
        camera.SetMainCamera();
        SetGameState(GameState.INIT);
        board.SetDependancies(this);
        if (board.isNetplay) restartButton.gameObject.SetActive(false);
        CreatePiecesFromLayout(initialLayout);
        activePlayer = whitePlayer;
        GenerateAllPossibleMoves(activePlayer);
        SetGameState(GameState.PLAY);
        SetStateText();

    }

    /// <summary>
    /// Initialize game based on passed fenstring
    /// </summary>
    /// <param name="boardLayout">The board layout ot use for loading</param>
    /// <param name="activePlayerFen">The FEN string to load</param>
    public void LoadGame(BoardLayout boardLayout, string activePlayerFen)
    {
        DestroyPieces();
        board.OnGameRestart();
        whitePlayer.OnGameRestart();
        blackPlayer.OnGameRestart();
        ResetText();
        uiManager.HideUI();
        SetGameState(GameState.INIT);
        CreatePiecesFromLayout(boardLayout);

        if (activePlayerFen == "w")
        {
            camera.SetMainCamera();
            activePlayer = whitePlayer;
        }
        else if (activePlayerFen == "b")
        {
            camera.SetMainCamera();
            activePlayer = blackPlayer;

            if (!(isAiGame || board.isNetplay)) camera.SwapCameras();

            if (isAiGame) minimax.MinimaxAI();
        }

        SetGameState(GameState.PLAY);
        SetStateText();
    }

    /// <summary>
    /// Restarts the game
    /// </summary>
    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestart();
        whitePlayer.OnGameRestart();
        blackPlayer.OnGameRestart();
        StartNewGame();
        pauseScreen.SetActive(false);
        halfmoveClock = 0;
        fullmoveNumber = 1;
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
        piece.SetData(squareCoordinates, colour, board);
        Material colourMaterial = pieceCreator.GetColourMaterial(colour);
        Material selectedMaterial = pieceCreator.GetSelectedMat();
        piece.SetMaterial(colourMaterial);
        piece.RotatePieceIfBlack(piece);
        board.SetPieceOnBoard(squareCoordinates, piece);
        Player currentPlayer = colour == Colour.WHITE ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(piece);
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
    /// Ends the current turn
    /// </summary>
    public void EndTurn()
    {
        if (isAiGame) aiSaveState.SaveGameToJson();
        if (isAiGame) aiSaveState.LoadGameFromJson();
        GenerateAllPossibleMoves(activePlayer);
        GenerateAllPossibleMoves(GetOppenentToPlayer(activePlayer));

        if (CheckGameFinished())
        {
            restartButton.gameObject.SetActive(true);
            EndGame();
        }
        else
        {
            ChangeActivePlayer();
            CheckStalemate();
            //CheckStalemate();
            //GenerateAllPossibleMoves(activePlayer);
            if (isAiGame && gameState == GameState.PLAY) minimax.MinimaxAI();
        }
        SetStateText();
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
        halfmoveClock = 0;
        fullmoveNumber = 1;
        uiManager.OnGameFinished(activePlayer.colour.ToString());
        SetGameState(GameState.FINISHED);
    }

    private void Forfeit()
    {
        Colour winner;
        switch (Client.Instance.player)
        {
            case 1:
                winner = Colour.BLACK;
                break;
            default:
                winner = Colour.WHITE;
                break;
        }
        uiManager.OnGameFinished(winner.ToString());
        SetGameState(GameState.FINISHED);


    }
    /// <summary>
    /// Checks if the game is ending based on the most recent action taken.
    /// This is the function used to check for Checkmate
    /// </summary>
    private bool CheckGameFinished()
    {
        Piece[] piecesAttackingKing = activePlayer.GetAttackingPieceOfType<King>();
        if (piecesAttackingKing.Length > 0)
        {
            Player opponent = GetOppenentToPlayer(activePlayer);
            Piece defendingKing = opponent.GetPiecesOfType<King>().FirstOrDefault();
            opponent.inCheck = true;
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
        else
        {
            GetOppenentToPlayer(activePlayer).inCheck = false;
        }
        return false;
    }

    /// <summary>
    /// Get the opponent of the player passed 
    /// </summary>
    /// <param name="player">Player to check opponent of</param>
    /// <returns>Opponent</returns>
    private Player GetOppenentToPlayer(Player player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    /// <summary>
    /// Changes the active player
    /// </summary>
    private void ChangeActivePlayer()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
        if (!(isAiGame || board.isNetplay)) camera.SwapCameras();
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

    /// <summary>
    /// Called once per frame.
    /// Checks for pause screen input 
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyUp("escape"))
        {
            if (pauseScreen.activeSelf == false)
            {
                pauseScreen.SetActive(true);
            }
            else
            {
                pauseScreen.SetActive(false);
            }
        }

        if (chatbarContainer == null || chatbarContainer.activeSelf != true)
        {
            if (Input.GetKeyUp(KeyCode.H))
            {
                if (pauseScreen.activeSelf == false)
                {
                    if (historyScreen.activeSelf == false)
                    {
                        historyScreen.SetActive(true);
                    }
                    else 
                    {
                        historyScreen.SetActive(false);
                    }
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            if (!chatbarContainer.activeInHierarchy)
            {
                chatbarContainer.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Exit the game and return to main menu
    /// </summary>
    public void ReturnToTitleScreen()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void ConcedeButton()
    {
        if (Client.Instance.player == -1)
            ReturnToTitleScreen();
        else
        {
            Client.Instance.ToServer(new NetForfeit());
            conceeded = true;   
        }
    }


    /// <summary>
    /// Resets any text  on screen
    /// </summary>
    public void ResetText()
    {
        TurnAction.globalTurnNum = 1.0f;
        board.ClearTurnText();
    }



    public void CheckStalemate()
    {
        int activePlayerMoves = 0;
        Piece defendingKing = activePlayer.GetPiecesOfType<King>().FirstOrDefault();
        activePlayer.RemoveMovesAllowingAttackOnPiece<King>(GetOppenentToPlayer(activePlayer), defendingKing);
        
        // If no moves available but not in checkmate
        foreach (var piece in activePlayer.activePieces)
        {
            piece.SelectAvailableSquares();
            RemoveMovesAllowingAttackOnPieceType<King>(piece);
            activePlayerMoves += piece.availableMoves.Count;
        }

        if (activePlayerMoves == 0)
        {
            EndInStalemate();
        }

        if (activePlayer.activePieces.Count == 1 && activePlayer.activePieces[0].type == PieceType.King)
        {
            //  If only two kings remain
            if (GetOppenentToPlayer(activePlayer).activePieces.Count == 1 && activePlayer.activePieces[0].type == PieceType.King)
            {
                EndInStalemate();
            }
        }

        if (halfmoveClock == 100)
        {
            EndInStalemate();
        }
    }

    public void EndInStalemate()
    {
        SetGameState(GameState.FINISHED);
        uiManager.OnGameStalemate();
    }

    public void Rematch()
    {
        Client.Instance.ToServer(new NetRematch());
    }

    private void Register()
    {
        NetUtil.CL_REMATCH += ClRematch;
        NetUtil.SV_REMATCH += ServRematch;
        NetUtil.CL_FORFEIT += ClForfeit;
        NetUtil.SV_FORFEIT += ServForfeit;
        NetUtil.CL_TERMINATE += ClTerm;
        NetUtil.SV_TERMINATE += ServTerm;
    }

    private void Unregister()
    {
        NetUtil.CL_REMATCH -= ClRematch;
        NetUtil.SV_REMATCH -= ServRematch;
        NetUtil.CL_FORFEIT -= ClForfeit;
        NetUtil.SV_FORFEIT -= ServForfeit;
        NetUtil.CL_TERMINATE -= ClTerm;
        NetUtil.SV_TERMINATE -= ServTerm;
    }
    private void ClRematch(NetMessage msg)
    {
        RestartGame();
    }
    private void ServRematch(NetMessage msg, NetworkConnection conn)
    {
        NetRematch rematch = msg as NetRematch;
        Server.Instance.Broadcast(rematch);
    }

    private void ClForfeit(NetMessage msg)
    {
        if(!conceeded) Forfeit();
        Client.Instance.ToServer(new NetTerminate());
    }

    private void ServForfeit(NetMessage msg, NetworkConnection conn)
    {
        NetForfeit forfeit = msg as NetForfeit;
        Server.Instance.Broadcast(forfeit);
    }

    private void ClTerm(NetMessage msg)
    {
        if (conceeded) ReturnToTitleScreen();

    }
    
    private void ServTerm(NetMessage msg, NetworkConnection conn)
    {
        NetTerminate term = msg as NetTerminate;
        Server.Instance.Broadcast(term);
    }



    public void UpdateHalfmoveClock(Piece selectedPiece, Vector2Int coordinates)
    {
        // If the move is a capture or a pawn moving
        if (selectedPiece.type == PieceType.Pawn || board.GetPieceOnSquare(coordinates) != null)
        {
            // Reset the halfmove clock
            halfmoveClock = 0;
        }
        else
        {
            // Increment the halfmove clock
            halfmoveClock++;
        }
    }
}
