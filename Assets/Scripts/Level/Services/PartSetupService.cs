using Ingame;
using Level;
using UnityEngine;

/// <summary>
/// Single responsibility: apply data (sprite, color, position, collider) lên một BasePart đã được spawn.
/// Không biết gì về pool, layer, hay screw dict.
/// </summary>
public class PartSetupService
{
    private readonly IPartSpriteService _spriteService;

    public PartSetupService(IPartSpriteService spriteService)
    {
        _spriteService = spriteService ?? throw new System.ArgumentNullException(nameof(spriteService));
    }

    /// <summary>
    /// Apply transform, sprite, outline, color, sorting layer lên partComponent.
    /// </summary>
    public void Setup(BasePart partComponent, BodyPartScriptable partData, int levelId, string sortingLayer)
    {
        if (partComponent == null || partData == null)
        {
            Debug.LogError("[PartSetupService] Null argument(s) in Setup.");
            return;
        }

        // ── Transform ────────────────────────────────────────────
        partComponent.transform.SetLocalPositionAndRotation(partData.partPosition, partData.partRotation);
        partComponent.transform.localScale = partData.partLocalScale;

        // ── Identity ─────────────────────────────────────────────
        partComponent.uniqueID = partData.partName;
        partComponent.gameObject.name = partData.partName;

        // ── Resolve color từ colorString (ưu tiên) hoặc default ──
        Color partColor = ResolveColor(partData.colorString);

        // ── Sprite (main) ────────────────────────────────────────
        var sprite = _spriteService.GetPartSprite(levelId, partData.spriteName, partData.layer, outline: false);
        if (sprite != null)
        {
            partComponent.Renderer.sprite = sprite;
            partComponent.Renderer.color = partColor;
        }
        else
            Debug.LogWarning($"[PartSetupService] Sprite not found: {partData.spriteName} (level {levelId})");

        // ── Outline sprite ───────────────────────────────────────
        if (partComponent.Outline != null)
        {
            var outlineSprite = _spriteService.GetPartSprite(levelId, partData.spriteName, partData.layer, outline: true);
            if (outlineSprite != null)
                partComponent.Outline.sprite = outlineSprite;
            else
                Debug.LogWarning($"[PartSetupService] Outline sprite not found: {partData.spriteName} (level {levelId})");
        }

        // ── Sorting layer ────────────────────────────────────────
        partComponent.SetSortingLayer(sortingLayer);

        // ── Collider ─────────────────────────────────────────────
        partComponent.ResetAndReapplyPolygonCollider();
        partComponent.FreezeBody();
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Parse colorString (RRGGBB hex, không có #) thành Color.
    /// Nếu không có hoặc parse lỗi → trả về white với alpha 0.8.
    /// </summary>
    private static Color ResolveColor(string colorString)
    {
        if (!string.IsNullOrEmpty(colorString) &&
            ColorUtility.TryParseHtmlString("#" + colorString, out Color parsed))
            return parsed;

        return new Color(1f, 1f, 1f, 0.8f); // default: white, alpha 0.8
    }
}