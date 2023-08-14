using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

public class Minimax : MonoBehaviour
{
    public static int movesEvaled = 0; 
    public struct TestMove
    {
        public int pieceMovedIndex;
        public Vector2Int occupiedSquare;
        public Vector2Int newLocation;
        public int playerScore;
        public string? pieceTakenName;
        public int movingPieceValue;
    }

    [SerializeField] private Player moveablePlayer;
    [SerializeField] private AIController aiController;
    [SerializeField] private GameController gameController;

    [SerializeField] private Board board;
    [SerializeField] private Board aiBoard;
    public bool movePiece = true;


    static List<TestMove> testedMoves = new List<TestMove>();

    private const int MAX_SEARCH_DEPTH = 3;

    public void MinimaxAI()
    {
        if (gameController.activePlayer == gameController.blackPlayer) 
        {
            aiController.activePlayer = aiController.blackPlayer;
            board.turnPrintText.text = "CPU thinking...";
            StartCoroutine(MinimaxThinkingDelay());
        }
    }

    IEnumerator MinimaxThinkingDelay()
    {
        yield return new WaitForSeconds(0.5f);


        // Create a new thread to process the minimax move
        Thread thread = new Thread(() =>
        {
            ProcessMinimaxMove();
        });

        // Start the thread
        thread.Start();

        // Wait for the thread to finish
        while (thread.IsAlive)
        {
            yield return null;
        }
    }

    private TestMove SimulateMove(Piece piece, Vector2Int coordinates, int pieceIndex)
    {
        movesEvaled++;

        Player pieceOwner = piece.colour == Colour.WHITE ? aiController.whitePlayer : aiController.blackPlayer;
        TestMove simulatedMove = new TestMove {playerScore = 0};
        pieceOwner.CalculateCurrentPlayerScore();
        int playerScore = pieceOwner.playerScore;
        Piece pieceOnTarget = aiBoard.GetPieceOnSquare(coordinates);
        Player opponent = aiController.GetOppenentToPlayer(pieceOwner);

        if (pieceOnTarget != null) 
        {
            playerScore += (int)pieceOnTarget.pieceValue;
            simulatedMove.pieceTakenName = pieceOnTarget.type.ToString() + " on " + pieceOnTarget.occupiedSquare;
            UnityMainThreadDispatcher.Instance().Enqueue(AITakePiece(pieceOnTarget, opponent));
        }

        simulatedMove.newLocation = coordinates;
        simulatedMove.occupiedSquare = piece.occupiedSquare;
        simulatedMove.pieceMovedIndex = pieceIndex;
        simulatedMove.movingPieceValue = (int)piece.GetPieceValue();
        simulatedMove.playerScore += playerScore;
        simulatedMove.playerScore += EvaluateBoard(aiBoard, pieceOwner );
        simulatedMove.playerScore += piece.GetBestPositionsOnBoardScore(coordinates.x, coordinates.y);

        return simulatedMove;
    }

    private List<TestMove> ShuffleMoveList(List<TestMove> moveList)
    {
        var newList = moveList.OrderBy(a => Guid.NewGuid()).ToList();    
        return newList;
    }

    private IEnumerator ExecuteMiniMaxMove(TestMove moveToExecute)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(LoadAIBoard());
        Piece pieceMoved = board.GetPieceOnSquare(moveToExecute.occupiedSquare);
        board.SelectPiece(pieceMoved);
        board.OnSelectedPieceMoved(moveToExecute.newLocation, pieceMoved);
        //aiController.ChangeAIActivePlayer();
        yield return null;
    }
    private IEnumerator AITakePiece(Piece piece, Player player)
    {
        aiBoard.grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
        RemoveAIPieceFromBoard(piece);
        

        yield return null;
    }

    public void RemoveAIPieceFromBoard(Piece piece)
    {
        piece.gameObject.transform.position = new Vector3(piece.gameObject.transform.position.x, piece.gameObject.transform.position.y - 10f, piece.gameObject.transform.position.z - 10f);
    }

    private IEnumerator LoadAIBoard()
    {
        aiController.aiSaveState.LoadGameFromJson();
        aiController.blackPlayer.CalculateCurrentPlayerScore();
        aiController.whitePlayer.CalculateCurrentPlayerScore();
        yield return null;
    }
    
    private void ProcessMinimaxMove()
    {
        testedMoves.Clear();

        movesEvaled = 0;

        aiController.GenerateAllPossibleMoves(aiController.activePlayer);

        List<TestMove> bestMoves = new List<TestMove>();

        int bestScore = int.MinValue;

        // Find all the pieces for the black player (human will be white player)
        if (gameController.IsColourTurnActive(Colour.BLACK))
        {
            List<Piece> blackPlayerPieces = aiController.blackPlayer.activePieces;
            if (!gameController.activePlayer.inCheck)
            {
                List<TestMove> bestCapture = GetBestCapture(aiController.activePlayer);
                if (bestCapture.Count > 0)
                {
                    if (aiBoard.CoordinatesOnBoard(bestCapture[0].occupiedSquare))
                    {
                        UnityMainThreadDispatcher.Instance().Enqueue(ExecuteMiniMaxMove(bestCapture[0]));
                        return;
                    }
                }
            }

            movePiece = true;
            while (movePiece)
            {
                for (int i = 0; i < blackPlayerPieces.Count; i++)
                {
                    //blackPlayerPieces = aiController.blackPlayer.activePieces;

                    Piece piece = blackPlayerPieces[i];

                    aiController.RemoveMovesAllowingAttackOnPieceType<King>(piece);

                    if (piece.availableMoves.Count > 0)
                    {
                        for (int x = 0; x < piece.availableMoves.Count; x++)
                        {

                            TestMove moveToTest = SimulateMove(piece, piece.availableMoves[x], blackPlayerPieces.IndexOf(piece));

                            int score = MinimaxSearch(aiBoard, MAX_SEARCH_DEPTH, int.MinValue, int.MaxValue, false);

                            if (moveToTest.playerScore >= bestScore)
                            {
                                bestScore = moveToTest.playerScore;
                                testedMoves.Add(moveToTest);
                            }

                            if (score >= bestScore)
                            {
                                bestScore = score;
                                testedMoves.Add(moveToTest);
                            }
                        }
                    }
                }

                foreach (TestMove move in testedMoves)
                {
                    if (move.playerScore == bestScore)
                    {
                        bestMoves.Add(move);
                    }
                }
                bestMoves = ShuffleMoveList(bestMoves);
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteMiniMaxMove(bestMoves[0]));
                movePiece = false;
            }
        }
    }

    private int MinimaxSearch(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        //aiController.ChangeAIActivePlayer();
        if (depth == 0 || aiController.CheckGameFinished())
        {
            aiController.activePlayer = aiController.blackPlayer;
            return EvaluateBoard(board, aiController.activePlayer);
        }
        if (maximizingPlayer)
        {
            int bestScore = int.MinValue;
            foreach (Piece piece in aiController.activePlayer.activePieces.ToList())
            {
                foreach (Vector2Int move in piece.availableMoves.ToList())
                {
                    TestMove testMove = SimulateMove(piece, move, aiController.activePlayer.activePieces.IndexOf(piece));
                    int score = MinimaxSearch(aiBoard, depth - 1, alpha, beta, false);
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
            return bestScore;
        }
        else
        {
            int bestScore = int.MaxValue;
            foreach (Piece piece in aiController.GetOppenentToPlayer(aiController.activePlayer).activePieces.ToList())
            {
                foreach (Vector2Int move in piece.availableMoves.ToList())
                {
                    TestMove testMove = SimulateMove(piece, move, aiController.GetOppenentToPlayer(aiController.activePlayer).activePieces.IndexOf(piece));
                    int score = MinimaxSearch(aiBoard, depth - 1, alpha, beta, true);
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);

                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }
            return bestScore;
        }
    }
    private int EvaluateBoard(Board board, Player player)
    {
        int score = 0;
        foreach (Piece piece in board.grid)
        {
            if (piece != null)
            {
                if (piece.colour == player.colour)
                {
                    score += (int)piece.GetPieceValue();
                }
                else 
                {
                    score -= (int)piece.GetPieceValue();
                }
            }
        }


        return score;
    }

    private List<TestMove> GatherPawnCaptureMoves(List<TestMove> moves)
    {
        List<TestMove> captureMoves = new List<TestMove>();

        foreach(TestMove move in moves)
        {
            Piece movingPiece = aiBoard.GetPieceOnSquare(move.occupiedSquare);
            if (movingPiece.type == PieceType.Pawn)
            {
                captureMoves.Add(move);
            }
        }

        return captureMoves;
    }

    private List<TestMove> MostValuableVictims(List<TestMove> moves)
    {
        List<TestMove> captureMoves = new List<TestMove>();
        List<TestMove> filteredMoves = new List<TestMove>();

        int highestScoreCaptured = int.MinValue;


        foreach (var move in moves)
        {
            if (move.playerScore >= highestScoreCaptured)
            {
                highestScoreCaptured = move.playerScore;
                captureMoves.Insert(0, move);
            }
        }

        for (int i = 0; i < captureMoves.Count; i++)
        {
            if (captureMoves.Count == 1 || i == captureMoves.Count - 1)
            {
                return captureMoves;
            }
            
            else filteredMoves = captureMoves.Where(m => m.playerScore == (captureMoves.Max(n => n.playerScore))).ToList();
        }


        return captureMoves;
    }

    private List<TestMove> GatherNonPawnCaptureMoves(List<TestMove> moves)
    {
        List<TestMove> captureMoves = new List<TestMove>();

        foreach(TestMove move in moves)
        {
            Piece movingPiece = aiBoard.GetPieceOnSquare(move.occupiedSquare);
            if (movingPiece.type != PieceType.Pawn)
            {
                captureMoves.Add(move);
            }
        }

        return captureMoves;

    }

    private List<TestMove> OrderAttackersByLeastValuable(List<TestMove> moves)
    {
        if (moves.Count > 1)
        {
            List<TestMove> orderedList = moves.OrderBy(x =>  x.movingPieceValue).ToList();
            return orderedList;
        }
        return moves;
    }


    private List<TestMove> GetBestCapture(Player player)
    {
        Piece king = player.GetPiecesOfType<King>().FirstOrDefault();
        Piece queen = player.GetPiecesOfType<Queen>().FirstOrDefault();
        player.RemoveMovesAllowingAttackOnPiece<King>(aiController.GetOppenentToPlayer(player) , king);
        if(queen != null) 
        {

            player.RemoveMovesAllowingAttackOnPiece<Queen>(aiController.GetOppenentToPlayer(player) , queen);

        }

        TestMove bestCapture = new TestMove();
        bestCapture.occupiedSquare = new Vector2Int(-1, -1);

        List<TestMove> allCaptureMoves = new List<TestMove>();
        foreach(Piece piece in player.activePieces)
        {
            foreach (Vector2Int coordinate in piece.availableMoves)
            {
                Piece pieceOnTarget = aiBoard.GetPieceOnSquare(coordinate);
                if (pieceOnTarget != null)
                {
                    TestMove captureMove = SimulateMove(piece, coordinate, player.activePieces.IndexOf(piece));
                    allCaptureMoves.Add(captureMove);
                }
            }
        }


        if (allCaptureMoves.Count > 0)
        {
            List<TestMove> mostValuableCaptureMoves = MostValuableVictims(allCaptureMoves);
            bestCapture = mostValuableCaptureMoves[0];
            List<TestMove> pawnCaptureMoves = GatherPawnCaptureMoves(mostValuableCaptureMoves);
            List<TestMove> otherCaptureMoves = GatherNonPawnCaptureMoves(mostValuableCaptureMoves);
            List<TestMove> filteredCaptureMoves = new List<TestMove>();

            if (pawnCaptureMoves.Intersect<TestMove>(mostValuableCaptureMoves).Count<TestMove>() > 0)
            {
                List<TestMove> valuablePawnCaptures = pawnCaptureMoves.Intersect<TestMove>(mostValuableCaptureMoves).ToList();
                foreach (var capture in valuablePawnCaptures)
                {
                    filteredCaptureMoves.Add(capture);
                }
            }

            else if (otherCaptureMoves.Intersect<TestMove>(mostValuableCaptureMoves).Count<TestMove>() > 0)
            {
                List<TestMove> valuableCaptures = otherCaptureMoves.Intersect<TestMove>(mostValuableCaptureMoves).ToList();
                foreach (var capture in valuableCaptures)
                {
                    filteredCaptureMoves.Add(capture);
                }
            }    
            if (filteredCaptureMoves.Count > 1) 
            {
                filteredCaptureMoves =  OrderAttackersByLeastValuable(filteredCaptureMoves); 
            }
            return filteredCaptureMoves;
        }
        return new List<TestMove>();
    }
}