using TMPro;
using UnityEngine;

public class CellView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private SpriteRenderer tokenRenderer;
    [SerializeField] private TextMeshProUGUI heightText;

    private int row;
    private int col;
    private bool isSelected = false;

    private void Reset()
    {
        AutoAssignReferences();
    }

    private void OnValidate()
    {
        AutoAssignReferences();
    }

    private void AutoAssignReferences()
    {
        if (backgroundRenderer == null)
            backgroundRenderer = GetComponent<SpriteRenderer>();

        if (tokenRenderer == null)
        {
            Transform token = transform.Find("Token");
            if (token != null)
                tokenRenderer = token.GetComponent<SpriteRenderer>();
        }

        if (heightText == null)
        {
            Transform canvas = transform.Find("HeightCanvas");
            if (canvas != null)
            {
                Transform text = canvas.Find("HeightText");
                if (text != null)
                    heightText = text.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    public void Setup(int row, int col)
    {
        this.row = row;
        this.col = col;
        gameObject.name = $"Cell_{row}_{col}";
        UpdateBackgroundColor();
    }

    public void Refresh(StackData stack)
    {
        if (stack == null)
        {
            if (tokenRenderer != null)
                tokenRenderer.gameObject.SetActive(false);

            if (heightText != null)
                heightText.text = "";

            return;
        }

        if (tokenRenderer != null)
        {
            tokenRenderer.gameObject.SetActive(true);
            tokenRenderer.color = stack.owner == PlayerColor.Red ? Color.red : Color.blue;
        }

        if (heightText != null)
        {
            heightText.text = stack.height.ToString();
            heightText.color = Color.white;
        }
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
    }

    private void UpdateBackgroundColor()
    {
        if (backgroundRenderer == null) return;

        bool dark = (row + col) % 2 == 0;
        Color baseColor = dark
            ? new Color(0.82f, 0.82f, 0.82f)
            : new Color(0.92f, 0.92f, 0.92f);

        if (isSelected)
        {
            backgroundRenderer.color = new Color(1f, 0.9f, 0.4f);
        }
        else
        {
            backgroundRenderer.color = baseColor;
        }
    }

    private void OnMouseDown()
    {
        Debug.Log($"Clicked {gameObject.name} ({row}, {col})");

        if (GameManager.Instance != null)
            GameManager.Instance.HandleCellClicked(new Position(row, col));
    }
}