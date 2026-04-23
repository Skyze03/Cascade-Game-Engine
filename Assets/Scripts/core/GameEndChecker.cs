using System.Text;
using UnityEngine;

public static class GameEndChecker
{
    public static void EvaluateAfterPlayAction(GameState state)
    {
        if (state == null || state.board == null)
            return;

        int redTokens = CountTokensForPlayer(state.board, PlayerColor.Red);
        int blueTokens = CountTokensForPlayer(state.board, PlayerColor.Blue);

        Debug.Log($"GameEndChecker: redTokens = {redTokens}, blueTokens = {blueTokens}, playTurns = {state.playTurnsTaken}");

        // 1. Elimination
        if (redTokens == 0 && blueTokens == 0)
        {
            state.result = GameResult.Draw;
            state.phase = GamePhase.GameOver;
            Debug.Log("GameEndChecker: both players eliminated -> Draw");
            return;
        }

        if (redTokens == 0)
        {
            state.result = GameResult.BlueWins;
            state.phase = GamePhase.GameOver;
            Debug.Log("GameEndChecker: Red eliminated -> BlueWins");
            return;
        }

        if (blueTokens == 0)
        {
            state.result = GameResult.RedWins;
            state.phase = GamePhase.GameOver;
            Debug.Log("GameEndChecker: Blue eliminated -> RedWins");
            return;
        }

        // 2. Threefold repetition
        string stateKey = BuildStateKey(state);
        if (!state.repetitionCounts.ContainsKey(stateKey))
        {
            state.repetitionCounts[stateKey] = 0;
        }

        state.repetitionCounts[stateKey]++;
        Debug.Log($"GameEndChecker: repetition count for current state = {state.repetitionCounts[stateKey]}");

        if (state.repetitionCounts[stateKey] >= 3)
        {
            state.result = GameResult.Draw;
            state.phase = GamePhase.GameOver;
            Debug.Log("GameEndChecker: threefold repetition -> Draw");
            return;
        }

        // 3. Turn limit
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
            Debug.Log($"GameEndChecker: turn limit reached -> {state.result}");
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