using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : FSMSystem, IItemView
{
    [SerializeField] private SkeletonAnimation skeleton;

    private Tween moveTween;
    private Tween fadeTween;
    private Tween scaleTween;

    readonly Dictionary<ItemType, SoundManager.SFX> itemSoundDict =
        new()
        {
            { ItemType.Magnet, SoundManager.SFX.Magnet },
            { ItemType.Breaker, SoundManager.SFX.Breaker },
            { ItemType.Drill, SoundManager.SFX.Drill },
            { ItemType.AddBox, SoundManager.SFX.AddBox },
            { ItemType.Gold, SoundManager.SFX.GoldCollect },
            { ItemType.Ticket, SoundManager.SFX.TicketCollect },
        };

    public void PlayItemEffect(
        ItemType type,
        Vector3 startPos,
        Vector3 targetPos,
        Action onComplete = null)
    {
        if (skeleton == null) return;

        skeleton.gameObject.SetActive(true);

        string animName = GetAnimName(type);
        if (string.IsNullOrEmpty(animName)) return;

        KillTweens();

        SetupTransform(type, startPos);

        skeleton.AnimationState
            .SetAnimation(0, animName, false)
            .Complete += _ =>
            {
                skeleton.gameObject.SetActive(false);
                onComplete?.Invoke();
            };

        PlayTweens(type, targetPos);
    }

    private void KillTweens()
    {
        moveTween?.Kill();
        fadeTween?.Kill();
        scaleTween?.Kill();
    }

    private void SetupTransform(ItemType type, Vector3 startPos)
    {
        float zRot = type == ItemType.Magnet ? 90f : 0f;
        skeleton.transform.rotation = Quaternion.Euler(0, 0, zRot);

        Transform t = skeleton.transform;
        t.position = startPos;
        t.localScale = Vector3.zero;

        var ske = skeleton.Skeleton;
        ske.A = 0f;
    }

    private void PlayTweens(ItemType type, Vector3 targetPos)
    {
        Transform t = skeleton.transform;
        var ske = skeleton.Skeleton;

        scaleTween = t
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack);

        fadeTween = DOTween.To(
            () => ske.A,
            a => ske.A = a,
            1f,
            0.25f);

        moveTween = t
            .DOMove(targetPos, 0.6f)
            .SetEase(Ease.InOutCubic)
            .OnComplete(() => PlaySound(type));
    }

    private void PlaySound(ItemType type)
    {
        var sfx = itemSoundDict.GetValueOrDefault(type, SoundManager.SFX.Button);
        SoundHelper.PlaySFX(sfx);
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