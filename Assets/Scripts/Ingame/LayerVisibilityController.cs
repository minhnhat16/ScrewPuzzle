using Enums;
using Ingame;
using Ingame.Board;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Điều khiển visibility của từng layer trên board.
/// FIXES trong version này:
///  [FIX 1] Init() — nhận LayerManager trực tiếp (overload mới)
///           để FinalizeStep không cần tự build queue ngoài
///  [FIX 2] ApplyLayerVisibility() — bỏ `previewMax + 1` bug
///           (cộng thêm 1 mỗi lần gọi làm window trôi dần)
///  [FIX 3] HideTopLayer() — implement thực sự
/// </summary>
public class LayerVisibilityController : MonoBehaviour
{
    public Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();
    public List<BaseLayer> indexedLayers = new List<BaseLayer>();

    [Header("Layer Range Settings")]
    [SerializeField] private int previewMax = 1;   // exclusive upper bound for fully visible
    [SerializeField] private int rePreviewMax = 7;  // exclusive upper bound for prereview (gray)
    [SerializeField] private int preViewMin = 0;   // inclusive lower bound for fully visible

    public float fadeDuration = 0.5f;

    public int RePreviewMax { get => rePreviewMax; set => rePreviewMax = value; }
    public int PreViewMin { get => preViewMin; set => preViewMin = value; }

    // ──────────────────────────────────────────────────────────────
    // INIT
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// [FIX 1] Overload mới: nhận LayerManager trực tiếp.
    /// Gọi từ FinalizeStep sau khi tất cả layers đã spawn.
    /// </summary>
    public void Init(LayerManager lm)
    {
        if (lm == null || lm.Layers == null || lm.Layers.Count == 0)
        {
            Init(incomingQueue: null);
            return;
        }

        var queue = new Queue<BaseLayer>(lm.Layers);
        Init(queue);
    }

    /// <summary>
    /// Init với optional incoming queue.
    /// Priority: incomingQueue → LayerManager.Layers → scan children.
    /// </summary>
    public void Init(Queue<BaseLayer> incomingQueue = null)
    {
        indexedLayers ??= new List<BaseLayer>();

        var lm = GetComponentInParent<LayerManager>();

        if (incomingQueue != null && incomingQueue.Count > 0)
        {
            indexedLayers = incomingQueue.ToList();
            layerQueue = new Queue<BaseLayer>(indexedLayers);
        }
        else if (lm != null && lm.Layers != null && lm.Layers.Count > 0)
        {
            indexedLayers = new List<BaseLayer>(lm.Layers);
            layerQueue = new Queue<BaseLayer>(indexedLayers);
        }
        else
        {
            indexedLayers.Clear();
            var found = GetComponentsInChildren<BaseLayer>(true)
                .OrderBy(l => l.transform.GetSiblingIndex())
                .ToList();
            indexedLayers.AddRange(found);
            layerQueue = new Queue<BaseLayer>(indexedLayers);
        }

        int count = indexedLayers.Count;

        // [FIX 2] Clamp một lần ở đây — KHÔNG cộng thêm 1
        preViewMin = Mathf.Clamp(preViewMin, 0, Math.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        if (lm != null) lm.visibilityController = this;

        Debug.Log($"[VisCtrl.Init] layers:{count} | min:{preViewMin} max:{previewMax} reMax:{rePreviewMax}");

        ApplyLayerVisibility();
    }

    // ──────────────────────────────────────────────────────────────
    // APPLY VISIBILITY
    // ──────────────────────────────────────────────────────────────

    internal void ApplyLayerVisibility()
    {
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();

        var lm = GetComponentInParent<LayerManager>();
        int count = layers.Count;

        // [FIX 2] KHÔNG cộng thêm 1 vào previewMax — đây là bug gốc
        // Bug cũ: previewMax = Mathf.Clamp(previewMax + 1, ...) → window trôi mỗi lần ApplyLayerVisibility() gọi
        preViewMin = Mathf.Clamp(preViewMin, 0, Math.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            if (layer == null) continue; // null slot = đã popped, skip

            if (IsFullyVisibleIndex(i)) SetLayerFullyVisible(layer, i, lm);
            else if (IsPrereviewIndex(i)) SetLayerPrereview(layer, i, lm);
            else if (IsHiddenIndex(i)) SetLayerHidden(layer, i, lm);
        }
    }

    // ──────────────────────────────────────────────────────────────
    // RANGE HELPERS
    // ──────────────────────────────────────────────────────────────

    private bool IsFullyVisibleIndex(int i) => i >= preViewMin && i < previewMax;
    private bool IsPrereviewIndex(int i) => i >= previewMax && i < rePreviewMax;
    private bool IsHiddenIndex(int i) => i >= rePreviewMax;

    // ──────────────────────────────────────────────────────────────
    // LAYER STATE HANDLERS
    // ──────────────────────────────────────────────────────────────

    private void SetLayerFullyVisible(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf) go.SetActive(true);

        var partsToFade = (layer.parts != null && layer.parts.Count > 0)
            ? layer.parts
            : new List<BasePart>();

        if (!gameObject.activeSelf)
        {
            LayerUtils.ActiveObjectInLayer(true, layer, lm);
            return;
        }

        StartCoroutine(ActivateAfterFade(partsToFade, layer, index, lm));
    }

    private IEnumerator ActivateAfterFade(List<BasePart> partsToFade, BaseLayer layer, int index, LayerManager lm)
    {
        if (partsToFade == null || partsToFade.Count == 0)
        {
            LayerUtils.ActiveObjectInLayer(true, layer, lm);
            yield break;
        }

        int remaining = partsToFade.Count;
        Color targetColor = Color.white;
        targetColor.a = 1f;

        foreach (var part in partsToFade)
        {
            if (part == null || part.Renderer == null) { remaining--; continue; }
            StartCoroutine(FadePartAndNotify(part.Renderer, part.Outline, targetColor, fadeDuration, () => remaining--));
        }

        while (remaining > 0) yield return null;

        LayerUtils.ActiveObjectInLayer(true, layer, lm);
    }

    private IEnumerator FadePartAndNotify(Renderer renderer, Renderer outline, Color originalColor, float duration, Action onDone)
    {
        yield return StartCoroutine(FadeToOriginalColor(renderer, outline, originalColor, duration));
        onDone?.Invoke();
    }

    private void SetLayerPrereview(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf) go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part?.Renderer == null) continue;
            StartCoroutine(FadeToGray(part.Renderer, fadeDuration));
        }

        LayerUtils.ActiveObjectInLayer(true, layer, lm);
    }

    private void SetLayerHidden(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (go.activeSelf) go.SetActive(false);

        LayerUtils.ActiveObjectInLayer(false, layer, lm);
        PreviewHiddenLayer(index);
    }

    // ──────────────────────────────────────────────────────────────
    // SHOW NEXT / HIDE TOP
    // ──────────────────────────────────────────────────────────────

    internal void ShowNextLayer()
    {

        Debug.Log("Next Layer called. Current preViewMin: " + preViewMin);  
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();
        int count = layers.Count;

        if (previewMax >= count) return;

        int visibleWidth = Mathf.Max(1, previewMax - preViewMin);
        int prereviewWidth = Mathf.Max(0, rePreviewMax - previewMax);

        int nextIndex = FindNextActiveIndex(layers, preViewMin + 1);
        preViewMin = nextIndex >= 0
            ? nextIndex
            : Mathf.Clamp(preViewMin, 0, Math.Max(0, count - 1));

        previewMax = Mathf.Clamp(preViewMin + visibleWidth, preViewMin, count);
        rePreviewMax = Mathf.Clamp(previewMax + prereviewWidth, previewMax, count);

        ApplyLayerVisibility();
    }

    /// <summary>
    /// [FIX 3] HideTopLayer — ẩn layer đang visible nhất (preViewMin).
    /// Dùng khi cần force-hide layer hiện tại (debug, item effect...).
    /// </summary>
    internal void HideTopLayer()
    {
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();

        if (preViewMin < 0 || preViewMin >= layers.Count) return;

        var layer = layers[preViewMin];
        if (layer == null) return;

        var lm = GetComponentInParent<LayerManager>();
        SetLayerHidden(layer, preViewMin, lm);

        Debug.Log($"[VisCtrl] HideTopLayer → hiding layer at index {preViewMin}");
    }

    // ──────────────────────────────────────────────────────────────
    // POP LAYER (board drop)
    // ──────────────────────────────────────────────────────────────

    internal void PopLayer(BaseLayer clearedLayer)
    {
        if (clearedLayer == null || indexedLayers == null)
        {
            Debug.LogWarning("[PopLayer] clearedLayer or indexedLayers is null");
            return;
        }

        int index = indexedLayers.IndexOf(clearedLayer);
        if (index < 0)
        {
            Debug.LogWarning($"[PopLayer] Layer not found: {clearedLayer.name}");
            return;
        }

        // Hide + null slot (giữ index để không shift các layer khác)
        if (clearedLayer.gameObject != null && clearedLayer.gameObject.activeSelf)
            clearedLayer.gameObject.SetActive(false);

        indexedLayers[index] = null;

        // Advance preViewMin nếu cần
        if (index == preViewMin)
        {
            int nextActive = FindNextActiveIndex(indexedLayers, index + 1);
            if (nextActive >= 0)
                preViewMin = nextActive;
            else
            {
                int firstActive = FindFirstActiveIndex(indexedLayers);
                preViewMin = firstActive >= 0 ? firstActive : Mathf.Clamp(preViewMin, 0, indexedLayers.Count);
            }
        }

        int count = indexedLayers.Count;
        preViewMin = Mathf.Clamp(preViewMin, 0, Math.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        ApplyLayerVisibility();
    }

    // ──────────────────────────────────────────────────────────────
    // PREVIEW HIDDEN LAYERS
    // ──────────────────────────────────────────────────────────────

    public void PreviewHiddenLayer(int layerIndex)
    {
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();

        if (layerIndex < 0 || layerIndex >= layers.Count) return;

        var layer = layers[layerIndex];
        if (layer == null) return;

        var go = layer.GameObject;
        if (!go.activeSelf) go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part?.Renderer != null)
                StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
        }
    }

    public void PreviewHiddenRange(int startIndex, int endIndex)
    {
        var layers = (indexedLayers != null && indexedLayers.Count > 0)
            ? indexedLayers
            : layerQueue.ToList();

        startIndex = Mathf.Max(0, startIndex);
        endIndex = Mathf.Min(layers.Count, endIndex);

        for (int i = startIndex; i < endIndex; i++)
        {
            var layer = layers[i];
            if (layer == null) continue;

            var go = layer.GameObject;
            if (!go.activeSelf) go.SetActive(true);

            foreach (var part in layer.parts)
            {
                if (part?.Renderer != null)
                    StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // INDEX HELPERS
    // ──────────────────────────────────────────────────────────────

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

    private int FindFirstActiveIndex(List<BaseLayer> layers) => FindNextActiveIndex(layers, 0);

    // ──────────────────────────────────────────────────────────────
    // FADE COROUTINES
    // ──────────────────────────────────────────────────────────────

    IEnumerator FadeToGray(Renderer renderer, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = renderer.material.color;
        Color targetColor = startColor;
        targetColor.a = 0.8f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            renderer.material.color = Color.Lerp(Color.clear, targetColor, t / duration);
            yield return null;
        }
        renderer.material.color = targetColor;
    }

    IEnumerator FadeToOriginalColor(Renderer renderer, Renderer outline, Color originalColor, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = renderer.material.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            renderer.material.color = Color.Lerp(startColor, originalColor, p);
            if (outline != null) outline.material.color = Color.Lerp(startColor, originalColor, p);
            yield return null;
        }
        renderer.material.color = originalColor;
        if (outline != null) outline.material.color = originalColor;
    }

    IEnumerator FadeToBlack(Renderer renderer, Renderer outline, float duration)
    {
        if (renderer == null) yield break;
        Color startColor = ColorEnum.Brown.ToColor(); startColor.a = 0f;
        Color targetColor = ColorEnum.Brown.ToColor(); targetColor.a = 1f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            renderer.material.color = Color.Lerp(startColor, targetColor, p);
            if (outline != null) outline.material.color = Color.Lerp(startColor, targetColor, p);
            yield return null;
        }
        renderer.material.color = targetColor;
        if (outline != null) outline.material.color = targetColor;
    }
}
