using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public class AddHoldItem : Button
{
    private Button _button;
    private ItemType _itemType;
    protected override void Awake()
    {
        base.Awake();
        _button = GetComponent<Button>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _button.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        IngameController.Instance.onItemInvoke.Invoke(_itemType);
    }
}
