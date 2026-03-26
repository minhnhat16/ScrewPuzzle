using UnityEngine;

public class CenteredGridLayout : MonoBehaviour
{
    public enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    [SerializeField] private Vector2 cellSize = new(100f, 100f);
    [SerializeField] private Vector2 spacing = new(16f, 16f);
    [SerializeField] private int columns = 3;
    [SerializeField] private Alignment alignment = Alignment.TopLeft;

    public void Apply()
    {
        int count = transform.childCount;
        if (count == 0) return;

        int rows = Mathf.CeilToInt((float)count / columns);

        // Tính width lớn nhất thật sự của các hàng
        int firstRowCount = Mathf.Min(columns, count);
        float contentWidth = firstRowCount * cellSize.x + (firstRowCount - 1) * spacing.x;
        float contentHeight = rows * cellSize.y + (rows - 1) * spacing.y;

        Vector2 alignmentOffset = GetAlignmentOffset(contentWidth, contentHeight);

        for (int i = 0; i < count; i++)
        {
            var rt = transform.GetChild(i) as RectTransform;
            if (rt == null) continue;

            int row = i / columns;
            int col = i % columns;

            bool isLastRow = row == rows - 1;
            int itemsInRow = isLastRow ? count - row * columns : columns;

            float rowWidth = itemsInRow * cellSize.x + (itemsInRow - 1) * spacing.x;

            // startX của riêng hàng này, căn theo content thực
            float startX = -rowWidth / 2f;
            float x = startX + col * (cellSize.x + spacing.x) + cellSize.x / 2f;

            // top xuống dưới
            float startY = contentHeight / 2f - cellSize.y / 2f;
            float y = startY - row * (cellSize.y + spacing.y);

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = cellSize;
            rt.anchoredPosition = new Vector2(x, y) + alignmentOffset;
        }

        var self = transform as RectTransform;
        if (self != null)
        {
            self.sizeDelta = new Vector2(contentWidth, contentHeight);
        }
    }
    private Vector2 GetAlignmentOffset(float width, float height)
    {
        return alignment switch
        {
            Alignment.TopLeft => new Vector2(-width / 2, height / 2),
            Alignment.TopCenter => new Vector2(0, height / 2),
            Alignment.TopRight => new Vector2(width / 2, height / 2),

            Alignment.MiddleLeft => new Vector2(-width / 2, 0),
            Alignment.MiddleCenter => Vector2.zero,
            Alignment.MiddleRight => new Vector2(width / 2, 0),

            Alignment.BottomLeft => new Vector2(-width / 2, -height / 2),
            Alignment.BottomCenter => new Vector2(0, -height / 2),
            Alignment.BottomRight => new Vector2(width / 2, -height / 2),

            _ => Vector2.zero
        };
    }

    public void Clear()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        var self = transform as RectTransform;
        if (self != null) self.sizeDelta = Vector2.zero;
    }

#if UNITY_EDITOR
    private void OnValidate() => Apply();
#endif
}