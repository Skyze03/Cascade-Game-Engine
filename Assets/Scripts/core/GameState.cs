public class GameState
{
    public BoardState board = new BoardState();
    public PlayerColor currentPlayer = PlayerColor.Red;
    public GamePhase phase = GamePhase.Placement;

    public int placementTurnsTaken = 0;
    public int playTurnsTaken = 0;
}