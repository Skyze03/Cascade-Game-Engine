using UnityEngine;

public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private CellView cellPrefab;
    [SerializeField] private Transform boardRoot;
    [SerializeField] private float cellSpacing = 1.1f;

    private CellView[,] cellViews = new CellView[BoardState.SIZE, BoardState.SIZE];

    public void BuildBoard()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("BoardRenderer: cellPrefab is not assigned.");
            return;
        }

        if (boardRoot == null)
        {
            boardRoot = transform;
        }

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }

        for (int row = 0; row < BoardState.SIZE; row++)
        {
            for (int col = 0; col < BoardState.SIZE; col++)
            {
                CellView cell = Instantiate(cellPrefab, boardRoot);
                cell.transform.localPosition = new Vector3(col * cellSpacing, -row * cellSpacing, 0f);
                cell.Setup(row, col);
                cell.Refresh(null);
                cellViews[row, col] = cell;
            }
        }

        float boardWidth = (BoardState.SIZE - 1) * cellSpacing;
        float boardHeight = (BoardState.SIZE - 1) * cellSpacing;

        boardRoot.localPosition = new Vector3(-boardWidth / 2f, boardHeight / 2f, 0f);
    }

    public void Render(BoardState board)
    {
        if (board == null) return;

        for (int row = 0; row < BoardState.SIZE; row++)
        {
            for (int col = 0; col < BoardState.SIZE; col++)
            {
                cellViews[row, col].Refresh(board.cells[row, col]);
            }
        }
    }

    public void SetSelectedCell(Position? selected)
    {
        for (int row = 0; row < BoardState.SIZE; row++)
        {
            for (int col = 0; col < BoardState.SIZE; col++)
            {
                bool isSelected = selected.HasValue
                    && selected.Value.row == row
                    && selected.Value.col == col;

                cellViews[row, col].SetSelected(isSelected);
            }
        }
    }
}