using Enums;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles visual representation of a Box.
/// Applies the correct sprite to the box body and lid based on ColorEnum.
/// Attach this component to the same GameObject as Box.
/// </summary>
public class BoxRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer boxBodyRenderer;
    [SerializeField] private SpriteRenderer boxLidRenderer;

    [Header("Fallback")]
    [SerializeField] private Color fallbackColor = Color.white;

    private ColorEnum _currentColor = ColorEnum.Clear;

    private void Awake()
    {
        if (boxBodyRenderer == null)
            Debug.LogWarning("[BoxRenderer] boxBodyRenderer not assigned.");
        if (boxLidRenderer == null)
            Debug.LogWarning("[BoxRenderer] boxLidRenderer not assigned.");
    }

    /// <summary>
    /// Apply the visual style matching the given ColorEnum.
    /// Call this from Box.Initialize().
    /// </summary>
    public void SetColor(ColorEnum color)
    {
        _currentColor = color;
        Debug.Log($"[BoxRenderer] Applying color {color} to box.");
        ApplySprite(boxBodyRenderer, color);
        SetLidColor(boxLidRenderer,color);
    }

    /// <summary>
    /// Returns the currently applied color.
    /// </summary>
    public ColorEnum CurrentColor => _currentColor;

    // ─── Private ───────────────────────────────────────────────────

    private void ApplySprite(SpriteRenderer renderer, ColorEnum color)
    {
        if (renderer == null) return;

        Sprite sprite = color.ToBoxSprite();
        if (sprite != null)
        {
            renderer.sprite = sprite;
            renderer.color = Color.white; // reset tint to show sprite as-is
        }
        else
        {
            // Fallback: tint the existing sprite with the color's tint
            renderer.color = color == ColorEnum.Rainbow ? fallbackColor : color.ToColor();
        }
    }

    private void SetLidColor(SpriteRenderer renderer,ColorEnum color)
    {
        var c = color.ToColor();
        c.a = 0.8f;
        renderer.color = c;
    }
}