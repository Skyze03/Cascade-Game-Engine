using System;

[Serializable]
public struct Position
{
    public int row;
    public int col;

    public Position(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public override string ToString()
    {
        return $"({row}, {col})";
    }
}