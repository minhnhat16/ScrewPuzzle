using Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DataBase;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class IngameController : MonoBehaviour
    {
        public static IngameController Instance;
        public string playerLevel;
        [SerializeField] public bool isOnMagnet;
        [SerializeField] public bool isOnBomb;
        [SerializeField] private bool isGameOver;
        [SerializeField] private bool itemJustInvoke;
        [SerializeField] private bool itemPerforming;
        [SerializeField] private int currentStar;
        [SerializeField] private int totalStarInLevel;

        [SerializeField] private float exp_Current;
        [SerializeField] private Player player;
        [SerializeField] private BoxQueue boxManager;
        [SerializeField] private ArrayScrew arrayScrew;
        [SerializeField] private SpriteRenderer bgRender;
        [HideInInspector] public UnityEvent<int> onGoldChanged;
        [HideInInspector] public UnityEvent<int> onGemChanged;
        [HideInInspector] public UnityEvent<float> onExpChange;
        [HideInInspector] public UnityEvent<bool> onCompleteLevel;
        [HideInInspector] public UnityEvent<ItemType> onItemInvoke;
       
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

        private Coroutine inputCoroutine;

        private void OnEnable() 
        {
            onCompleteLevel.AddListener(CompleteLevel);
            onItemInvoke.AddListener(ItemIvoked);
            if (inputCoroutine == null)
            {
                inputCoroutine = StartCoroutine(ListenForResetInput());
            }
        }


        private void OnDisable()
        {
            onCompleteLevel.RemoveListener(CompleteLevel);
            if (inputCoroutine != null)
            {
                StopCoroutine(inputCoroutine);
                inputCoroutine = null;
            }
        }

        private static void CompleteLevel(bool onComplete)
        {
            Debug.Log("Level complete");
            int level = LevelManager.Instance.currentLevelID;
            int totalGold = GameManager.instance.GoldCalculation(level);
            DialogManager.Instance.HideAllDialog();

            WinParam param = new();
            param.totalGold = GameManager.instance.GetPlayerGold();
            DialogManager.Instance.ShowDialog(DialogIndex.WinDialog);
        }


        private void Awake()
        {
            Instance = this;
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
            Debug.Log("ActiveBG " +isActive);
            bgRender.enabled = isActive ;
        }

        public void Init(Action callback)
        {
            LoadIngameAsset(callback);
        }

        public IEnumerator InitIngameCoroutine(Action callback)
        {
            yield return new WaitForSeconds(0f);

            // Callback when initialization is done
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

        private void ItemIvoked(ItemType item)
        {
            itemJustInvoke = true;
            StartCoroutine(ItemCoroutine(item));
        }

        private IEnumerator ItemCoroutine(ItemType itemType)
        {
            yield return new WaitUntil(() => itemJustInvoke);
            Debug.Log("Item couroutine " + itemType);
            itemJustInvoke = false;

            switch (itemType)
            {
                case ItemType.AddHold:
                    AddHold(() =>
                    {

                    });
                    break;
                case ItemType.AddBox:
                    AddBox(null);
                    break;
                case ItemType.ClearOneScrew:
                    ClearOneScrew(null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
            }
        }

        private void AddHold(Action callback)
        {
            itemPerforming = true;
            ArrayScrew.Instance.SpawnNewHold();
            callback?.Invoke();
        }

        internal void PauseGame()
        {
            Time.timeScale = 0;
        }
        internal void ResumeGame()
        {
            Time.timeScale =1;
        }
        private void AddBox(Action callback)
        {
            BoxQueue.Instance.AddNewBoxSlot();
            callback?.Invoke();
        }

        public void LoadIngameAsset(Action callback)
        {
            StartCoroutine(LoadIngameAssetCoroutine(callback));
        }

        public IEnumerator LoadIngameAssetCoroutine(Action callback = null)
        {
            //bool arrayScrewInitDone = false;
            //bool boxQueueInitDone = false;
            bool playerInitDone = false;
            /*StartCoroutine(LoadArrayScrew(() => boxQueueInitDone = true));*/
            StartCoroutine(LoadPlayer(() =>
            {
                playerInitDone = true;
                ActivateBG(playerInitDone);

            }));
            //StartCoroutine(LoadBoxManager(() => arrayScrewInitDone = true));
            yield return new WaitUntil(() => playerInitDone);
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

        private void ClearAllScrewOnArray(Action callback)
        {
            ArrayScrew.Instance.ClearAllScrewsOnArray();
            callback?.Invoke();
        }

        private void ClearOneScrew(Action callback)
        {
            itemPerforming = true;
            Debug.LogWarning("clear one screw");
            var screw = LevelManager.Instance.ScrewManager.RandomGetOneScrew();
            BoxQueue.Instance.onDeletOneScrew?.Invoke(screw);
            callback?.Invoke();
        }

        public void Reset()
        {
            //SceneManager.LoadScene("BootScene");
        }

        public void GameEndInvoker(Action callback = null)
        {
            isGameOver = true;
            itemPerforming = false;
            player.CanClick = false;
            ReviveDialogParam param = new ReviveDialogParam();
            param.isRevive = false;
            param.isHasAds = true;// set defaul allway true cus has none ads
            param.totalGold = GameManager.instance.GetPlayerGold();
            // ZenSDK.instance.IsVideoRewardReady();
            Debug.LogWarning("PREPARE SHOW DIALOG REVIVE DIALOG");

            DialogManager.Instance.ShowDialog(DialogIndex.ReviveDialog, param, () =>
            {
                Debug.LogWarning("SHOW DIALOG REVIVE DIALOG");
                //if accepted watch ads invoke no reset level
                // else return and reset current level, -1 heart
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
                    onItemInvoke.Invoke(ItemType.AddBox);
                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    Debug.LogWarning("Key 2 pressed");
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.AddHold);
                }
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    Debug.LogWarning("Key 3 pressed");
                    itemJustInvoke = true;
                    onItemInvoke.Invoke(ItemType.ClearOneScrew);
                }

                // Chờ một khung hình trước khi kiểm tra tiếp
                yield return null;
            }
        }
        public void StarChanging(int addedStar)
        {
            CurrentStar += addedStar;
            float percentStart = (float)CurrentStar / (float)totalStarInLevel;
            Debug.LogWarning($"Star Changing {percentStart}");

            onStarChange?.Invoke(percentStart);
        }
        public void OnRevive()
        {
            throw new NotImplementedException();
        }

        public void OnGameOver()
        {
            // minuss 1 life heart
            IsGameOver = true;
            int currentLevel = LevelManager.Instance.currentLevelID;
            Player.instance.CanClick = false;
            LevelManager.Instance.Reset();
            ArrayScrew.Instance.ClearAllScrewsOnArray();
            BoxQueue.Instance.Reset();
            LevelManager.Instance.LoadLevel(currentLevel);
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
    }
}