using Managers;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using UnityEngine;
using UnityEngine.UI;

public class GoldDisplay : MonoBehaviour
{
    public Text goldLB;
    public Button addGoldBtn;

    private void OnEnable()
    {
        addGoldBtn.onClick.AddListener(AddGoldButton);
    }
    private void OnDisable()
    {
        addGoldBtn.onClick.RemoveListener(AddGoldButton); 
    }
    private void AddGoldButton()
    {
        ShopViewParam param = new();
        param.gold = GameManager.instance.GetPlayerGold();
        ViewManager.Instance.SwitchView(ViewIndex.ShopView,param);
    }
    public void SetGoldToLable(int gold)
    {
        goldLB.text = GameManager.instance.DevideCurrency(gold);
    }
}
