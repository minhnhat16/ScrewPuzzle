using System.Collections.Generic;
using UIScript;

public enum ViewIndex
{
    EmptyView = 0,
    LoadingView = 1,
    MainScreenView = 2,
    GamePlayView = 3,
    LevelView =4,
    DailyView = 5,
    WardrobeView = 6,   
    ShopView = 7,
    CollectionView = 8,
}

public class ViewParam { }

public class LoadingViewParam : ViewParam
{ 
}
public class GamePlayViewParam : ViewParam
{
    public int totalGold;
    public bool isNewPlayer;
    public int currentCardCount;
    public int maxCardCount;
    public string currentTime;
    public string lastTime;
}

public class ShopViewParam : ViewParam
{
    public int gold;
}
public class MainScreenViewParam : ViewParam
{
    public int totalGold;
}
public class CollectionParam : ViewParam
{
    public int ownedCard;
    public int totalCard;
}

public class LevelParam : ViewParam
{
    public int currentLevel;
    public List<BaseLevelItem> listLevelItems;
}
public class ViewConfig
{
    public static ViewIndex[] viewArray = {
        ViewIndex.EmptyView,
        ViewIndex.LoadingView,
        ViewIndex.MainScreenView,
        ViewIndex.GamePlayView,
        ViewIndex.DailyView,
        ViewIndex.LevelView,

        ViewIndex.ShopView,
        ViewIndex.CollectionView,

    };
}