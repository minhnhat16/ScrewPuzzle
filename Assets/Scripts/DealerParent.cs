using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealerParent : MonoBehaviour
{
    private const string path = "Prefabs/Dealer";
    public Vector3 spacing = new Vector3(0, 0, 0);
    [SerializeField] private List<Dealer> _dealers = new(4);

    private void OnEnable()
    {
        DataTrigger.RegisterValueChange(DataPath.LEVEL, OnLevelChange);
    }

    public void Init()
    {
        StartCoroutine(InitDealerCoroutine());
    }

    private void OnLevelChange(object newLevel)
    {
        int level = (int)newLevel;
        if (level % 5 == 0)
        {
            NextDealerCanUnlock();
        }
    }

    private IEnumerator InitDealerCoroutine()
    {
        var dealersData = DataAPIController.instance.GetAllDealerData();
      

        for (int i = 0; i < dealersData.Count; i++)
        {
            Dealer dealer = Instantiate(Resources.Load<Dealer>(path), transform);
            dealer.Id = i;
            dealer.dealSlot.ID = i;
            dealer.Init();
            _dealers.Add(dealer);
            dealer.dealSlot.Init();
            dealer.dealSlot.gameObject.SetActive(true);
            //Debug.LogWarning(" upgrade dealer parent" + dealer.upgrade_btn.transform.parent + " index " + dealer.Id);
            if (dealer.Status == SlotStatus.Active)
            {
                //dealer.set();
                dealer.dealSlot.SetCollideActive(false);
            }
            else if (dealer.Status == SlotStatus.Locked)
            {
                dealer.SetFillActive(false);
                dealer.SetUpgradeButtonActive(false);
                dealer.SetDealerLvelActive(false);
                dealer.SetRewardActive(false);
                dealer.UpdateFillPostion();
            }
            else
            {
                dealer.SetFillActive(false);
                dealer.SetUpgradeButtonActive(false);
                dealer.SetDealerLvelActive(false);
                dealer.SetRewardActive(false);
            }

            // Check if the event has any listeners before invoking
            Debug.Log($"Checking listeners for dealer {dealer.Id}");
            if (dealer.dealSlot.onToucheHandle.GetPersistentEventCount() > 0)
            {
                Debug.Log($"Dealer {dealer.Id} has listeners for the onToucheHandle event.");
            }
            else
            {
                Debug.Log($"Dealer {dealer.Id} has NO listeners for the onToucheHandle event.");
            }

            dealer.dealSlot.CheckOnTouchEvent();
            Debug.LogWarning(" upgrade dealer parent" + dealer.upgrade_btn.transform.parent + " index " + dealer.Id);

        }

        yield return new WaitForSeconds(1f);
        int c = TotalActiveDealer();
        if (c > 1)
        {
            NewUpdateDealerPosition(c);
        }
        else
        {
            // For a single dealer, position at x = 0
            NewUpdateFillWithId(0, 0);
        }

    }
    public void NewUpdateDealerPosition(int count)
    {
        Debug.LogWarning("TotalfActiveDealer: " + count);

        switch (count)
        {
            case 1:
                NewUpdateFillWithId(0, 0); // Center position for single dealer
                break;

            case 2:
                NewUpdateFillWithId(0, -1.125f); // Left
                NewUpdateFillWithId(1, 1.125f);  // Right
                break;

            case 3:
                NewUpdateFillWithId(0, -2.25f);  // Left
                NewUpdateFillWithId(1, 0);       // Center
                NewUpdateFillWithId(2, 2.25f);   // Right
                break;

            case 4:
                NewUpdateFillWithId(0, -3.375f); // Far Left
                NewUpdateFillWithId(1, -1.125f); // Left
                NewUpdateFillWithId(2, 1.125f);  // Right
                NewUpdateFillWithId(3, 3.375f);  // Far Right
                break;

            default:
                Debug.LogWarning("Unsupported number of dealers: " + count);
                break;
        }
    }

    public void NewUpdateFillWithId(int index, float xTarget)
    {
        Debug.LogWarning("NewUpdateFillWithId " + index);
        var d = _dealers[index];
        //Debug.LogWarning(" upgrade dealer parent UPdate" + d.upgrade_btn.transform.parent + " index " + index);
        Tween t = d.transform.DOMoveX(xTarget, 0.5f);
        t.OnUpdate(() =>
        {
            d._anchorPoint.DOMoveX(xTarget, 0.5f);
            d.UpdateFillPostion();
            if (d.Status == SlotStatus.Active)
            {
                d.dealSlot.UpdateCardPositionX(d.transform.position);
            }

        });
        t.OnComplete(() =>
        {
            if (d.Status == SlotStatus.InActive)
            {
                d.SetDealerLvelActive(false);
                d.SetFillActive(false);
                d.SetUpgradeButtonActive(false);
                d.SetRewardActive(false);
            }
            else if (d.Status == SlotStatus.Locked)
            {
                d.SetFillActive(false);
                d.SetUpgradeButtonActive(false);
                d.SetDealerLvelActive(false);
                d.SetRewardActive(false);
                d.dealSlot.SettingBuyBtn(true);
                d.goldGroup.SetPositionWithParent(d.gameObject);
                d.gemGroup.SetPositionWithParent(d.gameObject);
                d.gameObject.SetActive(true);
            }
            else
            {
                d.SetUpgradeButtonActive(true);
                var dealerData = DataAPIController.instance.GetSlotDataInDict(d.Id,IngameController.instance.CurrentCardType);
                d.dealSlot.LoadCardData(dealerData.stack);
                d.goldGroup.SetPositionWithParent(d.gameObject);
                d.gemGroup.SetPositionWithParent(d.gameObject);
                d.dealSlot.SetCollideActive(false);
                d.dealSlot.SetCollideActive(true);
                d.dealSlot.SetColliderSize(-1);
            }
            t.Kill();
        });
        Debug.LogWarning(" upgrade dealer parent" + d.upgrade_btn.transform.parent);    

    }

    public void NextDealerCanUnlock()
    {
        Dealer canUnlockDealer = FindInActiveDealer();
        if (canUnlockDealer != null)
        {
            canUnlockDealer.Status = SlotStatus.Locked;
            var nextDealerData = DataAPIController.instance.GetDealerData(canUnlockDealer.Id);
            nextDealerData.status = SlotStatus.Locked;
            //NewUpdateFillWithId(canUnlockDealer.Id, canUnlockDealer.transform.position.x);
            canUnlockDealer.gameObject.SetActive(true);
            switch (canUnlockDealer.Id)
            {
                case 0: break;
                case 1:
                    NewUpdateFillWithId(0, -1.125f);
                    NewUpdateFillWithId(1, 1.125f);

                    break;
                case 2:
                    NewUpdateFillWithId(0, -3.375f);
                    NewUpdateFillWithId(1, -1.125f);
                    NewUpdateFillWithId(2, 1.125f);
                    break;
                case 3:
                    NewUpdateFillWithId(0, -3.375f);
                    NewUpdateFillWithId(1, -1.125f);
                    NewUpdateFillWithId(2, 1.125f);
                    NewUpdateFillWithId(3, 3.375f);

                    break;
            }
        }
    }

    public Dealer FindInActiveDealer()
    {
        foreach (var d in _dealers)
        {
            if (d.Status == SlotStatus.InActive) return d;
        }
        return null;
    }

    public void SaveDataDealer(CardType type)
    {
        foreach (Dealer dealer in _dealers)
        {
            dealer.dealSlot.SaveCardListToData(type);
            dealer.SetDealerAndFillActive(false);
            dealer.SetDealerLvelActive(false);
            dealer.SetFillActive(false);
            dealer.dealSlot.BuyBtn.gameObject.SetActive(false); 
        }
    }

    public Dealer GetDealerAtSlot(int index)
    {
        return _dealers[index];
    }

    public List<Dealer> ActiveDealers()
    {
        return _dealers.FindAll(dealer => dealer.Status == SlotStatus.Active);
    }

    public int TotalActiveDealer()
    {
        return _dealers.FindAll(dealer => dealer.Status == SlotStatus.Active || dealer.Status == SlotStatus.Locked).Count;
    }
}
