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
    public List<BaseLayer> indexedLayers = new();

    [Header("Layer Range Settings")]
    [Tooltip("Số layer fully visible cố định. Nếu > 0 thì dùng giá trị này, bỏ qua ratio.")]
    [SerializeField] private int visibleWindowSize = 0;

    [Tooltip("Tỷ lệ layer fully visible = ceil(totalLayers / divisor).\n" +
             "Chỉ dùng khi visibleWindowSize = 0.\n" +
             "VD: divisor=2 → 7 layers → 4 visible, 20 layers → 10 visible.\n" +
             "    divisor=3 → 7 layers → 3 visible, 20 layers → 7 visible.")]
    [SerializeField][Range(1f, 10f)] private float visibleDivisor = 2f;

    [Tooltip("Số layer prereview (gray) hiển thị sau vùng visible.")]
    [SerializeField] private int prereviewWindowSize = 6;

    [Tooltip("Số layer hidden được preview (FadeToBlack) ngay sau prereview. " +
             "Giới hạn để tránh spawn quá nhiều coroutine khi level có nhiều layer.")]
    [SerializeField] private int hiddenPreviewCount = 2;

    // Runtime — tính lại từ window sizes mỗi lần Init/ShowNext
    private int previewMax;
    private int rePreviewMax;
    private int preViewMin;

    public float fadeDuration = 0.5f;

    public int RePreviewMax { get => rePreviewMax; set => rePreviewMax = value; }
    public int PreViewMin { get => preViewMin; set => preViewMin = value; }

    // ──────────────────────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Tính số layer fully visible thực tế:
    ///   - visibleWindowSize > 0 → dùng giá trị cố định
    ///   - visibleWindowSize = 0 → ceil(count / visibleDivisor), tối thiểu 1
    /// </summary>
    private int CalcEffectiveVisible(int count)
    {
        if (visibleWindowSize > 0)
            return Mathf.Clamp(visibleWindowSize, 1, count);

        int calculated = Mathf.CeilToInt(count / Mathf.Max(1f, visibleDivisor));
        return Mathf.Clamp(calculated, 1, count);
    }

    /// <summary>
    /// [FIX 2] Scan forward từ startMin, đếm đủ `needed` non-null slot.
    /// Trả về exclusive-end index cho previewMax — chính xác bất kể có bao nhiêu null slot bên trong.
    /// Thay thế cách đếm nullInWindow cũ (tính trên window cũ → over-expand).
    /// </summary>
    private int CalcPreviewMaxAfterPop(int startMin, int needed, int count)
    {
        int found = 0;
        int i = startMin;
        while (i < count && found < needed)
        {
            if (indexedLayers[i] != null) found++;
            i++;
        }
        return i; // exclusive end, khớp với IsFullyVisibleIndex: i >= preViewMin && i < previewMax
    }

    // ──────────────────────────────────────────────────────────────
    // INIT
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Init với optional incoming queue.
    /// Priority: incomingQueue → LayerManager.Layers → scan children.
    /// </summary>
    public void Init(Queue<BaseLayer> incomingQueue = null)
    {
        indexedLayers ??= new List<BaseLayer>();

        var lm = GetComponentInParent<LayerManager>();
        if (lm == null)
        {
            Debug.LogWarning("[LayerVisibilityController] No LayerManager found in parent hierarchy.");
            return;
        }

        if (incomingQueue != null && incomingQueue.Count > 0)
        {
            indexedLayers = incomingQueue.ToList();
            layerQueue = new Queue<BaseLayer>(indexedLayers);
        }
        else if (lm.Layers != null && lm.Layers.Count > 0)
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
        int effectiveVisible = CalcEffectiveVisible(count);

        preViewMin = 0;
        previewMax = Mathf.Clamp(preViewMin + effectiveVisible, preViewMin, count);
        rePreviewMax = Mathf.Clamp(previewMax + prereviewWindowSize, previewMax, count);

        lm.visibilityController = this;

        Debug.Log($"[VisCtrl.Init] layers:{count} | divisor:{visibleDivisor} | effectiveVisible:{effectiveVisible} | min:{preViewMin} max:{previewMax} reMax:{rePreviewMax}");

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

        preViewMin = Mathf.Clamp(preViewMin, 0, Mathf.Max(0, count));
        previewMax = Mathf.Clamp(previewMax, preViewMin, count);
        rePreviewMax = Mathf.Clamp(rePreviewMax, previewMax, count);

        for (int i = 0; i < layers.Count; i++)
        {
            var layer = layers[i];
            if (layer == null) continue; // null slot = đã popped, skip

            if (IsFullyVisibleIndex(i))
            {
                Debug.Log($"[VisCtrl] Applying FullyVisible to layer index {i}");
                SetLayerFullyVisible(layer, i, lm);
            }
            else if (IsPrereviewIndex(i))
            {
                Debug.Log("[VisCtrl] Applying Prereview to layer index " + i);
                SetLayerPrereview(layer, i, lm);
            }
            else if (IsHiddenIndex(i))
            {
                Debug.Log("[VisCtrl] Applying Hidden to layer index " + i);
                SetLayerHidden(layer, i, lm);
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // POP LAYER (board drop / breaker clear)
    // ──────────────────────────────────────────────────────────────

    internal void PopLayer(BaseLayer clearedLayer)
    {
        if (clearedLayer == null || indexedLayers == null || !clearedLayer.isActiveAndEnabled)
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

        int count = indexedLayers.Count;
        int effectiveVisible = CalcEffectiveVisible(count);

        // [FIX 3] FindNextActiveIndex chỉ check null — không dùng activeInHierarchy
        int nextActive = FindNextActiveIndex(indexedLayers, 0);
        preViewMin = nextActive >= 0 ? nextActive : 0;

        // [FIX 2] Dùng CalcPreviewMaxAfterPop thay nullInWindow — tính đúng window sau pop
        previewMax = CalcPreviewMaxAfterPop(preViewMin, effectiveVisible, count);
        rePreviewMax = Mathf.Clamp(previewMax + prereviewWindowSize, previewMax, count);

        Debug.Log($"[VisCtrl] PopLayer index={index} → min:{preViewMin} max:{previewMax} reMax:{rePreviewMax}");
        ApplyLayerVisibility();
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
        Debug.Log("[VisCtrl] SetLayerFullyVisible: activating layer at index " + index);
        layer.IsHidden = false;
        var partsToFade = new List<BasePart>();

        foreach (var part in layer.parts)
        {
            if (part == null) continue;
            part.IsBreakableByItem = true;

            SetScrewsInteractable(part, lm, enable: true);
            if (part.CurrentVisibilityState != BasePart.VisibilityState.FullyVisible)
            {
                part.CurrentVisibilityState = BasePart.VisibilityState.FullyVisible;
                if (part.Renderer != null)
                    partsToFade.Add(part);
            }
        }

        if (!gameObject.activeSelf)
        {
            ActivateScrewsInLayer(layer, lm);
            return;
        }

        if (partsToFade.Count == 0)
        {
            ActivateScrewsInLayer(layer, lm);
            return;
        }

        StartCoroutine(ActivateAfterFade(partsToFade, layer, index, lm));
    }

    private IEnumerator ActivateAfterFade(List<BasePart> partsToFade, BaseLayer layer, int index, LayerManager lm)
    {
        Debug.Log($"[VisCtrl] Activating screws after fade for layer index {index}. Parts to fade: {partsToFade.Count}");
        if (partsToFade == null || partsToFade.Count == 0)
        {
            ActivateScrewsInLayer(layer, lm);
            yield break;
        }

        int remaining = partsToFade.Count;
        Color targetColor = Color.white;
        targetColor.a = 1f;

        foreach (var part in partsToFade)
        {
            if (part == null || part.Renderer == null) { remaining--; continue; }
            StartCoroutine(FadePartAndNotify(part.Renderer, part.Outline, targetColor, fadeDuration, () =>
            {
                part.UnfreezeBody();
                remaining--;
            }));
        }

        while (remaining > 0) yield return null;

        ActivateScrewsInLayer(layer, lm);
    }

    /// <summary>
    /// Activate screws theo BodyConnect của từng part trong layer —
    /// tránh mismatch giữa Layers list index và screwDict key (sortingOrder).
    /// </summary>
    private void ActivateScrewsInLayer(BaseLayer layer, LayerManager lm)
    {
        if (layer == null || lm == null) return;

        foreach (var part in layer.parts)
        {
            if (part == null || part.Body == null) continue;

            var body = part.Body;
            foreach (var kvp in lm.screwDict)
            {
                if (kvp.Value == null) continue;
                foreach (var screw in kvp.Value)
                {
                    if (screw == null) continue;

                    // If screw has been taken into hold/box, do not re-enable or re-parent it
                    if (screw.IsInHold || screw.IsActionComplete) continue;

                    if (screw.hingeController?.BodyConnect != body) continue;

                    if (!body.gameObject.activeSelf)
                    {
                        screw.gameObject.SetActive(false);
                        continue;
                    }

                    screw.gameObject.SetActive(true);
                }
            }
        }
    }

    private IEnumerator FadePartAndNotify(Renderer renderer, Renderer outline, Color originalColor, float duration, Action onDone)
    {
        yield return StartCoroutine(FadeToOriginalColor(renderer, outline, originalColor, duration));
        Debug.Log($"[VisCtrl] Fade to original color completed for part. Renderer: {renderer}, Outline: {outline}");
        onDone?.Invoke();
    }

    private void SetLayerPrereview(BaseLayer layer, int index, LayerManager lm)
    {
        var go = layer.GameObject;
        if (!go.activeSelf) go.SetActive(true);

        foreach (var part in layer.parts)
        {
            if (part == null) continue;
            part.IsBreakableByItem = false;
            SetScrewsInteractable(part, lm, enable: false);

            if (part.CurrentVisibilityState != BasePart.VisibilityState.Prereview)
            {
                part.CurrentVisibilityState = BasePart.VisibilityState.Prereview;
                if (part.Renderer != null)
                    StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
            }
        }
        // Prereview: keep screws active (visible) but non-interactable.
        LayerUtils.ActiveObjectInLayer(false, layer, lm);
    }

    private void SetLayerHidden(BaseLayer layer, int index, LayerManager lm)
    {
        // [FIX 1] Guard đúng: bảo vệ bằng index range thay vì IsLayerClear.
        // IsLayerClear = "đã clear xong", không phản ánh "đang trong visible window".
        // Nếu layer đang fully visible, ApplyLayerVisibility() sẽ không gọi SetLayerHidden()
        // vì IsFullyVisibleIndex(i) == true → nhánh này không bao giờ reach được với active layer.
        // Guard dưới là safety net phòng trường hợp gọi trực tiếp từ bên ngoài.
        if (IsFullyVisibleIndex(index)) return;

        var go = layer.GameObject;
        if (go.activeSelf) go.SetActive(false);

        foreach (var part in layer.parts)
        {
            if (part == null) continue;
            part.IsBreakableByItem = false;
            part.CurrentVisibilityState = BasePart.VisibilityState.Hidden;
            part.FreezeBody();
            SetScrewsInteractable(part, lm, enable: false);
        }

        LayerUtils.ActiveObjectInLayer(false, layer, lm);

        if (index < rePreviewMax + hiddenPreviewCount)
            PreviewHiddenLayer(index);
    }

    /// <summary>
    /// Enable/disable collider của tất cả ScrewController gắn với các part trong layer.
    /// Ngăn player click screw từ layer chưa được reveal.
    /// </summary>
    private void SetScrewsInteractable(BasePart part, LayerManager lm, bool enable)
    {
        if (lm == null || part == null) return;

        var body = part.Body;
        if (body == null) return;

        foreach (var kvp in lm.screwDict)
        {
            if (kvp.Value == null) continue;
            foreach (var screw in kvp.Value)
            {
                if (screw == null) continue;

                // Skip screws that have been taken into a hold/box
                if (screw.IsInHold) continue;

                if (screw.hingeController.BodyConnect != body) continue;
                screw.EnableColliderAndRig(enable);
            }
        }
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

        // [FIX 3] FindNextActiveIndex chỉ check null — không dùng activeInHierarchy
        int nextIndex = FindNextActiveIndex(layers, preViewMin + 1);

        // [FIX 4] Early-return nếu không còn layer hợp lệ phía sau
        if (nextIndex < 0) return;

        preViewMin = nextIndex;

        // [FIX 4] Dùng CalcPreviewMaxAfterPop để advance đúng visibleWidth non-null slot
        previewMax = CalcPreviewMaxAfterPop(preViewMin, visibleWidth, count);
        rePreviewMax = Mathf.Clamp(previewMax + prereviewWidth, previewMax, count);

        Debug.Log($"[VisCtrl] ShowNextLayer → min:{preViewMin} max:{previewMax} reMax:{rePreviewMax}");
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
            if (part != null && part.Renderer != null)
                StartCoroutine(FadeToBlack(part.Renderer, part.Outline, fadeDuration));
        }
    }

    // ──────────────────────────────────────────────────────────────
    // INDEX HELPERS
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// [FIX 3] Chỉ check null slot — không dùng activeInHierarchy.
    /// activeInHierarchy=false khi parent bị tắt, gây preViewMin nhảy sai
    /// vào layer prereview vẫn còn hợp lệ.
    /// </summary>
    private int FindNextActiveIndex(List<BaseLayer> layers, int startAt)
    {
        if (layers == null) return -1;
        for (int i = Mathf.Max(0, startAt); i < layers.Count; i++)
        {
            if (layers[i] != null) return i;
        }
        return -1;
    }

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