using System;

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
    public bool isMainScreen;
}

public class DailyParam : DialogParam
{
    int currenReward;
    public DailyData data;
    public DailyRewardConfig config;
}

public class RateParam : DialogParam
{
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
}

public class AddItemDialogParam : DialogParam
{
    public ItemType ItemType;
    public bool IsAdsAvailable;
    public int ItemPrice;
    public string detail;
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
    };
}