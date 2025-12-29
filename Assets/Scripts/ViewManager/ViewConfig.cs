using System.Collections.Generic;
using UIScript;

public enum ViewIndex
{
    EmptyView = 0,
    LoadingView = 1,
    MainScreenView = 2,
    GameView = 3,
    LevelView =4,
    DailyView = 5,
    WardrobeView = 6,   
    ShopView = 7,
    PuzzleView = 8,
}

public class ViewParam { }

public class LoadingViewParam : ViewParam
{
    public float target;
}
public class GamePlayViewParam : ViewParam
{
    public long totalGold;
    public bool isNewPlayer;
    public int currentCardCount;
    public int maxCardCount;
    public string currentTime;
    public string lastTime;
}

public class ShopViewParam : ViewParam
{
    public long gold;
}
public class MainScreenViewParam : ViewParam
{
    public long totalGold;
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
        ViewIndex.GameView,
        ViewIndex.DailyView,
        ViewIndex.LevelView,
        ViewIndex.ShopView,
        ViewIndex.PuzzleView,
    };
}