using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Up,
    Down,
    UpperLeft,
    UpperRight,
    DownLeft,
    DownRight,
    Unknown
}
public enum ItemType
{
   Magnet = 0,
   Bomb = 1,
}
public enum Currency
{
   Gold = 0,
   Gem = 1,
}
public enum DailyReward
{
    Gold_S,
    Gold_M,
    Bomb,
    Gold_L,
    Gem,
    Magnet,
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
public enum IEDailyType
{
    Available = 0,
    Unavailable = 1,
    Claimed = 2,
}
public enum CardType
{
    Default ,
    Lozenge,
    Lego,
    Cassette,
    Mail,
    Dics,
    Shirt,
    Cheese,
    Gift,
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
    SkyBlue ,       // 32
}

public enum SlotStatus
{
    Active,
    InActive,
    Locked,
}
public enum SizeAmoutGold
{
    S = 5,
    M = 10,
    L = 15,
   XL = 20,
}
public enum CardPickerType
{
    Premium,
    Free,
}

public enum Lable
{
    Rate,
    Home,
    Spin,
    Collection,
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