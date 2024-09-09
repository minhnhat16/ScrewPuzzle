using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private IngameController ingameController;
    [SerializeField] private DayTimeController dayTimeController;
    public List<CardColorPallet> listCurrentCardColor;
    public UIRootControlScale UIRoot;
    [SerializeField] private TutorialsScript tutorial;
    [SerializeField] private int languageID;
    [SerializeField] private int totalLevel;
    [SerializeField] private int trackLevelStart;
    [SerializeField] private int levelCanUnlockNewCard;

    [SerializeField] private bool isNewPlayer;

    public int TrackLevelStart { get=> trackLevelStart; set => trackLevelStart = value; }
    public bool IsNewPlayer { get => isNewPlayer; set => isNewPlayer = value; }
    public int TotalLevel { get => totalLevel; set => totalLevel = value; }

    private void Awake()
    {
        if (instance == null) instance = this;
        DOTween.SetTweensCapacity(1000, 125);
    }
    // Start is called before the first frame update
    void Start()
    {
        UIRoot = GetComponentInParent<UIRootControlScale>();
        DataTrigger.RegisterValueChange(DataPath.LEVEL, OnLevelChange);
        //ingameController.gameObject.SetActive(false);
    }
    public void OnLevelChange(object newLevel)
    {
        int level = (int)newLevel;
        if (level % 10 == 0)
        {
            level /= levelCanUnlockNewCard;
            NextLevelCanUnlock(level);
        }
    }
    public void NextLevelCanUnlock(int levelCanUnlock)
    {
        CardColorPallet yellow = CardColorPallet.Yellow;
        CardColorPallet red = CardColorPallet.Red;
        CardType type = (CardType)levelCanUnlock;
        DataAPIController.instance.SaveNewCardColor(yellow, type, null);
        DataAPIController.instance.SaveNewCardColor(red, type, null);

    }
    public void GetCardListColorFormData( CardType currentType)
    {
        listCurrentCardColor = new();
        var colorData = DataAPIController.instance.GetDataColorByType(currentType);
        listCurrentCardColor = colorData.color;
    }

    public void SetupGameManager()
    {
        dayTimeController = FindObjectOfType<DayTimeController>();
        dayTimeController.enabled = true;
    }
    public void LoadIngameSence(Action callback)
    {
        //ingameController.enabled = true;
        ingameController.gameObject.SetActive(true);
        ingameController.Init(callback);

        //CameraMain.instance.main.gameObject.SetActive(true);
    }
    public void SetUpIngame()
    {
        dayTimeController.StartCoroutine(dayTimeController.InitCouroutine());
        isNewPlayer = DataAPIController.instance.IsNewPlayer();
        totalLevel = DataAPIController.instance.GetAllCardColorType().Count;
    }
    public void SetupTutorial()
    {
        if (IsNewPlayer)
        {
            string path = "Prefabs/Tutorial";
            tutorial = Instantiate(Resources.Load<TutorialsScript>(path), transform);
            tutorial.gameObject.SetActive(true);
        }
    }
   
    public string DevideCurrency(int currency)
    {
        if (currency < 10000) return currency.ToString();
        else
        {
            currency /= 1000;
            currency.ToString();
            return $"{currency}k";
        }
    }
}
