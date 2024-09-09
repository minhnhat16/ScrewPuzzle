using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public static class DataTrigger
{
    /// <summary>
    /// Custom Extension method convert path to list path
    /// </summary>
    /// <param spriteName="path"></param>
    /// <returns></returns>
    public static List<string> ConvertToListPath(this string path)
    {
        string[] s = path.Split('/');
        List<string> paths = new List<string>();
        paths.AddRange(s);
        return paths;
    }

    private static Dictionary<string, UnityEvent<object>> dicvalueChange = new Dictionary<string, UnityEvent<object>>();

    public static void RegisterValueChange(string path, UnityAction<object> delegateDataChange)
    {
        if (!dicvalueChange.ContainsKey(path))
        {
            dicvalueChange[path] = new UnityEvent<object>();
        }
        dicvalueChange[path].AddListener(delegateDataChange);
    }

    public static void UnRegisterValueChange(string path, UnityAction<object> delegateDataChange)
    {
        if (dicvalueChange.ContainsKey(path))
        {
            dicvalueChange[path].RemoveListener(delegateDataChange);
        }
    }
    public static void TriggerValueChange(this string path, object data)
    {
        if (dicvalueChange.ContainsKey(path))
        {
            //Debug.Log("TRIGGER VALUE CHANGE" + path);
            dicvalueChange[path].Invoke(data);
        }
    }

    public static string ToKey(this int id)
    {
        return "K_" + id.ToString();
    }

    public static int FromKey(this string key)
    {
        string[] s = key.Split('_');
        return int.Parse(s[1]);
    }
}

public class DataModel : MonoBehaviour
{
    private UserData userData;
    public void InitData(Action callback)
    {
        if (LoadData())
        {
            Debug.Log("(BOOT) // INIT DATA DONE");
            callback?.Invoke();
        }
        else
        {
            //if (false) NewDataForTester();
            //else
                NewDataForPlayer();
            SaveData();

            Debug.Log("(BOOT) // INIT DATA DONE");
            callback?.Invoke();
        }
    }

    #region Read Normal

    public T ReadData<T>(string path)
    {
        object outData;
        // using extension method
        List<string> paths = path.ConvertToListPath();
        ReadDataByPath(paths, userData, out outData);
        return (T)outData;
    }

    private void ReadDataByPath(List<string> paths, object data, out object outData)
    {
        outData = null;
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            outData = field.GetValue(data);
        }
        else
        {
            paths.RemoveAt(0);
            ReadDataByPath(paths, field.GetValue(data), out outData);
        }
    }

    #endregion

    #region Read Dictionary

    public T ReadDictionary<T>(string path, string key)
    {
        // using extension method

        List<string> paths = path.ConvertToListPath();
        T outData;
        ReadDataDictionaryByPath(paths, userData, key, out outData);
        return outData;
    }

    private void ReadDataDictionaryByPath<T>(List<string> paths, object data, string key, out T dataOut)
    {
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);
        //Debug.Log(data.GetType().ToString());
        if (paths.Count == 1)
        {
            object dic = field.GetValue(data);
            Dictionary<string, T> dicData = (Dictionary<string, T>)dic;
            dicData.TryGetValue(key, out dataOut);
        }
        else
        {
            paths.RemoveAt(0);
            ReadDataDictionaryByPath(paths, field.GetValue(data), key, out dataOut);
        }
    }

    #endregion

    #region Update Normal

    public void UpdateData(string path, object newData, Action callback = null)
    {
        // using extension method
        List<string> paths = path.ConvertToListPath();
        UpdateDataByPath(paths, userData, newData, callback);
        path.TriggerValueChange(newData);
        SaveData();
    }

    private void UpdateDataByPath(List<string> paths, object data, object newData, Action callback)
    {
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            field.SetValue(data, newData);
            callback?.Invoke();
        }
        else
        {
            paths.RemoveAt(0);
            UpdateDataByPath(paths, field.GetValue(data), newData, callback);
        }
    }

    #endregion

    #region Update Dictionary

    public void UpdateDataDictionary<T>(string path, string key, T newData, Action callback = null)
    {
        List<string> paths = path.ConvertToListPath();
        object dicDataOut;
        UpdateDataDictionaryByPath<T>(paths, key, userData, newData, out dicDataOut, callback);
        (path + "/" + key).TriggerValueChange(newData);
        path.TriggerValueChange(dicDataOut);
        SaveData();
    }

    private void UpdateDataDictionaryByPath<T>(List<string> paths, string key, object data, T newData, out object dataOut, Action callback)
    {
        string p = paths[0];
        Type t = data.GetType();
        FieldInfo field = t.GetField(p);

        if (paths.Count == 1)
        {
            object dic = field.GetValue(data);
            Dictionary<string, T> dicData = (Dictionary<string, T>)dic;
            if (dicData == null)
            {
                dicData = new Dictionary<string, T>();
            }
            dicData[key] = newData;
            dataOut = dicData;
            field.SetValue(data, dicData);
            callback?.Invoke();
        }
        else
        {
            paths.RemoveAt(0);
            UpdateDataDictionaryByPath<T>(paths, key, field.GetValue(data), newData, out dataOut, callback);
        }
    }

    #endregion

    private void SaveData()
    {
        string json_string = JsonConvert.SerializeObject(userData);
        //Debug.Log("(DATA) // SAVE  DATA: " + json_string);
        PlayerPrefs.SetString("DATA", json_string);
    }

    private bool LoadData()
    {
        if (PlayerPrefs.HasKey("DATA"))
        {
            string json_string = PlayerPrefs.GetString("DATA");
            //Debug.Log("(DATA) // LOAD DATA: " + json_string);
            userData = JsonConvert.DeserializeObject<UserData>(json_string);
            return true;
        }
        return false;
    }
    private void NewDataForPlayer()
    {
        Debug.Log("(BOOT) // CREATE NEW DATA");
        userData = new UserData();
        UserInfo inf = new UserInfo();
        inf.name = ZenSDK.instance.GetConfigString("userName", "player");
        inf.isNewPlayer = true;
        userData.userInfo = inf;

        userData.itemInventory = new();
        ItemData newBombInvent = new();
        newBombInvent.type = ItemType.Bomb;
        newBombInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Bomb.ToString(), 5);
        ItemData newMagnetInvent = new();
        newMagnetInvent.type = ItemType.Magnet;
        newMagnetInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 5);
        userData.itemInventory.bombItem = newBombInvent;
        userData.itemInventory.magnetItem = newMagnetInvent;
        LevelInfo levelInf = new();
        levelInf.level = 1;
        levelInf.expLevel = 0.0f;
        userData.levelInfo = levelInf;

        ListCardColor defaultColor = new();

        CardInventory invent = new CardInventory();
        invent.listColorByType = new Dictionary<string, ListCardColor>();

        defaultColor.color = new List<CardColorPallet> { CardColorPallet.Red, CardColorPallet.Yellow/*, CardColorPallet.Blue*/ };
       
        ListCardColor newColorList = new();
        newColorList.color = new List<CardColorPallet> { CardColorPallet.Red, CardColorPallet.Yellow, CardColorPallet.Blue };

        for (int i = 0; i <9; i++)
        {
            CardType type = (CardType)i;
            Debug.LogWarning("ADD data type " + type);
            invent.currentCardType = type;
            invent.type = CardType.Default;
            if (type == CardType.Default)
            {
                invent.listColorByType.TryAdd(type.ToString(), defaultColor);
            }
            else
            {
                invent.currentCardType = type ;
                invent.listColorByType.TryAdd(type.ToString(), newColorList);

            }
        }
        //WALLET 
        userData.cardInvent = invent;
        userData.wallet = new();
        //Add gold 
        CurrencyWallet goldWallet = new();
        goldWallet.currency = Currency.Gold;
        goldWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Gold.ToString(), 100000);
        userData.wallet.goldWallet = goldWallet;

        //Add gem 
        CurrencyWallet gemWallet = new();
        gemWallet.currency = Currency.Gem;
        gemWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Gem.ToString(), 1000000);
        userData.wallet.gemWallet = gemWallet;
        //daily data
        DailyData newDaily = new();
        newDaily.isClaimToday = false;
        newDaily.timeClaimed = DateTime.MinValue.ToString();
        List<DailyItemData> _dailyData = new();
        for (int i = 0; i < 7; i++)
        {
            DailyItemData dailyData = new DailyItemData();
            dailyData.day = i + 1;
            IEDailyType iEDailyType = i == 0 ? IEDailyType.Available : IEDailyType.Unavailable;
            dailyData.currentType = iEDailyType;
            _dailyData.Add(dailyData);
        }
        newDaily.dailyList = _dailyData;
        userData.dailyData = newDaily;

        //slot data
        List<SlotData> newSlotList = new();
        int slotCount = 35;
        for (int i = -4; i < slotCount; i++)
        {
            SlotData newSlotData = new SlotData();
            newSlotData.id = i;
            if (i < 0)
            {
                newSlotData.status = SlotStatus.Active;
                //newSlotData.currentStack = new Stack<CardColorPallet>(new List<CardColorPallet>
                //        {
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red
                //        });
                newSlotList.Add(newSlotData);
            }
            else if (i == 0 || i == 1)
            {
                newSlotData.status = SlotStatus.Active;
                newSlotData.stack = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow
                            });
                newSlotList.Add(newSlotData);

            }
            else if (i == 2)
            {
                newSlotData.status = SlotStatus.Active;
                newSlotData.stack = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red
                            });
                newSlotList.Add(newSlotData);

            }
            else if (i == 3 || i == 4 || i == 7)
            {
                newSlotData.status = SlotStatus.Locked;
                newSlotList.Add(newSlotData);
            }
            else
            {
                newSlotData.status = SlotStatus.InActive;
                newSlotList.Add(newSlotData);
            }
        }
        SlotDataDict newDictSlotData = new();
        Dictionary<string, List<SlotData>> newDict = new();
        for (int i = 0; i < 10; i++)
        {
            string typeString = Convert.ToString((CardType)i);
            newDict.Add(typeString, newSlotList);
            newDictSlotData.slotDict = newDict;
        }
        userData.allSlotData = newDictSlotData;

        //dealer data
        Dictionary<string, DealerData> newDealerDict = new Dictionary<string, DealerData>();
        for (int i = 0; i < 4; i++)
        {
            DealerData newDealerData = new();
            newDealerData.status = i == 0 ? SlotStatus.Active : SlotStatus.InActive;
            newDealerData.id = i;
            newDealerData.upgradeLevel = 1;
            DataTrigger.ToKey(i);
            newDealerDict.Add(DataTrigger.ToKey(i), newDealerData);
        }
        userData.dealerDict = newDealerDict;

        SlotCameraData camData = new();
        camData.positionX = 0f;
        camData.positionY = -4.5f;
        camData.scaleTime = 0;
        camData.OrthographicSize = 10;
        userData.cameraData = new();
        for (int i = 0; i < 9; i++)
        {     
            string type = Convert.ToString((CardType)i);
            userData.cameraData.TryAdd(type, camData);
        }
        CardCounter newCardCounter = new();
        newCardCounter.lastSaveTime = DateTime.Now.ToString();
        newCardCounter.currentTime = DateTime.Now.ToString();

        newCardCounter.maxCardPool = ZenSDK.instance.GetConfigInt("cardPool", 500);
        newCardCounter.currentCardPool = newCardCounter.maxCardPool;
        userData.cardCounter = newCardCounter;

        SpinData newSpinData = new();
        newSpinData.isSpin = false;
        newSpinData.timeSpin = DateTime.MinValue.ToString();
        userData.spinData = newSpinData;
    }
    private void NewDataForTester()
    {
        Debug.Log("(BOOT) // CREATE NEW DATA");
        userData = new UserData();
        UserInfo inf = new UserInfo();
        inf.name = ZenSDK.instance.GetConfigString("userName", "player");
        inf.isNewPlayer = true;
        userData.userInfo = inf;

        userData.itemInventory = new();
        ItemData newBombInvent = new();
        newBombInvent.type = ItemType.Bomb;
        newBombInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Bomb.ToString(), 5);
        ItemData newMagnetInvent = new();
        newMagnetInvent.type = ItemType.Magnet;
        newMagnetInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 5);
        userData.itemInventory.bombItem = newBombInvent;
        userData.itemInventory.magnetItem = newMagnetInvent;
        LevelInfo levelInf = new();
        levelInf.level = 1;
        levelInf.expLevel = 0.0f    ;
        userData.levelInfo = levelInf;

        userData.wallet = new();
        //Add gold 
        CurrencyWallet goldWallet = new();
        goldWallet.currency = Currency.Gold;
        goldWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Gold.ToString(), 100000000);
        userData.wallet.goldWallet = goldWallet;

        //Add gem 
        CurrencyWallet gemWallet = new();
        gemWallet.currency = Currency.Gem;
        gemWallet.amount = ZenSDK.instance.GetConfigInt(Currency.Gem.ToString(), 10000000);
        userData.wallet.gemWallet = gemWallet;
        DailyData newDaily = new();
        newDaily.isClaimToday = false;
        newDaily.timeClaimed = DateTime.MinValue.ToString();
        List<DailyItemData> _dailyData = new();
        for (int i = 0; i < 7; i++)
        {
            DailyItemData dailyData = new DailyItemData();
            dailyData.day = i + 1;
            IEDailyType iEDailyType = i == 0 ? IEDailyType.Available : IEDailyType.Unavailable;
            dailyData.currentType = iEDailyType;
            _dailyData.Add(dailyData);
        }
        newDaily.dailyList = _dailyData;
        userData.dailyData = newDaily;
        List<SlotData> newSlotList = new();
        int slotCount = 35;
        for (int i = -4; i < slotCount; i++)
        {
            SlotData newSlotData = new SlotData();
            newSlotData.id = i;
            if (i < 0)
            {
                newSlotData.status = SlotStatus.Active;
                //newSlotData.currentStack = new Stack<CardColorPallet>(new List<CardColorPallet>
                //        {
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red,
                //            CardColorPallet.Red
                //        });
                newSlotList.Add(newSlotData);
            }
            else if (i == 0 || i == 1)
            {
                newSlotData.status = SlotStatus.Active;
                newSlotData.stack = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow,
                                CardColorPallet.Yellow
                            });
                newSlotList.Add(newSlotData);

            }
            else if (i == 2)
            {
                newSlotData.status = SlotStatus.Active;
                newSlotData.stack = new Stack<CardColorPallet>(new List<CardColorPallet>
                            {
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red,
                                CardColorPallet.Red
                            });
                newSlotList.Add(newSlotData);

            }
            else if (i == 3 || i == 4 || i == 7)
            {
                newSlotData.status = SlotStatus.Locked;
                newSlotList.Add(newSlotData);
            }
            else
            {
                newSlotData.status = SlotStatus.InActive;
                newSlotList.Add(newSlotData);
            }
        }
        SlotDataDict newDictSlotData = new();
        Dictionary<string, List<SlotData>> newDict = new();
        for (int i = 0; i < 9; i++)
        {
            string t = ((CardType)i).ToString();
            newDict.Add(t, newSlotList);
        }
        newDictSlotData.slotDict = newDict;
        userData.allSlotData = newDictSlotData;
        Dictionary<string, DealerData> newDealerDict = new Dictionary<string, DealerData>();
        for (int i = 0; i < 4; i++)
        {
            DealerData newDealerData = new();
            newDealerData.status = i == 0 ? SlotStatus.Active : SlotStatus.InActive;
            newDealerData.id = i;
            newDealerData.upgradeLevel = 1;
            DataTrigger.ToKey(i);
            newDealerDict.Add(DataTrigger.ToKey(i), newDealerData);
        }
        userData.dealerDict = newDealerDict;

        SlotCameraData camData = new();
        camData.positionX = 0f;
        camData.positionY = -4.5f;
        camData.scaleTime = 0;
        camData.OrthographicSize = 10;

        userData.cameraData = new Dictionary<string, SlotCameraData>();
        for (int i = 0; i < 9; i++)
        {
            string type = Convert.ToString((CardType)i);
            userData.cameraData.TryAdd(type, camData);
        }
        CardCounter newCardCounter = new();
        newCardCounter.lastSaveTime = DateTime.Now.ToString();
        newCardCounter.currentTime = DateTime.Now.ToString();

        newCardCounter.maxCardPool = ZenSDK.instance.GetConfigInt("cardPool", 500);
        newCardCounter.currentCardPool = newCardCounter.maxCardPool;
        userData.cardCounter = newCardCounter;

        SpinData newSpinData = new();
        newSpinData.isSpin = false;
        newSpinData.timeSpin = DateTime.MinValue.ToString();
        userData.spinData = newSpinData;
    }
}

public class GameInitData
{
    public int defaultSkinID = ZenSDK.instance.GetConfigInt("fruitSkin", 4);
    public int defaultBoxSkinID = ZenSDK.instance.GetConfigInt("boxSkin", 12);
}
