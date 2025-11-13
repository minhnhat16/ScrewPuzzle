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


        ///item
        userData.itemInventory = new();
        Dictionary<string, ItemData> newItemDict = new();

        ItemData newAddHoldInvent = new();
        newAddHoldInvent.type = ItemType.Magnet;
        newAddHoldInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 5);
        var key = newAddHoldInvent.type.ToString();
        newItemDict.Add(key, newAddHoldInvent);

        ItemData newAddBoxInvent = new();
        newAddBoxInvent.type = ItemType.Breaker;
        newAddBoxInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Breaker.ToString(), 5);
        key = newAddBoxInvent.type.ToString();
        newItemDict.Add(key, newAddBoxInvent);

        ItemData newClearOnScrewInvent = new();
        newClearOnScrewInvent.type = ItemType.Drill;
        newClearOnScrewInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Drill.ToString(), 5);
        key = newClearOnScrewInvent.type.ToString();
        newItemDict.Add(key, newClearOnScrewInvent);

        userData.itemInventory.itemDict = new();
        userData.itemInventory.itemDict = newItemDict;
        //leve
        LevelInfo levelInf = new();
        levelInf.expLevel = 0.0f;
        levelInf.bonusCount = 0;
        LevelData dataLevelOne = new();
        dataLevelOne.levelID = 1;
        dataLevelOne.levelStar = 0;
        dataLevelOne.isCompleted = true;
        levelInf.currentLevel = 1;
        levelInf.levelData = new();
        levelInf.levelData.Add(dataLevelOne);

        userData.levelInfo = levelInf;
        userData.wallet = new();

        //WALLET 
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
            DailyType iEDailyType = i == 0 ? DailyType.Available : DailyType.Unavailable;
            dailyData.currentType = iEDailyType;
            _dailyData.Add(dailyData);
        }
        newDaily.dailyList = _dailyData;
        userData.dailyData = newDaily;

        ; CollectionData newCollectionData = new CollectionData();

        // Initialize current items
        newCollectionData.currentBG = new BackGroundData { isUnlocked = true ,name = "07_Shape_mini board" };
        newCollectionData.currentBoard = new BoardColorData { isUnlocked = true, name = "18_Screw_mini board" };
        newCollectionData.currentScrew = new ScrewSkinData { isUnlocked = true, name = "19_Screw_mini board" };

        // Initialize background dictionary with example entries
        newCollectionData.backGroundDict = new Dictionary<string, BackGroundData>();
        for (int i = 0; i < 5; i++)
        {
            BackGroundData bgData = new BackGroundData { /* Set properties for each background */ };
            newCollectionData.backGroundDict.Add($"Background_{i + 1}", bgData);
        }
        // Initialize board color dictionary with example entries
        newCollectionData.boardColorDict = new Dictionary<string, BoardColorData>();
        for (int i = 0; i < 5; i++)
        {
            BoardColorData boardColorData = new BoardColorData { /* Set properties for each board color */ };
            newCollectionData.boardColorDict.Add($"BoardColor_{i + 1}", boardColorData);
        }

        // Initialize screw color dictionary with example entries
        newCollectionData.screwColorDict = new Dictionary<string, ScrewSkinData>();
        for (int i = 0; i < 5; i++)
        {
            ScrewSkinData screwSkinData = new ScrewSkinData { /* Set properties for each screw color */ };
            newCollectionData.screwColorDict.Add($"ScrewColor_{i + 1}", screwSkinData);
        }

        // Assign to user data
        userData.collectionData = newCollectionData;

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

        ///item
        userData.itemInventory = new();
        Dictionary<string, ItemData> newItemDict = new();

        ItemData newAddHoldInvent = new();
        newAddHoldInvent.type = ItemType.Magnet;
        newAddHoldInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Magnet.ToString(), 5);
        var key = newAddHoldInvent.type.ToString();
        newItemDict.Add(key, newAddHoldInvent);

        ItemData newAddBoxInvent = new();
        newAddBoxInvent.type = ItemType.Breaker;
        newAddBoxInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Breaker.ToString(), 5);
        key = newAddBoxInvent.type.ToString();
        newItemDict.Add(key, newAddBoxInvent);

        ItemData newClearOnScrewInvent = new();
        newClearOnScrewInvent.type = ItemType.Drill;
        newClearOnScrewInvent.total = ZenSDK.instance.GetConfigInt(ItemType.Drill.ToString(), 5);
        key = newClearOnScrewInvent.type.ToString();
        newItemDict.Add(key, newClearOnScrewInvent);

        userData.itemInventory.itemDict = new();
        userData.itemInventory.itemDict = newItemDict;

        //player level
        LevelInfo levelInf = new();
        levelInf.expLevel = 0.0f;
        LevelData dataLevelOne = new();
        dataLevelOne.levelID = 1;
        dataLevelOne.levelStar = 0;
        dataLevelOne.isCompleted = false;
        levelInf.currentLevel = 1;
        levelInf.levelData.Add(dataLevelOne);

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
            DailyType iEDailyType = i == 0 ? DailyType.Available : DailyType.Unavailable;
            dailyData.currentType = iEDailyType;
            _dailyData.Add(dailyData);
        }
        newDaily.dailyList = _dailyData;
        userData.dailyData = newDaily;
        SpinData newSpinData = new();
        newSpinData.isSpin = false;
        newSpinData.timeSpin = DateTime.MinValue.ToString();
        userData.spinData = newSpinData;
    }
}

