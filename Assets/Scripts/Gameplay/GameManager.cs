using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private BoardRenderer boardRenderer;
    [SerializeField] private GameUI gameUI;

    private GameState gameState;
    private Position? selectedCell = null;
    private PlayMode currentPlayMode = PlayMode.MoveEat;
    private bool initialPlayStateRegistered = false;
    public GamePhase CurrentPhase => gameState.phase;
    public PlayerColor CurrentPlayer => gameState.currentPlayer;
    public PlayMode CurrentPlayMode => currentPlayMode;

    public GameResult CurrentResult => gameState.result;

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

        boardRenderer.BuildBoard();
        StartNewGame();
    }

    public void StartNewGame()
    {
        gameState = new GameState();

        selectedCell = null;
        currentPlayMode = PlayMode.MoveEat;
        initialPlayStateRegistered = false;

        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);

        RefreshUI();
        PrintTurnInfo();

        Debug.Log("New game started.");
    }

    public void RestartGame()
    {
        Debug.Log("Restart button clicked.");
        StartNewGame();
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

        if (gameState.phase == GamePhase.GameOver)
        {
            Debug.Log("Game is over. No further actions allowed.");
            return;
        }

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

            RegisterInitialPlayStateIfNeeded();
        }

        RefreshUI();
        PrintTurnInfo();
    }

    private void HandleMoveEatClick(Position pos)
    {
        StackData clickedStack = gameState.board.GetStack(pos);

        // 第一次点击：必须先选自己的 stack
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

        // 点击自己当前选中的格子：取消选中
        if (from.row == pos.row && from.col == pos.col)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log("Selection cleared.");
            return;
        }

        // 不是相邻格，且点击的是另一个己方 stack -> 改选
        if (clickedStack != null && clickedStack.owner == gameState.currentPlayer && !RuleEngine.AreAdjacent(from, pos))
        {
            selectedCell = pos;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log($"Selection changed to {pos}");
            return;
        }

        // 必须是相邻格才能进行 Move/Eat
        if (!RuleEngine.AreAdjacent(from, pos))
        {
            Debug.Log("Target must be adjacent in Move/Eat mode.");
            return;
        }

        // 目标格为空：MOVE relocate
        if (clickedStack == null)
        {
            bool moveSuccess = RuleEngine.TryMove(gameState, from, pos);

            if (!moveSuccess)
            {
                Debug.Log("Invalid MOVE action.");
                return;
            }

            Debug.Log($"MOVE relocate succeeded: {from} -> {pos}");

            selectedCell = null;
            boardRenderer.Render(gameState.board);
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            EvaluateGameEndAfterSuccessfulPlayAction();
            PrintTurnInfo();
            return;
        }

        // 目标格是己方：MOVE merge
        if (clickedStack.owner == gameState.currentPlayer)
        {
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
            EvaluateGameEndAfterSuccessfulPlayAction();
            PrintTurnInfo();
            return;
        }

        // 目标格是敌方：尝试 EAT
        bool eatSuccess = RuleEngine.TryEat(gameState, from, pos);

        if (!eatSuccess)
        {
            Debug.Log("Invalid EAT action.");
            return;
        }

        Debug.Log($"EAT succeeded: {from} -> {pos}");

        selectedCell = null;
        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);
        RefreshUI();
        EvaluateGameEndAfterSuccessfulPlayAction();
        PrintTurnInfo();
    }

    private void HandleCascadeClick(Position pos)
    {
        StackData clickedStack = gameState.board.GetStack(pos);

        // 第一次点击：先选自己的 stack
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

        // 点自己：取消选中
        if (from.row == pos.row && from.col == pos.col)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
            RefreshUI();
            Debug.Log("Selection cleared.");
            return;
        }

        // Cascade 必须通过点相邻格来指定方向
        if (!RuleEngine.AreAdjacent(from, pos))
        {
            Debug.Log("Cascade direction must be chosen by clicking an adjacent cell.");
            return;
        }

        bool success = RuleEngine.TryCascade(gameState, from, pos);

        if (!success)
        {
            Debug.Log("Invalid CASCADE action.");
            return;
        }

        Debug.Log($"CASCADE succeeded: source {from}, direction target {pos}");

        selectedCell = null;
        boardRenderer.Render(gameState.board);
        boardRenderer.SetSelectedCell(selectedCell);
        RefreshUI();
        EvaluateGameEndAfterSuccessfulPlayAction();
        PrintTurnInfo();
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

    private void RegisterInitialPlayStateIfNeeded()
    {
        if (gameState.phase != GamePhase.Play)
            return;

        if (initialPlayStateRegistered)
            return;

        string stateKey = GameEndChecker.BuildStateKey(gameState);

        if (!gameState.repetitionCounts.ContainsKey(stateKey))
        {
            gameState.repetitionCounts[stateKey] = 0;
        }

        gameState.repetitionCounts[stateKey]++;
        initialPlayStateRegistered = true;

        Debug.Log("Initial play state registered for repetition tracking.");
    }

    private void EvaluateGameEndAfterSuccessfulPlayAction()
    {
        Debug.Log("EvaluateGameEndAfterSuccessfulPlayAction called.");

        GameEndChecker.EvaluateAfterPlayAction(gameState);

        Debug.Log($"After evaluation -> Phase: {gameState.phase}, Result: {gameState.result}");

        if (gameState.phase == GamePhase.GameOver)
        {
            selectedCell = null;
            boardRenderer.SetSelectedCell(selectedCell);
        }

        RefreshUI();

        if (gameState.phase == GamePhase.GameOver)
        {
            Debug.Log($"Game Over! Result = {gameState.result}");
        }
    }
}