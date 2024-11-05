using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
    [SerializeField]
    public UserInfo userInfo;
    [SerializeField]
    public LevelInfo levelInfo;
    [SerializeField]
    public ItemInvent itemInventory;
    [SerializeField]
    public Wallet wallet;
    [SerializeField]
    public DailyData dailyData;
    [SerializeField]
    public SpinData spinData;
    [SerializeField]
    public CollectionData collectionData;
}
[Serializable]
public class UserInfo
{
    public int ID;
    public string name;
    public bool isNewPlayer;
}
[Serializable]
public class LevelInfo
{
    public int currentLevel;
    public float expLevel;
    public List<LevelData> levelData;
}
[Serializable]
public class ItemInvent
{
    public Dictionary<string, ItemData> itemDict;
}
[Serializable]
public class DailyItemData
{
    public int day;
    public IEDailyType currentType;
}
[Serializable]
public class ItemData
{
    public ItemType type;
    public int total;
}

[Serializable]
public class Wallet
{
    public CurrencyWallet goldWallet;
    public CurrencyWallet gemWallet;
}
public class DailyData
{
    public bool isClaimToday;
    public string timeClaimed;
    public List<DailyItemData> dailyList;

}
[Serializable]
public class CurrencyWallet
{
    public Currency currency;
    public int amount;
}
[Serializable]
public class SpinData
{
    public bool isSpin;
    public string timeSpin;
}

[Serializable]
public class LevelData
{
    public int levelID;
    public bool isCompleted;
    public int levelStar;
}

[Serializable]
public class CollectionData
{

    [SerializeField]
    public BackGroundData currentBG;
    [SerializeField]
    public BackGroundData currentBoard;
    [SerializeField]
    public BackGroundData currentScrew;

    [SerializeField]
    public Dictionary<string, BackGroundData> backGroundDict;
    [SerializeField]
    public Dictionary<string, BoardColorData> boardColorDict;
    [SerializeField]
    public Dictionary<string, ScrewSkinData> screwColorDict;
}
[Serializable]
public class ScrewSkinData
{
    [SerializeField]
   public bool isUnlocked;
    [SerializeField]
    public string name;

}
[Serializable]

public class BackGroundData
{
    [SerializeField]
    public bool isUnlocked;
    [SerializeField]
    public string name;
}
[Serializable]
public class BoardColorData
{
    [SerializeField]
    public bool isUnlocked;
    [SerializeField]
    public string name;
}
