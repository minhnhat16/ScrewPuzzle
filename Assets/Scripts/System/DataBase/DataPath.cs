using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPath
{
    public const string NAME = "userInfo/name";
    public const string LEVEL = "levelInfo/level";
    public const string EXPCURRENT = "levelInfo/expLevel";
    public const string ITEM = "itemInventory";
    public const string MAGNET = "itemInventory/magnetItem";
    public const string BOMB = "itemInventory/bombItem";
    public const string WALLETINVENT = "wallet";
    public const string GOLDINVENT = "wallet/goldWallet";
    public const string GEMINVENT = "wallet/gemWallet";
    public const string DAILYDATA = "dailyData";
    public const string ISDAILYCLAIM = DAILYDATA + "/isClaimToday";
    public const string DAILYTIMECLAIMED = DAILYDATA + "/timeClaimed";
    public const string DAILYLIST = DAILYDATA + "/dailyList";
    public const string CARDINVENT = "cardInvent";
    public const string CURRENTCARDTYPE = "cardInvent/currentCardType";
    public const string LISTCOLORBYTYPE = "cardInvent/listColorByType";
    public const string ALLSLOTDATA = "allSlotData";
    public const string DEALERDICT = "dealerDict";
    public const string CAMERADATA = "cameraData";
    public const string SLOTDATADICT = "allSlotData/slotDict";
    public const string CARDCOUNTER = "cardCounter";
    public const string LASTSAVETIME = CARDCOUNTER + "/lastSaveTime";
    public const string CURRENTTIME = CARDCOUNTER + "/currentTime";
    public const string CURRENTCARDPOOL = CARDCOUNTER + "/currentCardPool";
    public const string MAXCARDPOOL = CARDCOUNTER + "/maxCardPool";
    public const string SPINDATA = "spinData";
    public const string ISSPIN =  SPINDATA +"/isSpin";
    public const string TIMESPIN = SPINDATA+ "/timeSpin";
    internal static readonly string NEWPLAYER = "userInfo/isNewPlayer";
}
