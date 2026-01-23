using Enums;
using Ingame;
using Ingame.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class LayerVisibilityController : MonoBehaviour
{
    public Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();

    [Header("Layer Range Settings")]
    [SerializeField]
    private int previewMax = 1;      // exclusive upper bound for fully visible layers
    [SerializeField]
    private int rePreviewMax = 7;    // exclusive upper bound for prereview (gray) layers

    [SerializeField]
    private int preViewMin = 0;      // inclusive lower bound for fully visible layers

    public float fadeDuration = 0.5f;

    public int RePreviewMax { get => rePreviewMax; set => rePreviewMax = value; }
    public int PreViewMin { get => preViewMin; set => preViewMin = value; }

    internal void ApplyLayerVisibility()
    {
        if (layerQueue == null || layerQueue.Count == 0)
            return;

        var layers = layerQueue.ToList();
        var lm = GetComponentInParent<LayerManager>();

        for (int i = 0; i < layers.Count; i++)
        {
            BaseLayer layer = layers[i];
            if (layer == null) continue;

            // Decide state using clear, easy-to-read ranges:
            // - fully visible: indices in [preViewMin, previewMax)
            // - prereview (gray): indices in [previewMax, rePreviewMax)
            // - hidden: indices >= rePreviewMax
            if (IsFullyVisibleIndex(i))
            {
                SetLayerFullyVisible(layer, i, lm);
            }
            else if (IsPrereviewIndex(i))
            {
                SetLayerPrereview(layer, i, lm);
            }
            else if (IsHiddenIndex(i))
            {
                Debug.Log($"Hiding layer at index {i}");
                SetLayerHidden(layer, i, lm);
            }
            else
            {
                layer.gameObject.SetActive(false);
                LayerUtils.ActiveObjectInLayer(false, i, lm);
            }
        }
    }

    // Range helpers (keeps comparison logic in one place)
    private bool IsFullyVisibleIndex(int index) => index >= preViewMin && index < previewMax;
    private bool IsPrereviewIndex(int index) => index >= previewMax && index < rePreviewMax;
    private bool IsHiddenIndex(int index) => index >= previewMax && index < previewMax + 1;
    // State handlers (single responsibility, easy to modify)
    private void SetLayerFullyVisible(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf)
            go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part?.Renderer == null) continue;
            var color = part.Renderer.material.color;
            color.a = 1f;
            StartCoroutine(FadeToOriginalColor(part.Renderer, color, fadeDuration));
        }

        LayerUtils.ActiveObjectInLayer(true, index, lm);
    }

    private void SetLayerPrereview(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf)
            go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part?.Renderer == null) continue;
            StartCoroutine(FadeToGray(part.Renderer, fadeDuration));
        }

        LayerUtils.ActiveObjectInLayer(true, index, lm);
    }

    private void SetLayerHidden(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (go.activeSelf)
            go.SetActive(false);

        LayerUtils.ActiveObjectInLayer(false, index, lm);
        PreviewHiddenLayer(index);
        // NOTE:
        // We intentionally do NOT call PreviewHiddenRange or change colors here.
        // PreviewHiddenRange remains a separate API you can call when you want
        // hidden layers to be shown as black previews (the caller controls that).
    }

    // =========================================================
    // NEW: Preview hidden helpers (keeps behaviour separate from ApplyLayerVisibility)
    // - These functions will make the layer visible but set its parts to black.
    // - They DO NOT call LayerUtils.ActiveObjectInLayer (per request).
    // =========================================================

    public void PreviewHiddenLayer(int layerIndex)
    {
        if (layerQueue == null || layerQueue.Count == 0) return;

        var layers = layerQueue.ToList();
        if (layerIndex < 0 || layerIndex >= layers.Count) return;

        var layer = layers[layerIndex];
        var go = layer.GameObject;
        if (!go.activeSelf)
            go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part?.Renderer != null)
                StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
        }
    }

    public void PreviewHiddenRange(int startIndex, int endIndex)
    {
        if (layerQueue == null || layerQueue.Count == 0) return;
        var layers = layerQueue.ToList();

        startIndex = Mathf.Max(0, startIndex);
        endIndex = Mathf.Min(layers.Count, endIndex);

        for (int i = startIndex; i < endIndex; i++)
        {
            var layer = layers[i];
            if (layer == null) continue;
            var go = layer.GameObject;
            if (!go.activeSelf)
                go.SetActive(true);

            foreach (var part in layer.parts)
            {
                if (part?.Renderer != null)
                    StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
            }
        }
    }

    // =========================================================
    // Existing helpers
    // =========================================================
    internal void HideTopLayer()
    {

    }

    internal void ShowNextLayer()
    {
        preViewMin++;
        previewMax += 1;
        rePreviewMax += 1;

        Debug.Log($"show next layer min: {preViewMin}, max {previewMax}, rePreviewMin {rePreviewMax}, layer queue {layerQueue.Count}");

        ApplyLayerVisibility();
    }

    IEnumerator FadeToGray(Renderer renderer, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = renderer.material.color;
        Color targetColor = startColor;
        targetColor.a = 0.6f;
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
        Color startColor = renderer.material.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, originalColor, t / duration);
            yield return null;
        }
    }

    IEnumerator FadeToBlack(Renderer renderer, Renderer outline, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = renderer.material.color;
        Color targetColor = ColorEnum.Brown.ToColor();
        targetColor.a = startColor.a; // keep same alpha as original
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, targetColor, t / duration);
            outline.material.color = Color.Lerp(startColor, targetColor, t / duration);
            yield return null;
        }
    }
}