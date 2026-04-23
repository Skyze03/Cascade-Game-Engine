using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BoardRenderer boardRenderer;

    private GameState gameState;
    private Position? selectedCell = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (boardRenderer == null)
        {
            boardRenderer = FindObjectOfType<BoardRenderer>();
        }

        gameState = new GameState();

        boardRenderer.BuildBoard();
        boardRenderer.Render(gameState.board);

        Debug.Log("Game started. Phase = Placement. Current player = Red");
    }

    public void HandleCellClicked(Position pos)
    {
        Debug.Log($"Clicked cell: {pos}");

        selectedCell = pos;

        boardRenderer.SetSelectedCell(selectedCell);

        if (gameState.phase == GamePhase.Placement)
        {
            HandlePlacementClick(pos);
        }
    }

    private void HandlePlacementClick(Position pos)
    {
        bool success = RuleEngine.TryPlace(gameState, pos);

        if (!success)
        {
            Debug.Log("Invalid PLACE action.");
            return;
        }

        Debug.Log($"Placed {GetCurrentPlayerNameBeforeSwitch()} stack at {pos}");

        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);

        if (gameState.phase == GamePhase.Play)
        {
            Debug.Log("Placement phase complete. Entering Play phase.");
        }
        else
        {
            Debug.Log($"Next player = {gameState.currentPlayer}");
        }
    }

    private string GetCurrentPlayerNameBeforeSwitch()
    {
        // 这个函数现在其实只会在日志里看起来有点奇怪，
        // 因为 TryPlace 成功后 currentPlayer 已经切换了，
        // 所以这里只是临时帮助阅读。
        return gameState.currentPlayer == PlayerColor.Red ? "Blue" : "Red";
    }
}