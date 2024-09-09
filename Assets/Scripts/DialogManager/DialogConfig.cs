using System;

public enum DialogIndex
{
    PickCardDialog = 0,
    LableChooseDialog = 1,
    BuyConfirmDialog = 2,
    ItemConfirmDialog = 3,
    SettingDialog = 4,
    DailyRewardDialog = 5,
    RateDialog = 6,
    OutOffCardDialog = 7,
    SpinDialog = 8,
    BreakDialog = 9,
}

public class DialogParam { }

public class SettingDialogParam : DialogParam
{
    public bool musicSetting;
    public bool sfxSetting;
}
public class ReviveDialogParam : DialogParam
{
    public int levelNum;
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

public class ItemConfirmParam : DialogParam
{
    public bool isAds;
    public ItemType type;
    public string name;
}

public class SettingParam : DialogParam
{
    public bool isMainScreen;
}

public class DailyParam : DialogParam
{
    int currenReward;
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
public class DialogConfig
{
    public static DialogIndex[] dialogArray = {
       DialogIndex.PickCardDialog,
       DialogIndex.LableChooseDialog,
       DialogIndex.BuyConfirmDialog,
       DialogIndex.ItemConfirmDialog,
       DialogIndex.DailyRewardDialog,
       DialogIndex.SettingDialog,
       DialogIndex.RateDialog,
       DialogIndex.OutOffCardDialog,
       DialogIndex.SpinDialog,
      DialogIndex.BreakDialog,
    };
}
