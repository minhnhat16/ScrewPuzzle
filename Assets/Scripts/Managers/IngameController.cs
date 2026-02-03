using Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Runtime.CompilerServices;
using UIScript.Dialog;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class IngameController : SingletonMono<IngameController>
    {
        public string playerLevel;
        [SerializeField] public bool isOnMagnet;
        [SerializeField] public bool isOnBomb;
        [SerializeField] private bool isGameOver;
        [SerializeField] private bool itemJustInvoke;
        [SerializeField] private bool itemPerforming;
        [SerializeField] private int currentStar;
        [SerializeField] private int totalStarInLevel;
        internal int requireCount = 3;


        //[SerializeField] private int lastPlayedIndex = -1;
        [SerializeField] private Coroutine playRoutine;

        [SerializeField] private float exp_Current;
        [SerializeField] private Player player;
        [SerializeField] private BoxQueue boxManager;
        [SerializeField] private ArrayScrew arrayScrew;
        [SerializeField] private BackgroundSizeControl bgRender;
        [HideInInspector] public UnityEvent<int> onGoldChanged;
        [HideInInspector] public UnityEvent<int> onGemChanged;
        [HideInInspector] public UnityEvent<float> onExpChange;
        [HideInInspector] public UnityEvent<bool> onCompleteLevel;
        [HideInInspector] public UnityEvent<ItemType,Vector3> onItemInvoke;

        public bool isPause;

        public bool ItemPerforming
        {
            get => itemPerforming;
            set => itemPerforming = value;
        }

        public float ExpCurrent
        {
            get { return exp_Current; }
            set { exp_Current = value; }
        }

        public bool IsGameOver { get => isGameOver; set => isGameOver = value; }
        public UnityEvent<float> onStarChange = new();
        public int CurrentStar { get => currentStar; set => currentStar = value; }
        public int TotalStarInLevel { get => totalStarInLevel; set => totalStarInLevel = value; }
        public UnityEvent OnRainbowGoalCompleted { get; internal set; }

        private Coroutine inputCoroutine;


        private SideMission currentMission;

        private void OnEnable()
        {
            onCompleteLevel.AddListener(CompleteLevel);
            onItemInvoke.AddListener(ItemIvoked);
            if (inputCoroutine == null)
            {
                inputCoroutine = StartCoroutine(ListenForResetInput());
            }
            StarMoveCounter.OnAllStarsFinished += OnAllStarsDone;

        }


        private void OnDisable()
        {
            onCompleteLevel.RemoveListener(CompleteLevel);
            if (inputCoroutine != null)
            {
                StopCoroutine(inputCoroutine);
                inputCoroutine = null;
            }

            onItemInvoke.RemoveAllListeners();
            StarMoveCounter.OnAllStarsFinished -= OnAllStarsDone;

        }

    

       
        private static void CompleteLevel(bool onComplete)
        {
            Debug.Log("Level complete");
            int level = LevelManager.ins.currentLevelID;
            int totalGold = GameManager.instance.GoldCalculation(level);
            WinParam param = new();
            param.totalGold = DataAPIController.instance.GetGold();

            DataAPIController.instance.AddOneCurrent();
            DialogManager.ins.ShowDialog(DialogIndex.WinDialog);
            MissionManager.ins.ProcessLevelComplete();
        }


        private void Start()
        {
            itemJustInvoke = false;
            //Init(() => Debug.Log("INGAME CONTROLLER INIT DONE"));
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                Reset();
            }
            ;
        }

        public void ActivateBG(bool isActive)
        {
            //Debug.Log("ActiveBG " +isActive);
            bgRender.enabled = isActive;
            bgRender.Fit();
        }

        public void Init(Action callback)
        {
            LoadIngameAsset(callback);
        }

        public IEnumerator InitIngameCoroutine(Action callback)
        {
            yield return new WaitForSeconds(0f);
            callback?.Invoke();
        }

        public LayerMask GetLayerMaskForRange(int startLayer, int endLayer)
        {
            LayerMask mask = 0;

            for (var i = startLayer; i <= endLayer; i++)
            {
                mask |= (1 << i); // Set the bit for each layer in the range
            }

            return mask;
        }

        private void ItemIvoked(ItemType item, Vector3 targetpos)
        {
            itemJustInvoke = true;
            ItemController.ins.IsHandlingHammer = itemJustInvoke;
            StartCoroutine(ItemCoroutine(item, targetpos));
        }

        private IEnumerator ItemCoroutine(ItemType itemType,Vector3 targetpos)
        {
            yield return new WaitUntil(() => itemJustInvoke);
            Debug.Log("x " + itemType);
            itemJustInvoke = false;


            DataAPIController.instance.MinusItemByOne(itemType);
            switch (itemType)
            {
                case ItemType.Magnet:
                    ItemController.ins.ClearArrayState.Use(targetpos);
                    break;
                case ItemType.Breaker:
                    AddBox(null);
                    ItemController.ins.RemovePartState.Use(targetpos);
                    break;
                case ItemType.Drill:
                    ItemController.ins.AddOneHold.Use(targetpos);
                    break;
                default:
                    break;
            }
        }
        internal void PauseGame()
        {
            Time.timeScale = 0;
        }
        internal void ResumeGame()
        {
            Time.timeScale = 1;
        }
        private void AddBox(Action callback)
        {
            BoxQueue.ins.AddNewBoxSlot();
            callback?.Invoke();
        }

        private void OnAllStarsDone()
        {

            Debug.Log("All stars done");
            SoundHelper.PlaySFX(SoundManager.SFX.Star_3);
        }
        public void LoadIngameAsset(Action callback)
        {
            StartCoroutine(LoadIngameAssetCoroutine(callback));
        }

        public IEnumerator LoadIngameAssetCoroutine(Action callback = null)
        {
            bool playerInitDone = false;
            StartCoroutine(LoadPlayer(() =>
            {
                playerInitDone = true;
                player.IsInputLocked = false;
                ActivateBG(playerInitDone);

            }));
            //StartCoroutine(LoadBoxManager(() => arrayScrewInitDone = true))
            //
            yield return new WaitUntil(() => playerInitDone);
            Debug.Log("Player init done");
            callback?.Invoke();
        }

        protected IEnumerator LoadPlayer(Action callback)
        {
            if (player != null)
            {
                callback?.Invoke();
                yield break;
            }
            var playerGameObject = Instantiate(Resources.Load<GameObject>($"Prefabs/Player"), transform);
            yield return new WaitUntil(() => playerGameObject != null);
            playerGameObject.TryGetComponent<Player>(out player);
            callback?.Invoke();
        }

        protected IEnumerator LoadBoxManager(Action callback)
        {
            if (boxManager != null) yield return null;
            var boxManagerGO = Instantiate(Resources.Load<GameObject>($"Prefabs/BoxManager"), transform);
            yield return new WaitUntil(() => boxManagerGO != null);
            boxManagerGO.TryGetComponent(out boxManager);
            callback?.Invoke();
        }

        protected IEnumerator LoadArrayScrew(Action callback)
        {
            if (arrayScrew != null) yield return null;
            var arrayScrewObj = Instantiate(Resources.Load<GameObject>($"Prefabs/ArrayScrews"), transform);
            yield return new WaitUntil(() => arrayScrew != null);
            arrayScrewObj.TryGetComponent<ArrayScrew>(out arrayScrew);
            callback?.Invoke();
        }

        public void ClearAllScrewOnArray(Action callback)
        {
            
        }

        private void ClearOneScrew(Action callback)
        {
            itemPerforming = true;
            Debug.LogWarning("clear one screw");
            var screw = LevelManager.ins.ScrewManager.RandomGetOneScrew();
            BoxQueue.ins.onDeletOneScrew?.Invoke(screw);
            callback?.Invoke();
        }


        public void GameEndInvoker(Action callback = null)
        {
            isGameOver = true;
            itemPerforming = false;
            player.IsInputLocked = true;
            ReviveDialogParam param = new();
            param.isRevive = true;
            param.isHasAds = true;// set defaul allway true cus has none ads
            param.totalGold = DataAPIController.instance.GetGold();
            param.currentTicket = DataAPIController.instance.GetTicket();
            // ZenSDK.instance.IsVideoRewardReady();
            // Debug.LogWarning("PREPARE SHOW DIALOG REVIVE DIALOG");
            int activeBoxCount = BoxQueue.ins.activeBoxCount;
            if (activeBoxCount >= 4)
            {
                DialogManager.ins.ShowDialog(DialogIndex.LoseDialog);
                callback?.Invoke();
                return;
            }

            DialogManager.ins.ShowDialog(DialogIndex.ReviveDialog, param, () =>
            {
                // Debug.LogWarning("SHOW DIALOG REVIVE DIALOG");
                //if accepted watch ads invoke no reset level
                // else return and reset current level, -1 heart
                callback?.Invoke();

            });
        }

        private IEnumerator ListenForResetInput()
        {
            while (true)
            {
                // Kiểm tra tổ hợp phím Ctrl + R
                if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
                {
                    Reset(); // Gọi phương thức Reset khi tổ hợp phím được nhấn
                }
                else if (Input.GetKey(KeyCode.Alpha1))
                {
                    Debug.LogWarning("Key 1 pressed");
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.Breaker,Vector3.zero);
                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    Debug.LogWarning("Key 2 pressed");
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.Magnet, Vector3.zero);
                }
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    Debug.LogWarning("Key 3 pressed");
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.Drill, Vector3.zero);
                }
                else if (Input.GetKey(KeyCode.Alpha4))
                {
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.AddBox, Vector3.zero);
                }
                // Chờ một khung hình trước khi kiểm tra tiếp
                yield return null;
            }
        }
        public void StarChanging(int addedStar)
        {
            CurrentStar += addedStar;
            float percentStart = (float)CurrentStar / (float)totalStarInLevel;
            Debug.Log($"StarChanging: CurrentStar={CurrentStar}, TotalStarInLevel={totalStarInLevel}, Percent={percentStart}");

            onStarChange?.Invoke(percentStart);
        }
        public void OnRevive()
        {
            isGameOver = false;
            player.IsInputLocked = false;
            ItemController.ins.IsHandlingHammer = false;
            BoxQueue.ins.UnlockedBox();
        }
        public void ReturnToHome(DialogIndex dialogIndex)
        {

            LevelManager.ins.OnReset();
          SoundManager.instance.PlaySFX(SoundManager.SFX.UI_Normal);
            DialogManager.ins.HideDialog(dialogIndex, () =>
            {
                Debug.Log($"HideDialog {dialogIndex} ");
               
            
            });

            LoadSceneManager.ins.LoadSceneByName("Buffer", () =>
            {
                DialogManager.ins.HideAllDialog();
                Debug.Log("Switch view mainscreenview ");
                MainScreenViewParam param = new();
                param.totalGold = GameManager.instance.GetPlayerGold();
                ViewManager.Instance.SwitchView(ViewIndex.MainScreenView, param);
                /*  
                DialogManager.Instance.ShowDialog(DialogIndex.LableChooseDialog, null, () =>
                {
                });*/
            });
        }
        public void OnGameOver()
        {
            // minuss 1 life heart
            IsGameOver = true;
            CurrentStar = 0;
            int currentLevel = LevelManager.ins.currentLevelID;
            Player.instance.IsInputLocked = true;
            LevelManager.ins.OnReset();
            ArrayScrew.Instance.ClearAllScrewsOnArray();
            BoxQueue.ins.OnReset();
            LevelManager.ins.LoadLevel(currentLevel);
        }
        public void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1); // Unity's Random.Range
                                                                      // Swap elements
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        internal void ShowAddBox()
        {
            ReviveDialogParam param = new();
            param.isRevive = false;
            param.isHasAds = true;
            DialogManager.ins.ShowDialog(DialogIndex.ReviveDialog, param);

        }

        public void SetSideMission(SideMission mission)
        {
            currentMission = mission;

            if (mission != null)
            {
                MissionParam param = new MissionParam();
                param.SideMission = mission;
                param.current = 0;
                param.target = mission.requiredCount;
                DialogManager.ins.ShowDialog(DialogIndex.MissionDialog, param);
            }

        }

    }
}