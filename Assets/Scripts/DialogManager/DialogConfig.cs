using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogIndex
{
    LableChooseDialog,
    BuyConfirmDialog,
    ItemDialog,
    SettingDialog,
    DailyRewardDialog,
    RateDialog,
    SpinDialog,
    ReviveDialog,
    WinDialog,
    LoseDialog,
    BreakDialog,
    LevelDialog,
    CollectionDialog,
    SpecialDialog,
    AdsRemoveDialog,
    QuitDialog,
    MissionDialog,
    NotifyDialog,
    QuestDialog,
    GiftClaimDialog,
}

public class DialogParam
{
}

public class SettingDialogParam : DialogParam
{
    public bool musicSetting;
    public bool sfxSetting;
}

public class OutOffCardParam : DialogParam
{
    public DateTime targetTime;
}

public class DailyDialogParam : DialogParam
{
    int totalDay;
    bool isClaimed;
}

public class SpinParam : DialogParam
{
    int totalDay;
}

public class BuyConfirmDialogParam : DialogParam
{
    public Action onConfirmAction;
    public Action onCancleAction;
    public string amount_lb;
    public string bonus_lb;
    public string cost_lb;
    public int cost;
    public string plaintext;
}
public class SettingParam : DialogParam
{
    public long totalGold;
    internal long totalTicket;
    public bool isMainScreen;
    public string title;
    internal bool music_enable;
    internal bool sfx_enable;
}

public class DailyParam : DialogParam
{
    short currenReward;
    public long totalGold;

    public DailyData data;
    public DailyRewardConfig config;
}

public class RateParam : DialogParam
{
    public long totalGold;

}


public class ReviveParam : DialogParam
{
    public bool isRevive;
    public bool isHasAds;
    public long totalGold;
    public long currentTicket;
}
public class WinParam : DialogParam
{
    public int level;
    public int gold;
    public long totalGold;
    public int reward;
    public float score;
    public ItemType typeReward;
    internal long ticket;

    public int targetLevel;
    public int currentLevel;
}
public class AddItemDialogParam : DialogParam
{
    public long totalGold;
    public ItemType ItemType;
    public bool IsAdsAvailable;
    public int ItemPrice;
    public string detail;
    public Sprite sprite;
}
public class CollectionDialogParam : DialogParam
{
    public long totalGold;
    public CollectionConfig collection;
    public ScrewSkinData currentSkin;
    public BackGroundData currentBG;
    public BoardColorData currentBoard;
    public List<ScrewSkinData> skinData;
    public List<BackGroundData> backGround;
    public List<BoardColorData> BoardColor;

}
public class AdsRemoveParam : DialogParam
{
    public bool isPaymentAvailable;
    public bool isPaid;

    public float price;
    public string currency;
    public long totalGold;

}
public class SpecialDialogParam : DialogParam
{
    public bool isPaymentAvailable;
    public bool isPaid;

    public string time;
    public float price;
    public string currency;
    public long totalGold;

    public List<ShopItem> specialItems;

}
public class NotifyDialog : DialogParam
{

    public string message;
    public string header;
}
public class MissionParam : DialogParam
{
    public long totalGold;
    public long totalTicket;
    public int current;
    public int target;
    public SideMission SideMission;
    public MissionParam() { }

}
public class GiftParam : DialogParam
{
    public List<RewardItem> rewards;

    public GiftParam() { }  
    public GiftParam(QuestChestParam chestParam)
    {
        this.rewards = chestParam.rewards; 
    }
}
public class LoseParam : DialogParam
{
    public bool isAdAvailable;
}
public class DialogConfig
{
    public static DialogIndex[] dialogArray =
    {
        DialogIndex.LableChooseDialog,
        DialogIndex.BuyConfirmDialog,
        DialogIndex.ItemDialog,
        DialogIndex.DailyRewardDialog,
        DialogIndex.SettingDialog,
        DialogIndex.SpinDialog,
        DialogIndex.ReviveDialog,
        DialogIndex.WinDialog,
        DialogIndex.LoseDialog,
        DialogIndex.BreakDialog,
        DialogIndex.LevelDialog,
        DialogIndex.CollectionDialog,
        DialogIndex.SpecialDialog,
        DialogIndex.AdsRemoveDialog,
        DialogIndex.SpecialDialog,
        DialogIndex.QuitDialog
,       DialogIndex.MissionDialog,
        DialogIndex.QuestDialog,
        DialogIndex.GiftClaimDialog,
    };
}