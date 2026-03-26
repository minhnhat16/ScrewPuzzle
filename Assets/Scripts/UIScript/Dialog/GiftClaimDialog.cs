using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Spine.Unity;

public class GiftClaimDialog : BaseDialog
{
    [Header("UI")]
    public Button openButton;
    public Button claimButton;
    [SerializeField] private Text helperTxt;
    [SerializeField] private RectTransform arrow;

    [Header("Grid")]
    [Tooltip("GameObject có CenteredGridLayout component — item spawn vào đây")]
    public CenteredGridLayout gridLayout;
    public GameObject itemPrefab;

    [Header("Spine")]
    public SkeletonGraphic spineBox;
    public string animOpen = "open";
    public string animIdle = "idle";
    public string animOpenIdle = "open_idle";

    [Header("Fly Settings")]
    [Tooltip("Điểm xuất phát icon bay ra — thường là tâm hộp quà")]
    [SerializeField] private RectTransform boxCenter;
    [SerializeField] private float flyToGridDuration = 0.45f;
    [SerializeField] private float flyToHUDDelay = 0.08f;

    // ─── Runtime state ─────────────────────────────────────────────
    private readonly Queue<RewardItem> lootQueue = new();
    private readonly List<GridSlot> spawnedSlots = new(); // track để fly về HUD sau

    private UnityAction onClaim;
    private bool boxOpened = false;
    private bool isClaimed = false;
    private bool revealing = false;

    // Mỗi slot trong grid giữ reference tới item và reward tương ứng
    private struct GridSlot
    {
        public GameObject go;
        public RectTransform rt;
        public RewardItem reward;
    }

    // ─── Lifecycle ─────────────────────────────────────────────────

    private void OnEnable()
    {
        openButton.onClick.AddListener(OnClickOpen);
        claimButton.onClick.AddListener(OnClickClaim);
        claimButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(OnClickOpen);
        claimButton.onClick.RemoveListener(OnClickClaim);
        StopAllCoroutines();
    }

    // ─── Setup ─────────────────────────────────────────────────────

    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        var param = dialogParam as GiftParam;
        InitRewards(param.rewards, param.onClaim != null ? new UnityAction(param.onClaim) : null);
    }

    public void Setup(List<RewardItem> rewards, UnityAction onClaimCallback)
    {
        InitRewards(rewards, onClaimCallback);
    }

    private void InitRewards(List<RewardItem> rewards, UnityAction onClaimCallback)
    {
        lootQueue.Clear();
        spawnedSlots.Clear();

        // Xóa item cũ trong grid
        gridLayout.Clear();

        foreach (var r in rewards)
            lootQueue.Enqueue(r);

        onClaim = onClaimCallback;
        boxOpened = false;
        isClaimed = false;
        revealing = false;

        claimButton.gameObject.SetActive(false);
        ShowHelper(false);
    }

    // ─── Show ──────────────────────────────────────────────────────

    public override void OnStartShowDialog()
    {
        claimButton.interactable = true;
        AnimationHelper.PlaySpineAnimation(spineBox, animIdle, true);
        SoundHelper.PlaySFX(SoundManager.SFX.GiftBoxOpen);
        StartCoroutine(HelperHintCoroutine());
    }

    private IEnumerator HelperHintCoroutine()
    {
        yield return new WaitForSeconds(5f);
        if (!boxOpened) ShowHelper(true);
    }

    // ─── Open box ──────────────────────────────────────────────────

    private void OnClickOpen()
    {
        if (boxOpened) return;
        boxOpened = true;
        ShowHelper(false);

        AnimationHelper.PlaySpineAnimation(spineBox, animOpen, false, () =>
        {
            AnimationHelper.PlaySpineAnimation(spineBox, animOpenIdle, true);
            StartCoroutine(RevealAllItems());
        });
    }

    // ─── Reveal: item bay từ hộp vào grid ──────────────────────────

    /// <summary>
    /// Spawn tất cả item prefab vào grid (alpha=0, scale=0),
    /// sau đó lần lượt bay từ boxCenter vào đúng slot của mình.
    /// </summary>
    private IEnumerator RevealAllItems()
    {
        if (revealing) yield break;
        revealing = true;

        // 1. Spawn toàn bộ vào grid trước để layout tính toán vị trí
        var allRewards = new List<RewardItem>(lootQueue);
        lootQueue.Clear();

        foreach (var reward in allRewards)
        {
            var go = SpawnGridItem(reward, hidden: true);
            spawnedSlots.Add(new GridSlot
            {
                go = go,
                rt = go.GetComponent<RectTransform>(),
                reward = reward
            });
        }

        // 2. Tính layout — hàng cuối tự căn giữa
        gridLayout.Apply();

        // 3. Force rebuild để RectTransform cập nhật world position
        Canvas.ForceUpdateCanvases();
        yield return null; // chờ 1 frame

        // 3. Lần lượt fly từ boxCenter vào từng slot
        foreach (var slot in spawnedSlots)
        {
            StartCoroutine(FlyIntoSlot(slot));
            yield return new WaitForSeconds(0.15f); // stagger nhẹ
        }

        // 4. Chờ animation cuối cùng xong
        float waitTime = flyToGridDuration + 0.15f * spawnedSlots.Count + 0.3f;
        yield return new WaitForSeconds(waitTime);

        revealing = false;
        claimButton.gameObject.SetActive(true);
    }

    private GameObject SpawnGridItem(RewardItem reward, bool hidden)
    {
        var go = Instantiate(itemPrefab, gridLayout.transform); // ← spawn vào CenteredGridLayout
        var icon = go.transform.Find("Icon").GetComponent<Image>();
        var txt = go.transform.Find("Txt").GetComponent<Text>();

        icon.sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, reward.icon_name);
        icon.preserveAspect = true; // giữ tỉ lệ, không méo, anchor giữ nguyên full stretch

        txt.text = "x" + reward.amount;

        if (hidden)
        {
            go.transform.localScale = Vector3.zero;
            var cg = go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        return go;
    }

    /// <summary>
    /// Item bắt đầu ở vị trí boxCenter (world), bay về đúng slot trong grid,
    /// đồng thời scale + fade in.
    /// </summary>
    private IEnumerator FlyIntoSlot(GridSlot slot)
    {
        var rt = slot.rt;
        var cg = slot.go.GetComponent<CanvasGroup>();

        Vector3 targetPos = rt.position;          // vị trí thực trong grid
        Vector3 startPos = boxCenter != null
            ? boxCenter.position
            : targetPos + Vector3.up * 200f;

        float elapsed = 0f;
        SoundHelper.PlaySFX(SoundManager.SFX.GiftItemAppear);

        while (elapsed < flyToGridDuration)
        {
            if (rt == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / flyToGridDuration;
            float smooth = t * t * (3f - 2f * t); // ease in-out

            rt.position = Vector3.Lerp(startPos, targetPos, smooth);
            rt.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, smooth);
            if (cg != null) cg.alpha = smooth;

            yield return null;
        }

        rt.position = targetPos;
        rt.localScale = Vector3.one;
        if (cg != null) cg.alpha = 1f;

        // Pop nhỏ khi đáp xuống
        yield return PopBounce(rt);
    }

    private IEnumerator PopBounce(RectTransform rt)
    {
        float t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.15f, t / 0.1f);
            yield return null;
        }
        t = 0f;
        while (t < 0.1f)
        {
            t += Time.deltaTime;
            rt.localScale = Vector3.Lerp(Vector3.one * 1.15f, Vector3.one, t / 0.1f);
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    // ─── Claim: item bay về HUD ────────────────────────────────────

    private void OnClickClaim()
    {
        if (isClaimed) return;
        isClaimed = true;

        claimButton.interactable = false;
        StartCoroutine(ClaimWithRewardService());
    }

    /// <summary>
    /// Dùng cùng flow với QuestItem:
    /// fire RewardEvents ngay khi bấm Take, dialog vẫn đang mở,
    /// RewardAnimationService sẽ tự play animation từ slot hiện tại.
    /// </summary>
    private IEnumerator ClaimWithRewardService()
    {
        var rewardAnim = FindAnyObjectByType<RewardAnimationService>();

        foreach (var slot in spawnedSlots)
        {
            if (rewardAnim != null && slot.rt != null)
                rewardAnim.SetFlyOrigin(slot.rt);

            RewardEvents.Fire(
                slot.reward.itemType,
                slot.reward.amount,
                slot.reward.icon_name
            );

            if (slot.go != null)
                slot.go.SetActive(false);

            yield return new WaitForSeconds(flyToHUDDelay);
        }

        yield return new WaitForSeconds(0.15f);

        onClaim?.Invoke();
        DialogManager.ins.HideDialog(DialogIndex.GiftClaimDialog);
    }

    // ─── Helper hint ───────────────────────────────────────────────

    private void ShowHelper(bool active)
    {
        if (helperTxt != null) helperTxt.gameObject.SetActive(active);
        if (arrow != null) arrow.gameObject.SetActive(active);
    }
}
