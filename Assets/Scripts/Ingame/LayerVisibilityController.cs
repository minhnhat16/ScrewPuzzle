using Ingame.Board;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LayerVisibilityController : MonoBehaviour
{
    public Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();

    [Header("Layer Range Settings")]
    public int previewMax = 4;      // 1–4
    public int rePreviewMax = 7;    // 5–7

    public float fadeDuration = 0.5f;

    internal void ApplyLayerVisibility()
    {
        if (layerQueue == null || layerQueue.Count == 0)
            return;

        // Lấy danh sách tạm để dễ duyệt
        var layers = layerQueue.ToList();

        for (int i = 0; i < layers.Count; i++)
        {
            BaseLayer layer = layers[i];
            GameObject go = layer.GameObject;

            if (i < previewMax)
            {
                // --- Preview (hiển thị rõ) ---
                if (!go.activeSelf)
                    go.SetActive(true);

                foreach (var part in layer.parts)
                {
                    if (part.Renderer != null)
                        part.Renderer.color = Color.white;

                    //if (part.OutLine != null)
                    //    part.OutLine.color = Color.white;
                }
            }
            else if (i < rePreviewMax)
            {
                // --- Re-preview (fade xám) ---
                if (!go.activeSelf)
                    go.SetActive(true);

                foreach (var part in layer.parts)
                {
                    if (part.Renderer != null)
                        StartCoroutine(FadeToGray(part.Renderer, fadeDuration));

                    //if (part.OutLine != null)
                    //    StartCoroutine(FadeToGray(part.OutLine, fadeDuration));
                }
            }
            else
            {
                // --- Ẩn hoàn toàn ---
                if (go.activeSelf)
                    go.SetActive(false);
            }
        }
    }

    IEnumerator FadeToGray(Renderer renderer, float duration)
    {
        if (renderer == null) yield break;

        Color startColor = renderer.material.color;
        Color targetColor = Color.gray;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, targetColor, t / duration);
            yield return null;
        }
    }
}
