using Ingame;
using Ingame.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LayerVisibilityController : MonoBehaviour
{
    public Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();

    [Header("Layer Range Settings")]
    [SerializeField]
    private int previewMax = 1;      // 1–4
    [SerializeField]
    private int rePreviewMax = 7;    // 5–7

    [SerializeField]
    private int preViewMin = 0;

    public float fadeDuration = 0.5f;

    internal void ApplyLayerVisibility()
    {

        Debug.Log("Applying Layer Visibility..." + layerQueue + ",count :" + layerQueue.Count);

        if (layerQueue == null || layerQueue.Count == 0)
            return;
        Debug.Log("Applying Layer Visibility...");
        // Lấy danh sách tạm để dễ duyệt
        var layers = layerQueue.ToList();
        var lm = GetComponentInParent<LayerManager>();

        for (int i = 0; i < layers.Count; i++)
        {
            BaseLayer layer = layers[i];
            GameObject go = layer.GameObject;


            Debug.Log($"Layer {i}: {go.name}, and {i < previewMax}");


            if (preViewMin > i && i < previewMax)
            {

                Debug.Log($"[VisibilityController] Showing layer {i} clearly. Total Part {layer.parts.Count}");

                // --- Preview (hiển thị rõ) ---
                if (!go.activeSelf)
                    go.SetActive(true);

                foreach (var part in layer.parts)
                {
                    var color = part.Renderer.material.color;
                    color.a = 1f;
                    StartCoroutine(FadeToOriginalColor(part.Renderer, color, fadeDuration));
                }

                LayerUtils.ActiveObjectInLayer(true, i, lm);

            }
            else if (previewMax > i && i < rePreviewMax)
            {

                Debug.Log($"[VisibilityController] Re-previewing layer {i} to gray. Total Part {layer.parts.Count}");

                // --- Re-preview (fade xám) ---
                if (!go.activeSelf)
                    go.SetActive(true);



                foreach (var part in layer.parts)
                {
                    if (part.Renderer != null)
                    {
                        StartCoroutine(FadeToGray(part.Renderer, fadeDuration));
                    }

                    //if (part.OutLine != null)
                    //    StartCoroutine(FadeToGray(part.OutLine, fadeDuration));
                }
                LayerUtils.ActiveObjectInLayer(true, i, lm);

            }
            else
            {

                go.SetActive(false);
                LayerUtils.ActiveObjectInLayer(false, i, lm);

            }

        }
    }

    internal void HideTopLayer()
    {

    }

    internal void ShowNextLayer()
    {
        preViewMin = previewMax;
        previewMax += 1;
        rePreviewMax += 1;

        Debug.Log($"show next layer min: {preViewMin}, max {previewMax}, rePreviewMin {rePreviewMax}, layer queue {layerQueue.Count}");

        ApplyLayerVisibility();
    }

    IEnumerator FadeToGray(Renderer renderer, float duration)
    {
        if (renderer == null) yield break;
        Debug.Log("Fading to gray: " + renderer.gameObject.name);
        Color startColor = renderer.material.color;
        Color targetColor = startColor;
        targetColor.a = 0.4f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(Color.clear, targetColor, t / duration);
            yield return null;
        }
    }

    IEnumerator FadeToOriginalColor(Renderer renderer, Color originalColor, float duration)
    {
        if (renderer == null) yield break;
        Debug.Log("Fading to original color: " + renderer.gameObject.name);
        Color startColor = renderer.material.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, originalColor, t / duration);
            yield return null;
        }
    }
}
