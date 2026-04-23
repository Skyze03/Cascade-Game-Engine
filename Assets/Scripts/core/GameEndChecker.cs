using System.Text;

public static class GameEndChecker
{
    public static void EvaluateAfterPlayAction(GameState state)
    {
        if (state == null || state.board == null)
            return;

        // 1. Elimination
        int redTokens = CountTokensForPlayer(state.board, PlayerColor.Red);
        int blueTokens = CountTokensForPlayer(state.board, PlayerColor.Blue);

        if (redTokens == 0 && blueTokens == 0)
        {
            state.result = GameResult.Draw;
            state.phase = GamePhase.GameOver;
            return;
        }

        if (redTokens == 0)
        {
            state.result = GameResult.BlueWins;
            state.phase = GamePhase.GameOver;
            return;
        }

        if (blueTokens == 0)
        {
            state.result = GameResult.RedWins;
            state.phase = GamePhase.GameOver;
            return;
        }

        // 2. Threefold repetition
        string stateKey = BuildStateKey(state);
        if (!state.repetitionCounts.ContainsKey(stateKey))
        {
            state.repetitionCounts[stateKey] = 0;
        }

        state.repetitionCounts[stateKey]++;

        if (state.repetitionCounts[stateKey] >= 3)
        {
            state.result = GameResult.Draw;
            state.phase = GamePhase.GameOver;
            return;
        }

        // 3. Turn limit (300 play turns)
        if (state.playTurnsTaken >= 300)
        {
            if (redTokens > blueTokens)
            {
                state.result = GameResult.RedWins;
            }
            else if (blueTokens > redTokens)
            {
                state.result = GameResult.BlueWins;
            }
            else
            {
                state.result = GameResult.Draw;
            }

            state.phase = GamePhase.GameOver;
        }
    }

    public static int CountTokensForPlayer(BoardState board, PlayerColor player)
    {
        int total = 0;

        for (int row = 0; row < BoardState.SIZE; row++)
        {
            for (int col = 0; col < BoardState.SIZE; col++)
            {
                StackData stack = board.cells[row, col];
                if (stack != null && stack.owner == player)
                {
                    total += stack.height;
                }
            }
        }

        return total;
    }

    public static string BuildStateKey(GameState state)
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("P:");
        sb.Append(state.currentPlayer);
        sb.Append("|");

        for (int row = 0; row < BoardState.SIZE; row++)
        {
            for (int col = 0; col < BoardState.SIZE; col++)
            {
                StackData stack = state.board.cells[row, col];

                if (stack == null)
                {
                    sb.Append(".,");
                }
                else
                {
                    char ownerChar = stack.owner == PlayerColor.Red ? 'R' : 'B';
                    sb.Append(ownerChar);
                    sb.Append(stack.height);
                    sb.Append(",");
                }
            }
        }

        return sb.ToString();
    }
}