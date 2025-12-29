using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPath
{
    public const string NAME = "userInfo/name";
    public const string ALLLEVEL = "levelInfo/levelData";
    public const string CURRENTPLAYERLEVEL = "levelInfo/currentLevel";
    public const string EXPCURRENT = "levelInfo/expLevel";
    public const string ITEM = "itemInventory";
    public const string ITEMDICT = "itemInventory/itemDict";
    public const string WALLETINVENT = "wallet";
    public const string GOLDINVENT = "wallet/goldWallet";
    public const string TICKET = "wallet/ticketWallet";
    public const string DAILYDATA = "dailyData";
    public const string ISDAILYCLAIM = DAILYDATA + "/isClaimToday";
    public const string DAILYTIMECLAIMED = DAILYDATA + "/timeClaimed";
    public const string DAILYLIST = DAILYDATA + "/dailyList";
    public const string CAMERADATA = "cameraData";
    public const string SPINDATA = "spinData";
    public const string ISSPIN =  SPINDATA +"/isSpin";
    public const string TIMESPIN = SPINDATA+ "/timeSpin";
    public const string NEWPLAYER = "userInfo/isNewPlayer";
    public const string MISSION_PROGRESS = "missions";
    public const string CRBACKGROUND = "collectionData/currentBG";
    public const string CRBOARDCOLOR = "collectionData/currentBoard";
    public const string CRSCREWCOLOR = "collectionData/currentScrew";
    public const string BACKGROUND = "collectionData/backGroundDict";
    public const string BOARDCOLOR = "collectionData/boardColorDict";
    public const string SCREWCOLOR = "collectionData/screwColorDict";
    internal static string STAGEPATH = "stageProgress";
    public static string CHESTSTAGE = "chestStates";

    public static string BLOCKSDATA = "puzzleBlockData";

    public static string TIMESAVEMETA = "timeMeta";
    public static string LASTRESETTIME = TIMESAVEMETA+ "/lastResetUtcTicks";
    public static string CURPUZZLEID = "currentPuzzleID";   

}
