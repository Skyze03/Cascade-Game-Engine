using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private GameUI gameUI;

    private GameState gameState;
    private Position? selectedCell = null;
    private PlayMode currentPlayMode = PlayMode.MoveEat;

    public GamePhase CurrentPhase => gameState.phase;
    public PlayerColor CurrentPlayer => gameState.currentPlayer;
    public PlayMode CurrentPlayMode => currentPlayMode;

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

        if (gameUI == null)
        {
            gameUI = FindObjectOfType<GameUI>();
        }

        gameState = new GameState();

        boardRenderer.BuildBoard();
        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);

        RefreshUI();
        PrintTurnInfo();
    }

    public void SetPlayModeToMoveEat()
    {
        currentPlayMode = PlayMode.MoveEat;
        Debug.Log("Play mode set to Move/Eat");
        RefreshUI();
    }

    public void SetPlayModeToCascade()
    {
        currentPlayMode = PlayMode.Cascade;
        Debug.Log("Play mode set to Cascade");
        RefreshUI();
    }

    public void HandleCellClicked(Position pos)
    {
        Debug.Log($"Clicked cell: {pos}");

        if (gameState.phase == GamePhase.Placement)
        {
            HandlePlacementClick(pos);
            return;
        }

        if (gameState.phase == GamePhase.Play)
        {
            if (currentPlayMode == PlayMode.MoveEat)
            {
                HandleMoveEatClick(pos);
                return;
            }

            if (currentPlayMode == PlayMode.Cascade)
            {
                HandleCascadeClick(pos);
                return;
            }
        }
    }

    private void HandlePlacementClick(Position pos)
    {
        selectedCell = pos;
        boardRenderer.SetSelectedCell(selectedCell);

        bool success = RuleEngine.TryPlace(gameState, pos);

        if (!success)
        {
            Debug.Log("Invalid PLACE action.");
            return;
        }

        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);

        if (gameState.phase == GamePhase.Play)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
            Debug.Log("Placement phase complete. Entering Play phase.");
        }

        RefreshUI();
        PrintTurnInfo();
    }

    private void HandleMoveEatClick(Position pos)
    {
        StackData clickedStack = gameState.board.GetStack(pos);

        if (selectedCell == null)
        {
            if (clickedStack == null)
            {
                Debug.Log("No stack selected. Click one of your own stacks first.");
                return;
            }

            if (clickedStack.owner != gameState.currentPlayer)
            {
                Debug.Log("You must select one of your own stacks.");
                return;
            }

            selectedCell = pos;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();

            Debug.Log($"Selected stack at {pos}");
            return;
        }

        Position from = selectedCell.Value;

        if (from.row == pos.row && from.col == pos.col)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log("Selection cleared.");
            return;
        }

        // 点击另一个己方 stack：
        // 相邻 -> 尝试 MOVE merge
        // 不相邻 -> 改选
        if (clickedStack != null && clickedStack.owner == gameState.currentPlayer)
        {
            if (!RuleEngine.AreAdjacent(from, pos))
            {
                selectedCell = pos;
                boardRenderer.SetSelectedCell(selectedCell);
                RefreshUI();
                Debug.Log($"Selection changed to {pos}");
                return;
            }

            bool mergeSuccess = RuleEngine.TryMove(gameState, from, pos);

            if (!mergeSuccess)
            {
                Debug.Log("Invalid MOVE action.");
                return;
            }

            Debug.Log($"MOVE merge succeeded: {from} -> {pos}");

            selectedCell = null;
            boardRenderer.Render(gameState.board);
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            PrintTurnInfo();
            return;
        }

        // 先只做 MOVE；敌方格后面再接 EAT
        bool success = RuleEngine.TryMove(gameState, from, pos);

        if (!success)
        {
            Debug.Log("Invalid MOVE action.");
            return;
        }

        Debug.Log($"MOVE succeeded: {from} -> {pos}");

        selectedCell = null;
        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);
        RefreshUI();
        PrintTurnInfo();
    }

    private void HandleCascadeClick(Position pos)
    {
        StackData clickedStack = gameState.board.GetStack(pos);

        if (selectedCell == null)
        {
            if (clickedStack == null)
            {
                Debug.Log("Cascade mode: click one of your own stacks first.");
                return;
            }

            if (clickedStack.owner != gameState.currentPlayer)
            {
                Debug.Log("Cascade mode: you must select one of your own stacks.");
                return;
            }

            selectedCell = pos;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log($"Cascade source selected at {pos}");
            return;
        }

        Position from = selectedCell.Value;

        if (from.row == pos.row && from.col == pos.col)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log("Selection cleared.");
            return;
        }

        // 暂时先只搭输入框架，不执行真正 Cascade
        if (!RuleEngine.AreAdjacent(from, pos))
        {
            Debug.Log("Cascade direction must be chosen by clicking an adjacent cell.");
            return;
        }

        Debug.Log($"Cascade mode placeholder: source {from}, direction target {pos}");
    }

    private void RefreshUI()
    {
        if (gameUI != null)
        {
            gameUI.Refresh(this, selectedCell);
        }
    }

    private void PrintTurnInfo()
    {
        Debug.Log($"Phase = {gameState.phase}, Current Player = {gameState.currentPlayer}, Placement Turns = {gameState.placementTurnsTaken}, Play Turns = {gameState.playTurnsTaken}, Mode = {currentPlayMode}");
    }
}