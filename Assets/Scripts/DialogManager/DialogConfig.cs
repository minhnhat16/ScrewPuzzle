using System;
using System.Collections.Generic;

public enum DialogIndex
{
    LableChooseDialog,
    BuyConfirmDialog,
    AddItemDialog,
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
    public int totalGold;
    public bool isMainScreen;
    public string title;
}

public class DailyParam : DialogParam
{
    int currenReward;
    public int totalGold;

    public DailyData data;
    public DailyRewardConfig config;
}

public class RateParam : DialogParam
{
    public int totalGold;

}

public class PickCardParam : DialogParam
{
    public CardColorPallet premium;
    public CardColorPallet free;
}

public class ReviveDialogParam : DialogParam
{
    public bool isRevive;
    public bool isHasAds;
    public int totalGold;
}
public class WinParam : DialogParam
{
    public int level;
    public int gold;
    public int totalGold;
    public int reward;
    public float score;
    public ItemType typeReward;
}
public class AddItemDialogParam : DialogParam
{
    public int totalGold;
    public ItemType ItemType;
    public bool IsAdsAvailable;
    public int ItemPrice;
    public string detail;
}
public class CollectionDialogParam : DialogParam
{
    public int totalGold;
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
    public int totalGold;

}
public class SpecialDialogParam : DialogParam
{
    public bool isPaymentAvailable;
    public bool isPaid;

    public string time;
    public float price;
    public string currency;
    public int totalGold;

    public List<ShopItem> specialItems;

}


public class DialogConfig
{
    public static DialogIndex[] dialogArray =
    {
        DialogIndex.LableChooseDialog,
        DialogIndex.BuyConfirmDialog,
        DialogIndex.AddItemDialog,
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
    };
}