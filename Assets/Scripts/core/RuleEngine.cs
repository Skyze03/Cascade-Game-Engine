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

    public static bool TryEat(GameState state, Position from, Position to)
    {
        if (!IsValidEat(state, from, to))
            return false;

        StackData attacker = state.board.GetStack(from);
        StackData defender = state.board.GetStack(to);

        if (attacker == null || defender == null)
            return false;

        state.board.SetStack(to, new StackData(attacker.owner, attacker.height));
        state.board.SetStack(from, null);

        state.playTurnsTaken++;
        SwitchPlayer(state);
        return true;
    }

    public static bool IsValidEat(GameState state, Position from, Position to)
    {
        if (state == null) return false;
        if (state.board == null) return false;

        if (state.phase != GamePhase.Play)
        {
            Debug.Log("IsValidEat failed: game is not in Play phase.");
            return false;
        }

        if (!state.board.InBounds(from) || !state.board.InBounds(to))
        {
            Debug.Log("IsValidEat failed: out of bounds.");
            return false;
        }

        if (!AreAdjacent(from, to))
        {
            Debug.Log("IsValidEat failed: source and target are not adjacent.");
            return false;
        }

        StackData attacker = state.board.GetStack(from);
        if (attacker == null)
        {
            Debug.Log("IsValidEat failed: source cell is empty.");
            return false;
        }

        if (attacker.owner != state.currentPlayer)
        {
            Debug.Log("IsValidEat failed: source stack does not belong to current player.");
            return false;
        }

        StackData defender = state.board.GetStack(to);
        if (defender == null)
        {
            Debug.Log("IsValidEat failed: target cell is empty.");
            return false;
        }

        if (defender.owner == state.currentPlayer)
        {
            Debug.Log("IsValidEat failed: target contains friendly stack. Use MOVE instead.");
            return false;
        }

        if (attacker.height < defender.height)
        {
            Debug.Log("IsValidEat failed: attacker height is smaller than defender height.");
            return false;
        }

        return true;
    }

    public static bool TryCascade(GameState state, Position from, Position directionTarget)
    {
        Direction direction;
        if (!TryGetDirectionFromAdjacent(from, directionTarget, out direction))
        {
            Debug.Log("TryCascade failed: direction target is not adjacent.");
            return false;
        }

        if (!IsValidCascade(state, from, direction))
            return false;

        StackData sourceStack = state.board.GetStack(from);
        if (sourceStack == null)
            return false;

        int height = sourceStack.height;
        PlayerColor owner = sourceStack.owner;

        // 原 stack 消失
        state.board.SetStack(from, null);

        // 逐个 token 向前落下
        Position current = from;
        for (int i = 1; i <= height; i++)
        {
            Position target = GetNextPosition(from, direction, i);

            // 这个 cascade token 自己掉出边界，直接消失
            if (!state.board.InBounds(target))
                continue;

            // 如果目标格已有 stack，先把那个 stack 往前推
            if (!state.board.IsEmpty(target))
            {
                PushStack(state.board, target, direction);
            }

            // 现在在 target 放一个高度 1 的 token
            PlaceSingleToken(state.board, target, owner);
        }

        state.playTurnsTaken++;
        SwitchPlayer(state);
        return true;
    }

    public static bool IsValidCascade(GameState state, Position from, Direction direction)
    {
        if (state == null) return false;
        if (state.board == null) return false;

        if (state.phase != GamePhase.Play)
        {
            Debug.Log("IsValidCascade failed: game is not in Play phase.");
            return false;
        }

        if (!state.board.InBounds(from))
        {
            Debug.Log("IsValidCascade failed: source out of bounds.");
            return false;
        }

        StackData sourceStack = state.board.GetStack(from);
        if (sourceStack == null)
        {
            Debug.Log("IsValidCascade failed: source cell is empty.");
            return false;
        }

        if (sourceStack.owner != state.currentPlayer)
        {
            Debug.Log("IsValidCascade failed: source stack does not belong to current player.");
            return false;
        }

        if (sourceStack.height < 2)
        {
            Debug.Log("IsValidCascade failed: stack height must be at least 2.");
            return false;
        }

        return true;
    }

    private static void PushStack(BoardState board, Position pos, Direction direction)
    {
        StackData stack = board.GetStack(pos);
        if (stack == null)
            return;

        Position next = GetNextPosition(pos, direction, 1);

        // 推出边界：删除该 stack
        if (!board.InBounds(next))
        {
            board.SetStack(pos, null);
            return;
        }

        // 如果下一格也有 stack，先递归推下一格
        if (!board.IsEmpty(next))
        {
            PushStack(board, next, direction);
        }

        // 把当前 stack 推过去
        board.SetStack(next, stack);
        board.SetStack(pos, null);
    }

    private static void PlaceSingleToken(BoardState board, Position pos, PlayerColor owner)
    {
        // Cascade 放下的永远是单个 token（高度 1）
        board.SetStack(pos, new StackData(owner, 1));
    }

    public static bool AreAdjacent(Position a, Position b)
    {
        int rowDiff = Mathf.Abs(a.row - b.row);
        int colDiff = Mathf.Abs(a.col - b.col);
        return rowDiff + colDiff == 1;
    }

    public static bool TryGetDirectionFromAdjacent(Position from, Position to, out Direction direction)
    {
        direction = Direction.Up;

        if (!AreAdjacent(from, to))
            return false;

        if (to.row == from.row - 1 && to.col == from.col)
        {
            direction = Direction.Up;
            return true;
        }

        if (to.row == from.row + 1 && to.col == from.col)
        {
            direction = Direction.Down;
            return true;
        }

        if (to.row == from.row && to.col == from.col - 1)
        {
            direction = Direction.Left;
            return true;
        }

        if (to.row == from.row && to.col == from.col + 1)
        {
            direction = Direction.Right;
            return true;
        }

        return false;
    }

    public static Position GetNextPosition(Position start, Direction direction, int distance)
    {
        switch (direction)
        {
            case Direction.Up:
                return new Position(start.row - distance, start.col);
            case Direction.Down:
                return new Position(start.row + distance, start.col);
            case Direction.Left:
                return new Position(start.row, start.col - distance);
            case Direction.Right:
                return new Position(start.row, start.col + distance);
            default:
                return start;
        }
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