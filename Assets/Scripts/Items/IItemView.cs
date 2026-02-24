using System;
using UnityEngine;

public interface IItemView
{
    void PlayItemEffect(ItemType type, Vector3 startPos, Vector3 targetPos, Action onComplete = null);
}