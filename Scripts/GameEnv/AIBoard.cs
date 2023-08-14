 using System;
 using System.Collections;
 using System.Collections.Generic;
 using System.Linq;
 using System.IO;
 using UnityEngine;



 public class AIBoard
{
    public int BOARD_SIZE = 8;
    public AIController? aiController;
    public SimulationPiece[,] grid;
    public char[][] charGrid;
    public List<LegalMove> legalMoves;
    public Player whitePlayer;
    public Player blackPlayer;
    public Player activePlayer;

    public List<string> boardStateHistory;


    public class SimulationPiece
    {
        public Vector2Int occupiedSquare { get; set; }
        public Colour colour { get; set; }
        public PieceType type { get; set; }
        public PieceValue value { get; set; }
        public List<Vector2Int> availableMoves = new List<Vector2Int>();
        
        public SimulationPiece(Piece piece) 
        {
            occupiedSquare = piece.occupiedSquare;
            colour = piece.colour;
            type = piece.type;
            value = piece.pieceValue;
            availableMoves = new List<Vector2Int>(piece.availableMoves);
        }
        public SimulationPiece()
        {

        }
    }

    public class LegalMove
    {
        public Colour pieceColour;
        public PieceType pieceType;
        public Vector2Int startingPos;
        public Vector2Int endingPos;
        public int moveScore;

        public LegalMove(Vector2Int startPos, Vector2Int move)
        {
            startingPos = startPos;
            endingPos = move;
            moveScore = 0;
        }


    }
    public AIBoard(Board board)
    {
        // grid = board.grid;
        aiController = board.aiController;
        blackPlayer = aiController.blackPlayer;
        whitePlayer = aiController.whitePlayer;
        aiController.GenerateAllPossibleMoves(blackPlayer);
        aiController.GenerateAllPossibleMoves(whitePlayer);
        
        // charGrid = AIBoardCharGridGen(aiController.aiSaveState.GetCurrentFenFromSave());
        grid = GenerateGridFromPieces();
        legalMoves = GenerateSimulationLegalMoves();

        
    }

    public AIBoard(AIBoard previousBoard)
    {
        grid = previousBoard.grid;
        aiController = previousBoard.aiController;
        blackPlayer = previousBoard.blackPlayer;
        whitePlayer = previousBoard.whitePlayer;
        boardStateHistory = previousBoard.boardStateHistory;
        legalMoves = GenerateSimulationLegalMoves();

        PrintAllPieceLocations();

    }

    public SimulationPiece[,] GenerateGridFromPieces()
    {
        SimulationPiece[,] grid = new SimulationPiece[BOARD_SIZE, BOARD_SIZE];

        foreach (Piece piece in whitePlayer.activePieces)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = new SimulationPiece(piece);
            Debug.Log("Piece: " + piece.colour + " " + piece.type + " at " + piece.occupiedSquare);

        }
        foreach (Piece piece in blackPlayer.activePieces)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = new SimulationPiece(piece);
            Debug.Log("Piece: " + piece.colour + " " + piece.type + " at " + piece.occupiedSquare);
        }
        return grid;

    }

    public char[][] AIBoardCharGridGen(string fen)
    {
        char[][] charGrid = new char[BOARD_SIZE][];
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            charGrid[i] = new char[BOARD_SIZE];
        }

        int rank = 0;
        int file = 0;
        foreach (char c in fen)
        {
            if (c == ' ')
            {
                break;
            }
            else if (c == '/')
            {
                rank++;
                file = 0;
            }
            else if (char.IsDigit(c))
            {
                file += int.Parse(c.ToString());
            }
            else
            {
                charGrid[rank][file] = c;
                file++;
            }
        }

        return charGrid;
    }       

    public void PrintChar()
    {
        for (int i = BOARD_SIZE -1; i >= 0; i--)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                Debug.Log(charGrid[i][j] + " ");
            }
        }
    } 

    public void PrintAllPieceLocations()
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (GetPieceOnSquare(new Vector2Int(i,j)) != null)
                {
                    SimulationPiece piece = grid[i,j];
                    Debug.Log(piece.occupiedSquare + " can move to " + piece.availableMoves.Count + " positions" );

                }
            }
        }

    }
    public List<LegalMove> GenerateSimulationLegalMoves()
    {
        List<LegalMove> allLegalMoves = new List<LegalMove>();

        foreach (Piece piece in whitePlayer.activePieces)
        {
            SimulationPiece newSimPiece = new SimulationPiece(piece);

            foreach (var move in piece.availableMoves)
            {

                LegalMove newMove = new LegalMove(piece.occupiedSquare, move);
                Debug.Log("Adding white move: " + newMove.startingPos + " -> " + newMove.endingPos);
                newSimPiece.availableMoves.Add(move);
                allLegalMoves.Add(newMove);
            }
        }

        foreach (Piece piece in blackPlayer.activePieces)
        {
            SimulationPiece newSimPiece = new SimulationPiece(piece);

            foreach (var move in piece.availableMoves)
            {
                LegalMove newMove = new LegalMove(piece.occupiedSquare, move);
                Debug.Log("Adding black move: " + newMove.startingPos + " -> " + newMove.endingPos);
                newSimPiece.availableMoves.Add(move);

                allLegalMoves.Add(newMove);
            }
        }

        Debug.Log("Number of total moves available on simulation board: " + allLegalMoves.Count);

        return allLegalMoves;
    }

        public int EvaluateBoard(Player player)
        {
            int score = 0;
            foreach (var piece in grid)
            {
                if (piece.colour == player.colour)
                {
                    score += (int)piece.value;
                }
                else 
                {
                    score -= (int)piece.value;
                }
                
            }
            return score;
        }

    public SimulationPiece GetPieceOnSquare(Vector2Int coordinates)
    {
        if (grid[coordinates.x, coordinates.y] != null)
        {

            Debug.Log( "Found " + grid[coordinates.x, coordinates.y].type + " on square " + coordinates);
            Debug.Log("Found it");

            SimulationPiece piece = grid[coordinates.x, coordinates.y];
            return piece;

        }
        Debug.Log("No piece found at location: " + coordinates);
        return null;


    }

    public void TakePiece(SimulationPiece piece)
    {
        if (piece != null)
        {
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            OnPieceRemoved(piece);

        }
    }

    public void OnPieceRemoved(SimulationPiece piece)
    {
        Debug.Log(piece.colour);
        Player piecePlayer = (piece.colour == Colour.WHITE) ? whitePlayer : blackPlayer;
        Piece pieceToRemove = piecePlayer.activePieces.Where(i => i.occupiedSquare == piece.occupiedSquare).FirstOrDefault();
        piecePlayer.RemovePiece(pieceToRemove);
    }

        /// <summary>
    /// Called when a piece is moved
    /// </summary>
    /// <param name="coordinates">Where the piece is moving</param>
    /// <param name="piece">The piece being moved</param>
    public void OnSelectedPieceMoved(  SimulationPiece piece, Vector2Int targetPos)
    {
        Debug.Log(piece);

        TakeOpponentPieceAttempt(targetPos);
        Debug.Log(piece.occupiedSquare + " debugging");
        UpdateBoardOnPieceMoved(targetPos, piece.occupiedSquare, piece, null);
        MovePiece(piece, targetPos);
        //if(!isAIBoard) EndTurn();

    }

    /// <summary>
    /// Take opponent's piece if it exists
    /// </summary>
    /// <param name="coordinates">Coordinates to check</param>
    private void TakeOpponentPieceAttempt(Vector2Int coordinates)
    {
        SimulationPiece piece = GetPieceOnSquare(coordinates);
        TakePiece(piece);

        // if (piece != null && piece.colour != )
        // {
        //     TakePiece(piece);
        // }
    }

    public void UpdateBoardOnPieceMoved(Vector2Int newCoordinates, Vector2Int oldCoordinates, SimulationPiece newPiece, SimulationPiece oldPiece)
    {
        grid[oldCoordinates.x, oldCoordinates.y] = oldPiece;
        grid[newCoordinates.x, newCoordinates.y] = newPiece;
    }

    public void MovePiece(SimulationPiece piece, Vector2Int endPos)
    {
        piece.occupiedSquare = endPos;
    }

    public LegalMove SimulateMove(Vector2Int startPos, Vector2Int targetPos)
    {
        SimulationPiece piece = GetPieceOnSquare(startPos);
        LegalMove simulatedMove = new LegalMove(startPos, targetPos);
        Player pieceOwner = simulatedMove.pieceColour == Colour.WHITE ? aiController.whitePlayer : aiController.blackPlayer;
        pieceOwner.CalculateCurrentPlayerScore();
        simulatedMove.moveScore = pieceOwner.playerScore;
        SimulationPiece pieceOnTarget = GetPieceOnSquare(targetPos);
        Player opponent = aiController.GetOppenentToPlayer(pieceOwner);

        if (pieceOnTarget != null) 
        {
            simulatedMove.moveScore += (int)pieceOnTarget.value;
        }
        OnSelectedPieceMoved( piece, targetPos);
        simulatedMove.moveScore += EvaluateBoard( pieceOwner );
        return simulatedMove;
    }

    

























}
