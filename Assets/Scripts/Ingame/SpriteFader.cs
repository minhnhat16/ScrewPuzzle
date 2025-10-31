using UnityEngine;
using DG.Tweening;

public class SpriteSwapper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeTime = 0.5f;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SwapSprite(Sprite newSprite)
    {
        // Fade out
        spriteRenderer.DOFade(0f, fadeTime)
            .OnComplete(() =>
            {
                spriteRenderer.sprite = newSprite; // đổi sprite sau khi ẩn
                spriteRenderer.DOFade(1f, fadeTime); // fade in lại
            });
    }
}
    