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

    [SerializeField]
    public Dictionary<int, ChestStageData> chestStates = new();
    [SerializeField]
    public Dictionary<int, StageProgress> stageProgress = new();
    [SerializeField]
    public Dictionary<int, BlockData> puzzleBlockData = new();
    [SerializeField]
    public TimeSaveMeta timeMeta;


    [SerializeField]
    public int currentPuzzleID;

    [SerializeField]
    public bool music;
    [SerializeField]
    public bool sfx;
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

    public Dictionary<int, MissionProgress> missions = new();
    [SerializeField]
    public Dictionary<int, ChestStageData> chestStates = new();
    [SerializeField]
    public TimeSaveMeta timeMeta;
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
    public MissionState stage;

    [SerializeField]
    public string extra;
}
[Serializable]
public class StageProgress
{
    public int stageId;        // số thứ tự stage
    public bool isUnlocked;    // đã mở chưa
    public bool isCompleted;   // hoàn thành chưa
    public bool rewardClaimed; // nếu có chest ở cuối stage

    public int chestProgress;  // tiến trình mở rương của stage

    public int claimedMissions = 0;
}

[Serializable]
public class ChestStageData
{
    public int chestId;
    public bool isUnlocked;
    public bool isClaimed;
    public float progress;   // nếu bạn có tiến trình mở rương
    public ChestLocation location;
}
[Serializable]
public class BlockData
{
    public bool unlocked;
    public int screwRequired;
    public Dictionary<int, bool> removedCells = new();

}
[Serializable]
public class TimeSaveMeta
{
    public long lastResetUtcTicks;
}
