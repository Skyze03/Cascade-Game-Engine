using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button moveEatButton;
    [SerializeField] private Button cascadeButton;
    [SerializeField] private Button restartButton;

    [SerializeField] private TMP_Text gameOverText;

    private void Start()
    {
        if (moveEatButton != null)
            moveEatButton.onClick.AddListener(() => GameManager.Instance.SetPlayModeToMoveEat());

        if (cascadeButton != null)
            cascadeButton.onClick.AddListener(() => GameManager.Instance.SetPlayModeToCascade());

        if (restartButton != null)
            restartButton.onClick.AddListener(() => GameManager.Instance.RestartGame());

        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    public void Refresh(GameManager gameManager, Position? selectedCell)
    {
        if (statusText == null || gameManager == null)
            return;

        string selectedText = selectedCell.HasValue
            ? $"Selected: ({selectedCell.Value.row}, {selectedCell.Value.col})"
            : "Selected: None";

        string resultText = $"Result: {gameManager.CurrentResult}";

        statusText.text =
            $"Phase: {gameManager.CurrentPhase}\n" +
            $"Current Player: {gameManager.CurrentPlayer}\n" +
            $"Mode: {gameManager.CurrentPlayMode}\n" +
            $"{selectedText}\n" +
            $"{resultText}";

        RefreshGameOverText(gameManager.CurrentResult);
    }

    private void RefreshGameOverText(GameResult result)
    {
        if (gameOverText == null)
            return;

        if (result == GameResult.Ongoing)
        {
            gameOverText.gameObject.SetActive(false);
            return;
        }

        gameOverText.gameObject.SetActive(true);

        switch (result)
        {
            case GameResult.RedWins:
                gameOverText.text = "Red Wins!";
                gameOverText.color = Color.red;
                break;

            case GameResult.BlueWins:
                gameOverText.text = "Blue Wins!";
                gameOverText.color = Color.blue;
                break;

            case GameResult.Draw:
                gameOverText.text = "Draw!";
                gameOverText.color = Color.black;
                break;
        }
    }
}