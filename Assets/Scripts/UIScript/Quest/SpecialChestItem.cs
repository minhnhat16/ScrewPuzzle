using Coffee.UIEffects;
using UnityEngine;

public class SpecialChestItem : QuestChestItem
{
    [SerializeField]
    private UIEffect questParticle;


    internal override void Awake()
    {
        base.Awake();
        questParticle = GetComponentInChildren<UIEffect>();
    }
}
