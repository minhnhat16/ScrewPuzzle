

using Managers;
using UnityEngine;

public class ItemController : FSMSystem 
{
    [SerializeField]
    private bool isHandlingItem;
    public bool IsHandlingItem { get => isHandlingItem; internal set => isHandlingItem = value; }

    public static ItemController ins;

    public void Awake()
    {
        ins = this;
    }
    public AddBoxItem AddBoxItem { get; private set; }
    public AddOneHold AddOneHold { get; private set; }
    public ClearArrayState ClearArrayState { get; private set; }
    public RemovePartState RemovePartState { get; private set; }
}