using System;
using System.Collections.Generic;
using System.DataBase;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public IngameController ingameController;
        public DayTimeController dayTimeController;

        [SerializeField] private int languageID;
        [SerializeField] private int totalLevel;
        [SerializeField] private int trackLevelStart;
        [SerializeField] private int levelCanUnlockNewCard;
        [SerializeField] private int playerGold;
        [SerializeField] private float popupDuration = 1;
        [SerializeField] private float starMoveDuration = 1;
        [SerializeField] private Vector3 starScale = new(0.5f, 0.5f);

        [SerializeField] private bool isNewPlayer;
        public List<CardColorPallet> listCurrentCardColor;
        public UIRootControlScale UIRoot;
        public int TrackLevelStart { get => trackLevelStart; set => trackLevelStart = value; }
        public bool IsNewPlayer { get => isNewPlayer; set => isNewPlayer = value; }
        public int TotalLevel { get => totalLevel; set => totalLevel = value; }
        public float PopupDuration { get => popupDuration; set => popupDuration = value; }
        public float StarMoveDuration { get => starMoveDuration; set => starMoveDuration = value; }
        public Vector3 StarScale { get => starScale; set => starScale = value; }

        private void Awake()
        {
            if (instance == null) instance = this;
            DOTween.SetTweensCapacity(1000, 125);
            ingameController = GetComponent<IngameController>();
            dayTimeController = GetComponent<DayTimeController>();
        }
        // Start is called before the first frame update
        void Start()
        {
            UIRoot = GetComponentInParent<UIRootControlScale>();

            //DataTrigger.RegisterValueChange(DataPath.ALLLEVEL, OnLevelChange);
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
        public void AddGoldToCurrent(int gold, Action<bool> success = null)
        {

            int current = playerGold;
            playerGold += gold;
            success?.Invoke(current < playerGold);
        }
        public void MinusGoldToCurrent(int gold, Action<bool> success = null)
        {

            int current = playerGold;
            playerGold -= gold;
            success?.Invoke(current > playerGold);
        }
        public int GetPlayerGold()
        {
            return playerGold;
        }
        public void SaveGoldToData(Action<bool> success = null)
        {
            CurrencyWallet gold = new();
            gold.currency = Currency.Gold;
            gold.amount = playerGold;
            DataAPIController.instance.SaveGold(gold, success);
        }
        public void NextLevelCanUnlock(int levelCanUnlock)
        {


        }
        public void SetupGameManager()
        {
            dayTimeController = FindFirstObjectByType<DayTimeController>();
            dayTimeController.enabled = true;
        }
        public void LoadIngameSence(Action callback)
        {
            //ingameController.enabled = true;
            ingameController.gameObject.SetActive(true);

            //CameraMain.instance.main.gameObject.SetActive(true);
        }
        public void SetUpIngame()
        {
            dayTimeController.StartCoroutine(dayTimeController.InitCouroutine());
            isNewPlayer = DataAPIController.instance.IsNewPlayer();
            totalLevel = DataAPIController.instance.GetPlayerLevel();
            playerGold = DataAPIController.instance.GetGold();
        }
        public void SetupTutorial()
        {
            if (IsNewPlayer)
            {

            }
        }
        public int GoldCalculation(int level)
        {
            float gold = (int)((1f + level * 0.4f) * 1000);
            int goldInt = Mathf.FloorToInt(gold);
            return goldInt;
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
}
