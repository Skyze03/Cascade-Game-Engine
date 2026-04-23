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
            return false;

        if (!state.board.InBounds(pos))
            return false;

        if (!state.board.IsEmpty(pos))
            return false;

        if (state.placementTurnsTaken == 0)
            return true;

        PlayerColor opponent = GetOpponent(state.currentPlayer);

        Position[] neighbors = new Position[]
        {
            new Position(pos.row - 1, pos.col),
            new Position(pos.row + 1, pos.col),
            new Position(pos.row, pos.col - 1),
            new Position(pos.row, pos.col + 1),
        };

        foreach (Position neighbor in neighbors)
        {
            if (!state.board.InBounds(neighbor))
                continue;

            StackData stack = state.board.GetStack(neighbor);
            if (stack != null && stack.owner == opponent)
                return false;
        }

        return true;
    }

    public static bool TryMove(GameState state, Position from, Position to)
    {
        if (!IsValidMove(state, from, to))
            return false;

        StackData movingStack = state.board.GetStack(from);
        StackData targetStack = state.board.GetStack(to);

        if (movingStack == null)
            return false;

        if (targetStack == null)
        {
            state.board.SetStack(to, new StackData(movingStack.owner, movingStack.height));
            state.board.SetStack(from, null);
        }
        else
        {
            int newHeight = movingStack.height + targetStack.height;
            state.board.SetStack(to, new StackData(movingStack.owner, newHeight));
            state.board.SetStack(from, null);
        }

        state.playTurnsTaken++;
        SwitchPlayer(state);
        return true;
    }

    public static bool IsValidMove(GameState state, Position from, Position to)
    {
        if (state == null) return false;
        if (state.board == null) return false;

        if (state.phase != GamePhase.Play)
        {
            Debug.Log("IsValidMove failed: game is not in Play phase.");
            return false;
        }

        if (!state.board.InBounds(from) || !state.board.InBounds(to))
        {
            Debug.Log("IsValidMove failed: out of bounds.");
            return false;
        }

        if (!AreAdjacent(from, to))
        {
            Debug.Log("IsValidMove failed: source and target are not adjacent.");
            return false;
        }

        StackData movingStack = state.board.GetStack(from);
        if (movingStack == null)
        {
            Debug.Log("IsValidMove failed: source cell is empty.");
            return false;
        }

        if (movingStack.owner != state.currentPlayer)
        {
            Debug.Log("IsValidMove failed: source stack does not belong to current player.");
            return false;
        }

        StackData targetStack = state.board.GetStack(to);

        if (targetStack == null)
            return true;

        if (targetStack.owner == state.currentPlayer)
            return true;

        Debug.Log("IsValidMove failed: target contains enemy stack. Use EAT instead.");
        return false;
    }

    public static bool AreAdjacent(Position a, Position b)
    {
        int rowDiff = Mathf.Abs(a.row - b.row);
        int colDiff = Mathf.Abs(a.col - b.col);
        return rowDiff + colDiff == 1;
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