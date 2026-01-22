using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GiftClaimDialog : BaseDialog
{
    private bool isClaimed;
    [Header("UI")]
    public Button openButton;
    public Button claimButton;
    [SerializeField]
    private Text helperTxt;

    public Transform itemContainer;      // chứa các item spawn ra
    public GameObject itemPrefab;        // prefab icon + text

    [SerializeField]    
    private RectTransform arrow;

    [Header("Animation")]
    public SkeletonGraphic spineBox;
    public string animOpen = "open";
    public string animIdle = "idle";
    public string animOpenIdle = "open_idle";

    private UnityAction onClaim;

    // MULTI-STAGE ITEM QUEUE
    private readonly Queue<RewardItem> lootQueue = new();

    private bool boxOpened = false;
    private bool revealing = false;


    private UnityEvent<bool> onRewardNotClaim = new();

    private void OnEnable()
    {
        openButton.onClick.AddListener(OnClickOpen);
        claimButton.onClick.AddListener(OnClickClaim);
        claimButton.gameObject.SetActive(false);
        onRewardNotClaim.AddListener(ShowHelper);
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(OnClickOpen);
        claimButton.onClick.RemoveListener(OnClickClaim);
    }

    //============================================================
    //  INIT
    //============================================================

    public override void Setup(DialogParam dialogParam)
    {
        base.Setup(dialogParam);
        GiftParam param = dialogParam as GiftParam;
        var rewards = param.rewards;


        lootQueue.Clear();
        foreach (var r in rewards)
            lootQueue.Enqueue(r);
        boxOpened = false;
        claimButton.gameObject.SetActive(false);
        isClaimed = false;
        ShowHelper(false);

    }
    public void Setup(List<RewardItem> rewards, UnityAction onClaimCallback)
    {
        lootQueue.Clear();
        foreach (var r in rewards)
            lootQueue.Enqueue(r);

        onClaim = onClaimCallback;
        claimButton.gameObject.SetActive(false);
    }

    //============================================================
    //  SHOW
    //============================================================
    public override void OnStartShowDialog()
    {
        // vào dialog là idle trước khi open
        AnimationHelper.PlaySpineAnimation(spineBox, animIdle, true);

        StartCoroutine(PlayerClaimCouroutine());
    }

    private IEnumerator PlayerClaimCouroutine()
    {
        if(boxOpened || isClaimed) yield break;
        yield return new WaitForSeconds(5f);
        ShowHelper(true);
    }

    //============================================================
    //  OPEN BOX
    //============================================================
    private void OnClickOpen()
    {
        if (boxOpened) return;
        boxOpened = true;
        isClaimed = true;
        StopCoroutine(PlayerClaimCouroutine());
        // Play OPEN → OPEN_IDLE
        AnimationHelper.PlaySpineAnimation(spineBox, animOpen, false, () =>
        {
            AnimationHelper.PlaySpineAnimation(spineBox, animOpenIdle, true);
            StartCoroutine(RevealNextItem());
        });

    }

    //============================================================
    //  MULTI-STAGE ITEM REVEAL
    //============================================================
    private IEnumerator RevealNextItem()
    {
        if (revealing) yield break;

        revealing = true;
        Debug.Log("Loot queue" + lootQueue.Count);

        while (lootQueue.Count > 0)
        {
            yield return new WaitForSeconds(0.35f);

            var reward = lootQueue.Dequeue();
            var rewardItem = SpawnRewardItem(reward);
            yield return PlayPopAnimation(itemContainer.GetChild(itemContainer.childCount - 1));

            yield return new WaitForSeconds(0.25f);
            rewardItem.gameObject.SetActive(false);
        }

        revealing = false;

        // hiển thị nút claim cuối cùng
        claimButton.gameObject.SetActive(true);
    }

    private GameObject SpawnRewardItem(RewardItem reward)
    {
        var go = Instantiate(itemPrefab, itemContainer);

        var icon = go.transform.Find("Icon").GetComponent<Image>();
        var txt = go.transform.Find("Txt").GetComponent<Text>();

        icon.sprite = SpriteLibControl.Instance.GetSprite(0, SpriteGroup.UI, reward.icon_name);

        // 1) Set kích thước tự nhiên của sprite
        icon.SetNativeSize();

        // 2) Scale icon để vừa khung 100x100
        RectTransform rt = icon.rectTransform;
        float maxSize = 300f;

        float scale = Mathf.Min(
            maxSize / rt.sizeDelta.x,
            maxSize / rt.sizeDelta.y
        );

        rt.localScale = Vector3.one * scale;

        // 3) Set text
        txt.text = "x" + reward.amount;

        go.transform.localScale = Vector3.one;

        return go;
    }


    private IEnumerator PlayPopAnimation(Transform t)
    {
        float tVal = 0;
        float dur = 0.1f;

        Vector3 big = Vector3.one * 1.2f;
        Vector3 small = Vector3.one;

        // scale up
        while (tVal < dur)
        {
            tVal += Time.deltaTime;
            t.localScale = Vector3.Lerp(Vector3.zero, big, tVal / dur);
            yield return null;
        }

        // settle down
        tVal = 0;
        while (tVal < 0.1f)
        {
            tVal += Time.deltaTime;
            t.localScale = Vector3.Lerp(big, small, tVal / 0.1f);
            yield return null;
        }
    }

    //============================================================
    //  CLAIM
    //============================================================
    private void OnClickClaim()
    {
        onClaim?.Invoke();
        DialogManager.ins.HideDialog(DialogIndex.GiftClaimDialog);
    }


    private void ShowHelper(bool arg0)
    {
        if(!arg0) return;
        Debug.Log("Show helper");
        helperTxt.gameObject.SetActive(arg0);
        ArrowHelper(arg0);
    }

    private void ArrowHelper(bool active)
    {
        arrow.gameObject.SetActive(active);
    }

}


