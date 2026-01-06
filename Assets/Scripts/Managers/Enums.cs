public enum SpriteGroup
{
    None = 0,     // Không thuộc a / b
    Main = 1,     // a
    Outline = 2,  // b
    UI = 3,
    Effect = 4,
    Background = 5


}
public enum ItemType
{
    Magnet = 0,
    Breaker = 1,
    Drill = 2,
    AddBox = 3,
    Gold = 4,
    Ticket = 5,
}

public enum PackType
{
    Common,
    Rare,
    Epic,
    Ticket,
    Gold
}
public enum Currency
{
    Gold = 0,
    Ticket = 1,
    RealMoney = 2,
    Ads = 3,
}
public enum DailyReward
{
    GoldS,
    GoldM,
    GoldL,
    Gem,
    AddHold,
    AddBox,
    ClearOneScrew,
    Bonus,
}
public enum SpinEnum
{
    Magnet,
    Bomb,
    Gold,
    Gem,
    Bonus,
}
public enum DailyType
{
    Available = 0,
    Unavailable = 1,
    Claimed = 2,
}
public enum CardColorPallet
{
    Empty,
    LightRed,      // 1
    Pink,          // 2
    LightPurple,   // 3
    Purple,        // 4
    DarkPurple,    // 5
    LightBlue,     // 6
    Cyan,          // 7
    Aqua,          // 8
    LightGreen,    // 9
    LimeGreen,     // 10
    Yellow,        // 11
    Orange,        // 12
    DarkOrange,    // 13
    Peach,         // 14
    Red,           // 15
    BrightRed,     // 16
    SalmonPink,    // 17
    Gray,          // 18
    Teal,          // 19
    Turquoise,     // 20
    Blue,          // 21
    Violet,        // 22
    Magenta,       // 23
    DarkRed,       // 24
    OliveGreen,    // 25
    MustardYellow, // 26
    Brown,         // 27
    LightTeal,     // 28
    Mauve,         // 29
    Maroon,        // 30
    Lavender,      // 31
    SkyBlue,       // 32
}

public enum SizeAmoutGold
{
    S = 5,
    M = 10,
    L = 15,
    XL = 20,
}

public enum PackEnum
{
    Pack,
    Ticket,
    Coin,
}

public enum MissionType
{
    CollectColor,
    ClearRainbowBox,
    ClearNormalBoxes,
    UseItem,
    TimeSurvive,
    ScoreReached,
    CompleteLevel,
    CompleteSpecialLevel,
}

public enum MissionState
{
    Locked = 0,      // chưa mở
    InProgress = 1,  // đang làm
    Completed = 2,   // đủ điều kiện claim
    Claimed = 3 ,     // đã nhận thưởng
    NotStarted = 4,
}

public enum TutorialEnum
{
    StepOne = 1,
    StepTwo = 2,
    StepThree = 3,
    SteppFourth = 4,
    StepFive = 5,
    StepUnlock = 6,
    Final = 7,
}

public enum LevelEnum
{
    Lock,
    Complete,
    Hard,
}


public enum  ChestLocation
{
    Quest,
    Puzzle,
}
