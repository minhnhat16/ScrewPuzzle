using UnityEngine;

[ExecuteAlways]
public class BackgroundSizeControl : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void Fit()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * screenRatio;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = worldScreenWidth / spriteSize.x;
        float scaleY = worldScreenHeight / spriteSize.y;

        // scale theo chiều lớn hơn để phủ đầy màn hình
        transform.localScale = new Vector3(Mathf.Max(scaleX, scaleY), Mathf.Max(scaleX, scaleY), 1);
    }
}
