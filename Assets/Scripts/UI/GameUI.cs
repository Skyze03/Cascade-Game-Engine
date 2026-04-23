using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button moveEatButton;
    [SerializeField] private Button cascadeButton;

    private void Start()
    {
        if (moveEatButton != null)
            moveEatButton.onClick.AddListener(() => GameManager.Instance.SetPlayModeToMoveEat());

        if (cascadeButton != null)
            cascadeButton.onClick.AddListener(() => GameManager.Instance.SetPlayModeToCascade());
    }

    public void Refresh(GameManager gameManager, Position? selectedCell)
    {
        if (statusText == null || gameManager == null)
            return;

        string selectedText = selectedCell.HasValue
            ? $"Selected: ({selectedCell.Value.row}, {selectedCell.Value.col})"
            : "Selected: None";

        statusText.text =
            $"Phase: {gameManager.CurrentPhase}\n" +
            $"Current Player: {gameManager.CurrentPlayer}\n" +
            $"Mode: {gameManager.CurrentPlayMode}\n" +
            $"{selectedText}";
    }
}