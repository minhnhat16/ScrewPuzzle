using Coffee.UIEffects;
using UnityEngine;

public class SpecialChestItem : QuestChestItem
{
    [SerializeField]
    private UIEffect questParticle;


    public override void Awake()
    {
        base.Awake();
        questParticle = GetComponentInChildren<UIEffect>();
    }
    public override void Setup(QuestChestParam param)
    {
        base.Setup(param);
        Debug.Log("[SpecialChestItem] Special setup param is null" + (param == null));
    }

}
