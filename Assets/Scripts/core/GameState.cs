using System.Collections.Generic;

public class GameState
{
    public BoardState board = new BoardState();
    public PlayerColor currentPlayer = PlayerColor.Red;
    public GamePhase phase = GamePhase.Placement;

    public int placementTurnsTaken = 0;
    public int playTurnsTaken = 0;

    public GameResult result = GameResult.Ongoing;

    // 用来记录三次重复
    public Dictionary<string, int> repetitionCounts = new Dictionary<string, int>();
}