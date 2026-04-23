using UnityEngine;

public static class RuleEngine
{
    public static bool TryPlace(GameState state, Position pos)
    {
        if (!IsValidPlace(state, pos))
            return false;

        state.board.SetStack(pos, new StackData(state.currentPlayer, 3));
        state.placementTurnsTaken++;

        if (state.placementTurnsTaken >= 8)
        {
            state.phase = GamePhase.Play;
        }

        SwitchPlayer(state);

        return true;
    }

    public static bool IsValidPlace(GameState state, Position pos)
    {
        if (state == null) return false;
        if (state.board == null) return false;

        if (state.phase != GamePhase.Placement)
        {
            Debug.Log("IsValidPlace failed: game is not in Placement phase.");
            return false;
        }

        if (!state.board.InBounds(pos))
        {
            Debug.Log("IsValidPlace failed: out of bounds.");
            return false;
        }

        if (!state.board.IsEmpty(pos))
        {
            Debug.Log("IsValidPlace failed: target cell is not empty.");
            return false;
        }

        // 第一手例外：整个游戏的第一步不受相邻限制
        if (state.placementTurnsTaken == 0)
        {
            return true;
        }

        // 从第二手开始：不能放在与“对手已有 stack”相邻的位置
        PlayerColor opponent = GetOpponent(state.currentPlayer);

        Position[] neighbors = new Position[]
        {
            new Position(pos.row - 1, pos.col), // up
            new Position(pos.row + 1, pos.col), // down
            new Position(pos.row, pos.col - 1), // left
            new Position(pos.row, pos.col + 1), // right
        };

        foreach (Position neighbor in neighbors)
        {
            if (!state.board.InBounds(neighbor))
                continue;

            StackData stack = state.board.GetStack(neighbor);
            if (stack != null && stack.owner == opponent)
            {
                Debug.Log("IsValidPlace failed: adjacent to opponent stack.");
                return false;
            }
        }

        return true;
    }

    public static PlayerColor GetOpponent(PlayerColor player)
    {
        return player == PlayerColor.Red ? PlayerColor.Blue : PlayerColor.Red;
    }

    public static void SwitchPlayer(GameState state)
    {
        state.currentPlayer = GetOpponent(state.currentPlayer);
    }
}