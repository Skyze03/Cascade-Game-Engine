public class BoardState
{
    public const int SIZE = 8;

    public StackData[,] cells = new StackData[SIZE, SIZE];

    public bool InBounds(Position pos)
    {
        return pos.row >= 0 && pos.row < SIZE && pos.col >= 0 && pos.col < SIZE;
    }

    public StackData GetStack(Position pos)
    {
        if (!InBounds(pos)) return null;
        return cells[pos.row, pos.col];
    }

    public void SetStack(Position pos, StackData stack)
    {
        if (!InBounds(pos)) return;
        cells[pos.row, pos.col] = stack;
    }

    public bool IsEmpty(Position pos)
    {
        return GetStack(pos) == null;
    }
}