

using DG.Tweening;
using Managers;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ItemController : FSMSystem
{
    [SerializeField]
    private bool isHandlingHammer;
    public bool IsHandlingHammer { get => isHandlingHammer; internal set => isHandlingHammer = value; }

    public static ItemController ins;
    [SerializeField]
    private SkeletonAnimation skeleton;
    public Vector3 targetPos = Vector3.zero;
    public UnityEvent<bool> itemPerformed = new();

    public void Awake()
    {
        ins = this;

        AddBoxItem = new AddBoxItem(this);
        AddOneHold = new AddOneHold(this);
        ClearArrayState = new ClearArrayState(this);
        RemovePartState = new RemovePartState(this);
        IdleItemState = new IdleItemState(this);
        GotoState(IdleItemState); ;
    }
    public AddBoxItem AddBoxItem { get; private set; }
    public AddOneHold AddOneHold { get; private set; }
    public ClearArrayState ClearArrayState { get; private set; }
    public RemovePartState RemovePartState { get; private set; }
    public IdleItemState IdleItemState { get; private set; }



    public void WaitFor(float time, System.Action callback)
    {
        StartCoroutine(WaitForSeconds(time, callback));
    }

    private IEnumerator WaitForSeconds(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        IsHandlingHammer = false;
        callback?.Invoke();
    }


    private Tween moveTween;
    private Tween fadeTween;
    private Tween scaleTween;

    internal void PlaySkeAnimOnTarget(
        ItemType type,
        Vector3 startPos,
        Vector3 targetPos,
        Action callback = null
    )
    {
        if (skeleton == null) return;
        skeleton.gameObject.SetActive(true);

        string animName = GetAnimName(type);
        if (string.IsNullOrEmpty(animName)) return;

        // Kill tween cũ
        moveTween?.Kill();
        fadeTween?.Kill();
        scaleTween?.Kill();



        float zRot = type != ItemType.Magnet ? 0f : 90f;
        skeleton.transform.rotation = Quaternion.Euler(0, 0, zRot);
        // Reset transform
        Transform t = skeleton.transform;
        t.position = startPos;
        t.localScale = Vector3.zero;

        var ske = skeleton.Skeleton;
        ske.A = 0f;

        skeleton.AnimationState
                     .SetAnimation(0, animName, false)
                     .Complete += _ =>
                     {
                         skeleton.gameObject.SetActive(false);
                         callback?.Invoke();
                     };


        scaleTween = t
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack);

        fadeTween = DOTween.To(
            () => ske.A,
            a => ske.A = a,
            1f,
            0.25f
        );

        // 🚀 Move tới target
        moveTween = t
            .DOMove(targetPos, 0.6f)
            .SetEase(Ease.InOutCubic).OnComplete(() =>
            {

            });


    }



    private string GetAnimName(ItemType type)
    {
        return type switch
        {
            ItemType.Magnet => "anim_hut",
            ItemType.Breaker => "anim_bua",
            ItemType.Drill => "anim_khoan",
            ItemType.AddBox => "add_box",
            ItemType.Gold => "gold",
            ItemType.Ticket => "ticket",
            _ => null
        };
    }
}