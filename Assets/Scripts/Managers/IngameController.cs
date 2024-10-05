using Ingame;
using System;
using System.Collections;
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

        [SerializeField] private float exp_Current;
        [SerializeField] private Player player;
        [SerializeField] private BoxQueue boxManager;
        [SerializeField] private ArrayScrew arrayScrew;

        [HideInInspector] public UnityEvent<int> onGoldChanged;
        [HideInInspector] public UnityEvent<int> onGemChanged;
        [HideInInspector] public UnityEvent<float> onExpChange;
        [HideInInspector] public UnityEvent<bool> onCompleteLevel;
        [HideInInspector] public UnityEvent<ItemType> onItemInvoke;
        [SerializeField] private bool itemJustInvoke;
        [SerializeField] private bool itemPerforming;
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
        }



        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            itemJustInvoke = false;
            Init(() => Debug.Log("INGAME CONTROLLER INIT DONE"));
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.LeftControl))
            {
                Reset();
            };
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
                    AddHold(null);
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
            bool arrayScrewInitDone = false;
            bool boxQueueInitDone = false;
            bool playerInitDone = false;
            StartCoroutine(LoadArrayScrew(() => boxQueueInitDone = true));
            StartCoroutine(LoadPlayer(() => playerInitDone = true));
            StartCoroutine(LevelManager.Instance.LoadLevel(Convert.ToInt32(playerLevel), () =>
            {
                Debug.Log("Load Level Done");
            }));
            StartCoroutine(LoadBoxManager(() => arrayScrewInitDone = true));
            yield return new WaitUntil(() => arrayScrewInitDone && boxQueueInitDone && playerInitDone);
            callback?.Invoke();
        }
        protected IEnumerator LoadPlayer(Action callback)
        {

            var playerGameObject = Instantiate(Resources.Load<GameObject>($"Prefabs/Player"), transform);
            yield return new WaitUntil(() => playerGameObject != null);
            if (playerGameObject != null) this.player = playerGameObject.GetComponent<Player>();
            callback?.Invoke();
        }
        protected IEnumerator LoadBoxManager(Action callback)
        {

            var boxManagerGameobject = Instantiate(Resources.Load<GameObject>($"Prefabs/BoxManager"), transform);
            yield return new WaitUntil(() => boxManagerGameobject != null);
            if (boxManagerGameobject != null) this.boxManager = boxManagerGameobject.GetComponent<BoxQueue>();
            callback?.Invoke();
        }
        protected IEnumerator LoadArrayScrew(Action callback)
        {
            var arrayScrewObj = Instantiate(Resources.Load<GameObject>($"Prefabs/ArrayScrews"), transform);
            yield return new WaitUntil(() => arrayScrew != null);
            if (arrayScrewObj != null) this.arrayScrew = arrayScrewObj.GetComponent<ArrayScrew>();
            callback?.Invoke();
        }
        private void ClearAllScrewOnArray(Action callback)
        {
            ArrayScrew.Instance.ClearAllScrewsOnArray();
            callback?.Invoke();
        }
        private void ClearOneScrew(Action callback)
        {
            callback?.Invoke();
        }
        public void Reset()
        {
            SceneManager.LoadScene("SampleScene");
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
                else if (Input.GetKey(KeyCode.Alpha3) )
                {
                    Debug.LogWarning("Key 3 pressed");
                    itemJustInvoke = true;

                    onItemInvoke.Invoke(ItemType.ClearOneScrew);
                }
                // Chờ một khung hình trước khi kiểm tra tiếp
                yield return null;
            }
        }
    }
}



