using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Map từng ItemType sang tên sprite (string) để lookup qua SpriteLibControl.
/// Assign trong Inspector ở Header "Item Sprite Map".
/// </summary>
[Serializable]
public struct ItemSpriteEntry
{
    public ItemType itemType;
    [Tooltip("Tên sprite trong SpriteLib — giống với tên dùng ở SpriteLibControl.GetSprite()")]
    public int Id;
}

/// <summary>
/// Chạy animation theo loại reward:
///   Currency (Gold/Ticket) → icon bay về đúng HUD anchor
///   Item                   → icon pop-up giữa màn hình theo UX mới
/// </summary>
public class RewardAnimationService : MonoBehaviour
{
    [Header("Fly-to-HUD (Currency)")]
    [SerializeField] private GameObject flyIconPrefab;   // Image + CanvasGroup
    [SerializeField] private RectTransform flyOrigin;    // điểm xuất phát (vị trí claim/buy)
    [SerializeField] private RectTransform goldTarget;   // HUD gold anchor
    [SerializeField] private RectTransform ticketTarget; // HUD ticket anchor
    [SerializeField][Range(1, 10)] private int maxTicketFlyCount = 5;
    [SerializeField] private float flyDuration = 0.6f;
    [SerializeField] private float flySpawnDelay = 0.08f;
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite ticketSprite;
    [SerializeField] private Vector3 goldFlyScale = Vector3.one;
    [SerializeField] private Vector3 ticketFlyScale = new Vector3(0.85f, 0.85f, 1f);

    [Header("Item Sprite Map")]
    [Tooltip("Map ItemType → Sprite cho Drill, Breaker, Magnet... Drag sprite tương ứng vào đây.")]
    [SerializeField] private List<ItemSpriteEntry> itemSpriteMap = new();

    // ── Public API để lấy sprite theo ItemType ──────────────────────
    /// <summary>
    /// Trả về sprite phù hợp cho từng loại reward:
    ///   Gold   → goldSprite
    ///   Ticket → ticketSprite
    ///   Khác   → tra trong itemSpriteMap (Drill, Breaker, Magnet...)
    ///   Không tìm thấy → null (caller tự fallback)
    /// </summary>
    public Sprite GetSpriteForReward(ItemType itemType)
    {
        if (itemType == ItemType.Gold) return goldSprite;
        if (itemType == ItemType.Ticket) return ticketSprite;

        // 1) Try configured map (explicit sprite name)
        ItemSpriteEntry entry = itemSpriteMap.Find(e => e.itemType == itemType);
    
        try { return SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, entry.itemType.ToString()); }
        catch { /* ignore if not available yet */ }

        // 2) Try enum name (e.g. "Drill") — matches previous fallback
        try
        {
            var byName = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, itemType.ToString());
            if (byName != null) return byName;
        }
        catch { /* ignore */ }

        // 3) Try numeric id string — matches ShopItem usage of item.Id.ToString()
        try
        {
            var byId = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, ((int)itemType).ToString());
            if (byId != null) return byId;
        }
        catch { /* ignore */ }

        return null;
    }

    // Legacy accessors (giữ backward compat)
    public Sprite GoldSprite => goldSprite;
    public Sprite TicketSprite => ticketSprite;

    [Header("Item Pop-Up (Center Screen)")]
    // Vẫn giữ tên biến cũ (itemDropPrefab, v.v...) để Inspector không bị mất Reference đã kéo vào
    [SerializeField] private GameObject itemDropPrefab;  // Prefab có Image + CanvasGroup + Text
    [SerializeField] private Text itemDropAmountText;    // Text hiển thị số lượng "x{amount}" gắn trên prefab
    [SerializeField] private float itemDropDuration = 1.3f; // Thời gian hiển thị (pop-up rồi bay mờ đi)

    private Canvas _rootCanvas;
    [SerializeField] private float itemPopupGap = 0.18f;
    private readonly Queue<RewardResult> _pendingItemRewards = new();
    private Coroutine _itemRewardQueueCoroutine;

    // ─── Lifecycle ─────────────────────────────────────────────────

    private void Awake()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable() => RewardEvents.OnRewardGranted += HandleReward;
    private void OnDisable()
    {
        RewardEvents.OnRewardGranted -= HandleReward;

        if (_itemRewardQueueCoroutine != null)
        {
            StopCoroutine(_itemRewardQueueCoroutine);
            _itemRewardQueueCoroutine = null;
        }

        _pendingItemRewards.Clear();
    }

    // ─── Dispatch ──────────────────────────────────────────────────

    private void HandleReward(RewardResult reward)
    {
        if (reward.Kind == RewardKind.Currency)
            StartCoroutine(PlayFlyAnimation(reward));
        else
            EnqueueItemReward(reward);
    }

    // ─── Animation: Fly-to-HUD (Gold / Ticket) ─────────────────────

    /// <summary>
    /// Gọi trước khi fire event để thay điểm xuất phát (ví dụ: nút claim của từng màn).
    /// </summary>
    public void SetFlyOrigin(RectTransform origin) => flyOrigin = origin;

    /// <summary>
    /// Trả về world position của HUD anchor theo loại item.
    /// </summary>
    public Vector3 GetHUDTargetPosition(ItemType itemType)
    {
        var target = itemType == ItemType.Ticket ? ticketTarget : goldTarget;
        return target != null ? target.position : Vector3.zero;
    }

    private IEnumerator PlayFlyAnimation(RewardResult reward)
    {
        if (flyIconPrefab == null || flyOrigin == null) yield break;

        // Chọn đúng anchor: gold → goldTarget, ticket → ticketTarget
        var target = reward.ItemType == ItemType.Ticket ? ticketTarget : goldTarget;
        if (target == null) yield break;

        // Currency phải luôn dùng icon cố định theo wallet type.
        Sprite sprite = reward.ItemType == ItemType.Ticket ? ticketSprite : goldSprite;
        if (sprite == null)
            sprite = GetSpriteForReward(reward.ItemType);

        // SFX phân biệt Gold vs Ticket khi đến đích
        var collectSfx = reward.ItemType == ItemType.Ticket
            ? SoundManager.SFX.TicketCollect
            : SoundManager.SFX.GoldCollect;

        bool isTicket = reward.ItemType == ItemType.Ticket;
        int spawnCount = isTicket
            ? Mathf.Clamp(reward.Amount, 1, maxTicketFlyCount)
            : 1;

        Vector3 iconScale = isTicket ? ticketFlyScale : goldFlyScale;
        string amountText = isTicket ? null : $"x{reward.Amount}";

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnFlyIcon(sprite, flyOrigin.position, target, collectSfx, iconScale, amountText);
            yield return new WaitForSeconds(flySpawnDelay);
        }
    }

    private void SpawnFlyIcon(Sprite sprite, Vector3 from, RectTransform target, SoundManager.SFX collectSfx, Vector3 iconScale, string amountText)
    {
        // Use the canvas path (preferred). Ensure rect transform anchors/pivot set to center so anchoredPosition is predictable.
        if (_rootCanvas != null)
        {
            var go = Instantiate(flyIconPrefab, _rootCanvas.transform);
            var rt = go.GetComponent<RectTransform>();
            var img = go.GetComponentInChildren<Image>();
            var txt = go.GetComponentInChildren<Text>(true);
            var cg = go.GetComponent<CanvasGroup>();

            rt.localScale = iconScale;

            if (img != null && sprite != null) img.sprite = sprite;
            if (txt != null)
            {
                bool showAmount = !string.IsNullOrEmpty(amountText);
                txt.text = amountText ?? string.Empty;
                txt.gameObject.SetActive(showAmount);
            }

            // Ensure predictable pivot/anchors for UI element
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Start by setting exact world position, Unity calculates anchoredPosition automatically
            rt.position = from;
            Vector2 localStart = rt.anchoredPosition + new Vector2(Random.Range(-20f, 20f), Random.Range(-10f, 10f));

            // To find the correct target relative coordinate, move 'rt' to the target, then get its local coords
            rt.position = target.position;
            Vector2 localTarget = rt.anchoredPosition;

            // Restore animated local position
            rt.anchoredPosition = localStart;

            // SFX: Swoosh when starting to fly
            SoundManager.instance?.PlaySFX(SoundManager.SFX.Swoosh);

            // Animate anchoredPosition from localStart -> localTarget
            StartCoroutine(FlyToCanvas(rt, cg, localTarget, collectSfx));
            return;
        }

        // Fallback: spawn in world space (keeps original behavior)
        var goFallback = Instantiate(flyIconPrefab);
        var rtFb = goFallback.GetComponent<RectTransform>();
        var imgFb = goFallback.GetComponentInChildren<Image>();
        var txtFb = goFallback.GetComponentInChildren<Text>(true);
        var cgFb = goFallback.GetComponent<CanvasGroup>();
        
        rtFb.localScale = iconScale;
        if (imgFb != null && sprite != null) imgFb.sprite = sprite;
        if (txtFb != null)
        {
            bool showAmount = !string.IsNullOrEmpty(amountText);
            txtFb.text = amountText ?? string.Empty;
            txtFb.gameObject.SetActive(showAmount);
        }
        rtFb.position = from + new Vector3(Random.Range(-20f, 20f), Random.Range(-10f, 10f), 0f);
        SoundManager.instance?.PlaySFX(SoundManager.SFX.Swoosh);
        StartCoroutine(FlyTo(rtFb, cgFb, target, collectSfx));
    }

    private IEnumerator FlyTo(RectTransform rtFb, CanvasGroup cgFb, RectTransform target, SoundManager.SFX collectSfx)
    {
        if (rtFb == null)
            yield break;

        Vector3 startPos = rtFb.position;
        Vector3 targetPos = target != null ? target.position : startPos + Vector3.up * 10;
        float elapsed = 0f;
        bool arrivedSfxPlayed = false;

        while (elapsed < flyDuration)
        {
            if (rtFb == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);
            float smooth = t * t * (3f - 2f * t); // smoothstep ease in-out

            // Interpolate world position and optional fade
            rtFb.position = Vector3.Lerp(startPos, targetPos, smooth);
            if (cgFb != null)
                cgFb.alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f;

            // Play arrival SFX once when near target
            if (!arrivedSfxPlayed && t >= 0.85f)
            {
                arrivedSfxPlayed = true;
                SoundManager.instance?.PlaySFX(collectSfx);
            }

            yield return null;
        }

        if (rtFb != null) Destroy(rtFb.gameObject);
    }

    // New coroutine that animates RectTransform.anchoredPosition in canvas local space
    private IEnumerator FlyToCanvas(RectTransform rt, CanvasGroup cg, Vector2 targetLocal, SoundManager.SFX collectSfx)
    {
        Vector2 startLocal = rt.anchoredPosition;
        float elapsed = 0f;
        bool arrivedSfxPlayed = false;

        while (elapsed < flyDuration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / flyDuration;
            float smooth = t * t * (3f - 2f * t); // smoothstep

            rt.anchoredPosition = Vector2.Lerp(startLocal, targetLocal, smooth);
            if (cg != null)
                cg.alpha = t < 0.5f ? 1f : 1f - (t - 0.5f) * 2f; // fade second half

            // SFX near arrival
            if (!arrivedSfxPlayed && t >= 0.85f)
            {
                arrivedSfxPlayed = true;
                SoundManager.instance?.PlaySFX(collectSfx);
            }

            yield return null;
        }

        if (rt != null) Destroy(rt.gameObject);
    }

    // ─── Animation: Item Pop-Up (Center Screen) ─────────────────────────

    private void EnqueueItemReward(RewardResult reward)
    {
        _pendingItemRewards.Enqueue(reward);
        if (_itemRewardQueueCoroutine == null)
            _itemRewardQueueCoroutine = StartCoroutine(ProcessItemRewardQueue());
    }

    private IEnumerator ProcessItemRewardQueue()
    {
        while (_pendingItemRewards.Count > 0)
        {
            yield return PlayItemPopAnimation(_pendingItemRewards.Dequeue());

            if (_pendingItemRewards.Count > 0 && itemPopupGap > 0f)
                yield return new WaitForSeconds(itemPopupGap);
        }

        _itemRewardQueueCoroutine = null;
    }

    private IEnumerator PlayItemPopAnimation(RewardResult reward)
    {
        if (itemDropPrefab == null) yield break;

        // Resolve sprite: prefer IconName, then itemSpriteMap, then fallback to GetSpriteForReward
        Sprite itemSprite = null;
        if (!string.IsNullOrEmpty(reward.IconName))
        {
            try { itemSprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, reward.ItemType.ToString()); }
            catch { /* ignore */ }
        }

        if (itemSprite == null)
            itemSprite = GetSpriteForReward(reward.ItemType);

        Debug.Log("PlayItemPopAnimation: " + reward.ItemType + " x" + reward.Amount + " (Sprite: " + (itemSprite != null ? itemSprite.name : "null") + ")");

        // Chỉ spawn 1 icon duy nhất và gắn số lượng vào text
        SpawnPopIcon(itemSprite, reward.Amount);
        yield return new WaitForSeconds(itemDropDuration);
    }

    /// <summary>
    /// Bật 1 icon to ngay giữa màn hình phụt ra, dừng 1 chút cho user đọc text, xong ngâm mờ và trôi lên trên.
    /// </summary>
    private void SpawnPopIcon(Sprite sprite, int amount)
    {
        Transform parentTransform = _rootCanvas != null ? _rootCanvas.transform : transform;
        var go = Instantiate(itemDropPrefab, parentTransform);
        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponentInChildren<Image>();
        var cg = go.GetComponent<CanvasGroup>();

        rt.localScale = Vector3.one;

        if (img != null && sprite != null) img.sprite = sprite;

        // Ép Anchor và Pivot ra ngay chính giữa màn hình (hoặc chính giữa Canvas Parent)
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        
        // Neo vuông góc tại tọa độ 0,0 (chính giữa)
        rt.anchoredPosition = Vector2.zero;

        // Nếu có text đi kèm trên prefab, thì truyền đúng số lượng
        if (itemDropAmountText != null) itemDropAmountText.text = $"x{amount}";

        SoundManager.instance?.PlaySFX(SoundManager.SFX.GiftItemAppear);

        StartCoroutine(PopUpAnim(rt, cg));
    }

    private IEnumerator PopUpAnim(RectTransform rt, CanvasGroup cg)
    {
        if (rt == null) yield break;

        float elapsed = 0f;

        while (elapsed < itemDropDuration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            
            // Tỷ lệ hoàn thành animation t trong khoảng [0, 1]
            float t = Mathf.Clamp01(elapsed / itemDropDuration);

            // Phase 1: Bật Scale từ 0 -> 1.2 (trong 20% thời gian đầu)
            if (t < 0.2f)
            {
                float phaseT = t / 0.2f;
                // Ease out sine cho cảm giác pop mềm mại
                float scale = Mathf.Lerp(0f, 1.2f, Mathf.Sin(phaseT * Mathf.PI / 2f));
                rt.localScale = new Vector3(scale, scale, 1f);
                if (cg != null) cg.alpha = phaseT;
                rt.anchoredPosition = Vector2.zero;
            }
            // Phase 2: Scale nẩy thu về 1.0 (trong 10% kế tiếp)
            else if (t < 0.3f)
            {
                float phaseT = (t - 0.2f) / 0.1f;
                float scale = Mathf.Lerp(1.2f, 1.0f, phaseT);
                rt.localScale = new Vector3(scale, scale, 1f);
                if (cg != null) cg.alpha = 1f;
                rt.anchoredPosition = Vector2.zero;
            }
            // Phase 3: Ngâm hình (giữ màn hình) (từ 0.3 -> 0.7)
            else if (t <= 0.7f)
            {
                rt.localScale = Vector3.one;
                rt.anchoredPosition = Vector2.zero;
                if (cg != null) cg.alpha = 1f;
            }
            // Phase 4: Biến mất (Fade out và trôi nhẹ lêm trên) (từ 0.7 -> end)
            else
            {
                float phaseT = (t - 0.7f) / 0.3f;
                rt.localScale = Vector3.one;
                // Trôi lên trên 60 pixel
                rt.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, 60f), phaseT);
                if (cg != null) cg.alpha = 1f - phaseT;
            }

            yield return null;
        }

        // Xong animation thì phá hủy icon
        if (rt != null) Destroy(rt.gameObject);
    }
}
