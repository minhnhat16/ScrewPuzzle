using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DailyGrid : MonoBehaviour
{
    [SerializeField] private bool settingupGrid;
    [SerializeField] private List<DailyItem> _items;
    [SerializeField] private GridLayoutGroup _content;
    public DailyItem currentDaily;
    public bool isNewDay;
    [HideInInspector] public UnityEvent<bool> newDateEvent = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> resetDailyEvent = new UnityEvent<bool>();
    [HideInInspector] public UnityEvent<bool> lastItemClaimEvent = new UnityEvent<bool>();

    public GridLayoutGroup Content { get => _content; set => _content = value; }

    private void OnEnable()
    {
        newDateEvent = DayTimeController.instance.newDateEvent;
        newDateEvent.AddListener(NewDayCouroutine);
        resetDailyEvent.AddListener(ResetDaily);
        DataTrigger.RegisterValueChange(DataPath.DAILYTIMECLAIMED, (data) =>
        {
            if (data == null) return;
            string newData = data as string;

        });
    }
    private void OnDisable()
    {
        newDateEvent.RemoveListener(NewDayCouroutine);
    }
    public void Init()
    {

    }
    void Start()
    {
        DayTimeController.instance.CheckNewDay();
        isNewDay = DayTimeController.instance.isNewDay;
    }

    public void FetchDailyData(DailyRewardConfig config)
    {
        DailyItem curItem;
       for (int i = 0; i < 7; i++)
        {
            curItem = _items[i];
            var itemDailyConfig = config.GetRecordByKeySearch(i);
            curItem = SetupDailyRewardItem(curItem, itemDailyConfig);
            _items[i] = curItem;
        }
    }
    public void InvokeWhenHaveCurrentDaily()
    {
        if (currentDaily != null && currentDaily.currentType == IEDailyType.Available) currentDaily.CheckItemAvailable();
        else
        {
            var item = _items.First((DailyItem daily) => daily.currentType == IEDailyType.Unavailable);
            item.CheckItemAvailable();
        }
    }

    private void CheckFullDailyClaim()
    {
        //check daily in day seven is claimed;
        var lastDailyData = DataAPIController.instance.GetDailyData(6);
        if (lastDailyData.currentType == IEDailyType.Claimed)
        {
            DataAPIController.instance.SetNewDailyCircle();
        }
    }
    private DailyItem SetupDailyRewardItem(DailyItem dailyItem, DailyRewardConfigRecord dailyRewardConfig)
    {
        if (dailyRewardConfig == null) return null;
        int day = dailyRewardConfig.ID;
        //Debug.Log("Day" + day);
        DailyItemData dailyData = DataAPIController.instance.GetDailyData(day);
        if (dailyData.currentType == IEDailyType.Available)
        {
            //Debug.Log("AVAILABLE TO CLAIM");
            dailyItem.Init(IEDailyType.Available, dailyRewardConfig.TotalItem, dailyRewardConfig.ID + 1, dailyRewardConfig.SpriteName, dailyRewardConfig.ItemName);
            currentDaily = dailyItem;
            return dailyItem;
        }
        else
        {
            //Debug.Log("ELSE UNAVAILABLE TO CLAIM " + dailyData.currentType);
            dailyItem.Init(dailyData.currentType, dailyRewardConfig.TotalItem, dailyRewardConfig.ID + 1, dailyRewardConfig.SpriteName, dailyRewardConfig.ItemName);
            return dailyItem;
        }
    }
    void ResetDaily(bool isReset) {
        CheckFullDailyClaim();
    }
    public DailyItem NewDayItemInList()
    {
        for (int i = 0; i < _items.Count - 1; i++)
        {
            //Debug.Log($"daily item { _items[i].day} + type { _items[i].currentType} ");

            if (_items[i].currentType == IEDailyType.Claimed && _items[i + 1].currentType == IEDailyType.Unavailable)
            {
                //Debug.Log($"new day item id {_items[i + 1].day}");
                return _items[i + 1];
            }
        }
        resetDailyEvent?.Invoke(true);
        return _items[0];
        //Debug.LogError("null daily item");
    }
    IEnumerator NewDayCouroutine(Action callback)
    {
        currentDaily = NewDayItemInList();
        yield return new WaitUntil(() => currentDaily != null);
        callback?.Invoke();
    }
    public void NewDayCouroutine(bool isNewDay)
    {
        StartCoroutine(NewDayRewardRemain(isNewDay));
    }
    public IEnumerator NewDayRewardRemain(bool isNewDay)
    {
        //Debug.Log("NEW DAY REWARD REMAIN" + isNewDay);
        if (isNewDay)
        {
            Debug.Log("New day re ward remain");
            yield return new WaitUntil(() => settingupGrid == false);
            this.isNewDay = false;
            //Debug.Log("NEW DAY REWARD REMAIN");
            StartCoroutine(NewDayCouroutine(() =>
            {
                //Debug.Log("CURRENT  DAY REWARD REMAIN"+ currentDaily.day);
                currentDaily.SwitchType(IEDailyType.Available);
                int currentDay = currentDaily.day - 1;
                DataAPIController.instance.SetDailyData(currentDay, currentDaily.currentType);
                currentDaily.CheckItemAvailable();
            }));

        }
        yield return null;
    }
}
