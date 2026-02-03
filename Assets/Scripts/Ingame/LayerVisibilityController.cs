using Enums;
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
    public List<BaseLayer> indexedLayers = new List<BaseLayer>();

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
        // Use indexedLayers (preserves indices). Fallback to queue->list for legacy usage.
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();

        var lm = GetComponentInParent<LayerManager>();
        int count = layers.Count;

        // ensure ranges valid for current count
        preViewMin = Mathf.Clamp(preViewMin, 0, Math.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        for (int i = 0; i < layers.Count; i++)
        {
            BaseLayer layer = layers[i];
            if (layer == null)
            {
                // placeholder: keep index reserved, nothing to show for this slot
                continue;
            }

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
                SetLayerHidden(layer, i, lm);
            }
            else
            {
                // out-of-window -> disable
                layer.gameObject.SetActive(false);
                LayerUtils.ActiveObjectInLayer(false, layer, lm);
            }
        }
    }

    // Range helpers (keeps comparison logic in one place)
    private bool IsFullyVisibleIndex(int index) => index >= preViewMin && index < previewMax;
    private bool IsPrereviewIndex(int index) => index >= previewMax && index < rePreviewMax;
    private bool IsHiddenIndex(int index) => index >= rePreviewMax; // fixed: use rePreviewMax

    // State handlers (single responsibility, easy to modify)
    private void SetLayerFullyVisible(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf)
            go.SetActive(true);

        // Start fades for all parts and only activate objects in layer after all fades complete.
        var partsToFade = layer.parts != null && layer.parts.Count > 0 ? layer.parts : new List<BasePart>();
        if(gameObject.activeSelf == false)
        {
            // If controller is inactive, skip fades and activate immediately
            LayerUtils.ActiveObjectInLayer(true, layer, lm);
            return;
        }
        // Kick off fade coroutines for each part. Activation of screws/objects will happen after all fades finish.
        StartCoroutine(ActivateAfterFade(partsToFade, layer, index, lm));
    }

    /// <summary>
    /// Starts fade coroutine per part and waits until all fades complete, then activates objects in layer.
    /// </summary>
    private IEnumerator ActivateAfterFade(List<BasePart> partsToFade, BaseLayer layer, int index, LayerManager lm)
    {
        if (partsToFade == null || partsToFade.Count == 0)
        {
            // Activate by BaseLayer
            LayerUtils.ActiveObjectInLayer(true, layer, lm);
            yield break;
        }

        int remaining = partsToFade.Count;

        // Color to fade to: opaque white
        Color targetColor = Color.white;
        targetColor.a = 1f;

        // Start a fade coroutine for every part; each will decrement remaining when done.
        foreach (var part in partsToFade)
        {
            if (part == null || part.Renderer == null) { remaining--; continue; }
            var r = part.Renderer;
            var outline = part.Outline;
            StartCoroutine(FadePartAndNotify(r, outline, targetColor, fadeDuration, () => remaining--));
        }

        // Wait until all part fades finished
        while (remaining > 0)
            yield return null;

        // Now that visuals are restored, enable screws/objects in the layer (by object)
        LayerUtils.ActiveObjectInLayer(true, layer, lm);
    }

    /// <summary>
    /// Wraps FadeToOriginalColor and invokes onDone when finished.
    /// </summary>
    private IEnumerator FadePartAndNotify(Renderer renderer, Renderer outline, Color originalColor, float duration, Action onDone)
    {
        yield return StartCoroutine(FadeToOriginalColor(renderer, outline, originalColor, duration));
        onDone?.Invoke();
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

        // Activate by BaseLayer
        LayerUtils.ActiveObjectInLayer(true, layer, lm);
    }

    private void SetLayerHidden(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (go.activeSelf)
            go.SetActive(false);

        // Deactivate by BaseLayer
        LayerUtils.ActiveObjectInLayer(false, layer, lm);
        PreviewHiddenLayer(index);
    }

    // =========================================================
    // Preview helpers
    // =========================================================
    public void PreviewHiddenLayer(int layerIndex)
    {
        var layers = indexedLayers != null && indexedLayers.Count > 0 ? indexedLayers : layerQueue.ToList();
        if (layerIndex < 0 || layerIndex >= layers.Count) return;

        var layer = layers[layerIndex];
        if (layer == null) return;
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
        var layers = indexedLayers != null && indexedLayers.Count > 0 ? indexedLayers : layerQueue.ToList();

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

    // Find next active layer index after `startAt` (inclusive)
    private int FindNextActiveIndex(List<BaseLayer> layers, int startAt)
    {
        if (layers == null) return -1;
        for (int i = Mathf.Max(0, startAt); i < layers.Count; i++)
        {
            var l = layers[i];
            if (l != null && l.GameObject != null && l.GameObject.activeInHierarchy)
                return i;
        }
        return -1;
    }

    // Find first active layer index (search from 0)
    private int FindFirstActiveIndex(List<BaseLayer> layers)
    {
        return FindNextActiveIndex(layers, 0);
    }

    internal void ShowNextLayer()
    {
        // Use indexedLayers count if available
        var layers = indexedLayers != null && indexedLayers.Count > 0 ? indexedLayers : layerQueue.ToList();
        int count = layers.Count;

        // already at end
        if (previewMax >= count)
            return;

        // preserve window widths
        int visibleWidth = Mathf.Max(1, previewMax - preViewMin);
        int prereviewWidth = Mathf.Max(0, rePreviewMax - previewMax);

        // try to advance to next active layer after current preViewMin
        int nextIndex = FindNextActiveIndex(layers, preViewMin + 1);
        if (nextIndex < 0)
        {
            // fallback: keep previous behavior of incrementing by 1 (but clamped)
            preViewMin = Mathf.Clamp(preViewMin + 1, 0, Math.Max(0, count - 1));
        }
        else
        {
            preViewMin = nextIndex;
        }

        // restore widths
        previewMax = Mathf.Clamp(preViewMin + visibleWidth, preViewMin, count);
        rePreviewMax = Mathf.Clamp(previewMax + prereviewWidth, previewMax, count);

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

    IEnumerator FadeToOriginalColor(Renderer renderer, Renderer outline, Color originalColor, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = renderer.material.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, originalColor, t / duration);
            if (outline != null)
                outline.material.color = Color.Lerp(startColor, originalColor, t / duration);
            yield return null;
        }
    }

    IEnumerator FadeToBlack(Renderer renderer, Renderer outline, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = ColorEnum.Brown.ToColor();
        Color targetColor = ColorEnum.Brown.ToColor();
        targetColor.a = 1f;
        startColor.a = 0f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(startColor, targetColor, t / duration);
            if (outline != null)
                outline.material.color = Color.Lerp(startColor, targetColor, t / duration);
            yield return null;
        }
    }

    internal void PopLayer(BaseLayer clearedLayer)
    {
        if (clearedLayer == null || indexedLayers == null)
        {
            Debug.LogWarning("[PopLayer] clearedLayer or indexedLayers is null");
            return;
        }

        // Snapshot before state for debugging
        int beforeCount = indexedLayers.Count;
        var beforeNames = indexedLayers
            .Select((l, i) => l == null ? $"[{i}]=null" : $"[{i}]={l.name}")
            .ToArray();

        int index = indexedLayers.IndexOf(clearedLayer);
        if (index < 0)
        {
            Debug.LogWarning($"[PopLayer] Layer not found in indexedLayers: {clearedLayer.name}");
            Debug.Log(Environment.StackTrace);
            return;
        }

        bool wasActive = clearedLayer.gameObject != null && clearedLayer.gameObject.activeSelf;

        Debug.Log($"[PopLayer] indexedLayers before: {string.Join(", ", beforeNames)}");

        // Hide and mark cleared (keep slot reserved)
        if (clearedLayer.gameObject != null && wasActive)
            clearedLayer.gameObject.SetActive(false);

        indexedLayers[index] = null;

        // If preViewMin pointed to the removed slot, advance it to the next active layer index
        var layers = indexedLayers;
        if (index == preViewMin)
        {
            int nextActive = FindNextActiveIndex(layers, index + 1);
            if (nextActive >= 0)
                preViewMin = nextActive;
            else
            {
                // fallback: find first active from start
                int firstActive = FindFirstActiveIndex(layers);
                preViewMin = firstActive >= 0 ? firstActive : Mathf.Clamp(preViewMin, 0, layers.Count);
            }
        }

        // Clamp ranges to valid bounds relative to indexedLayers.Count
        int count = indexedLayers.Count;
        preViewMin = Mathf.Clamp(preViewMin, 0, Math.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        var afterNames = indexedLayers
            .Select((l, i) => l == null ? $"[{i}]=null" : $"[{i}]={l.name}")
            .ToArray();

        Debug.Log($"[PopLayer] indexedLayers after removal: {string.Join(", ", afterNames)}");
        Debug.Log($"[PopLayer] Ranges after: preViewMin={preViewMin}, previewMax={previewMax}, rePreviewMax={rePreviewMax}");

        // Print stack to help trace caller
        Debug.Log($"[PopLayer] CallStack:\n{Environment.StackTrace}");

        // Re-apply visibility using indexedLayers
        ApplyLayerVisibility();
    }
}