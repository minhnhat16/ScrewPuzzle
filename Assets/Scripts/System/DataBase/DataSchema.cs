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

    [SerializeField]
    public Dictionary<string,MissionProgress> missions;
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
    public int bonusCount;
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
    public DailyType currentType;
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
    public CurrencyWallet ticketWallet;
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
    public long amount;
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
    public BoardColorData currentBoard;
    [SerializeField]
    public ScrewSkinData currentScrew;

    [SerializeField]
    public Dictionary<string, BackGroundData> backGroundDict;
    [SerializeField]
    public Dictionary<string, BoardColorData> boardColorDict;
    [SerializeField]
    public Dictionary<string, ScrewSkinData> screwColorDict;

    [SerializeField]

    public Dictionary<string, MissionProgress> missions = new();
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

[Serializable]
public class MissionProgress
{
    [SerializeField]
    public int missionId;

    [SerializeField]
    public int current;

    [SerializeField]
    public int target;

    [SerializeField]
    public MissionState state;

    [SerializeField]
    public long startTimestamp;

    [SerializeField]
    public bool rewardClaimed;

    [SerializeField]
    public int stage;

    [SerializeField]
    public string extra;
}

