

using Managers;
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
}