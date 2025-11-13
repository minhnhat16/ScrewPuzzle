using Ingame.Board;
using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        [SerializeField] private float clickCooldown = 0.5f; // Cooldown time between clicks
        [SerializeField] private bool canClick;        // Flag to check if the player can click

        public bool CanClick
        {
            get => canClick;
            set => canClick = value;
        }
        [SerializeField] private Screw.Screw CurrentScrew;
        [SerializeField] private Camera mainCam;
        [SerializeField] private List<Screw.Screw> _screw;
        [SerializeField] private Queue<Screw.Screw> screwQueue;
        [HideInInspector] public UnityEvent onPlayerClick = new();  // Custom event for player clicks
        [HideInInspector] public UnityEvent<Screw.Screw> onScrewClicked;
        private Coroutine inputCoroutine;
        private Coroutine processCoroutine;
        private void OnEnable()
        {
            if (onScrewClicked != null)
            {
                onScrewClicked.RemoveListener(ScrewClicked);
                onScrewClicked.AddListener(ScrewClicked);
            }

            inputCoroutine ??= StartCoroutine(WaitForInput());
        }

        private void OnDisable()
        {
            if (onScrewClicked != null)
            {
                onScrewClicked.RemoveListener(ScrewClicked);
            }
            if (inputCoroutine != null)
            {
                StopCoroutine(inputCoroutine);
                inputCoroutine = null;
            }

            if (processCoroutine != null)
            {
                StopCoroutine(processCoroutine);
                processCoroutine = null;
            }
        }

        private void Awake()
        {
            instance = this;
            canClick = true;
            CurrentScrew = null;
        }

        private void Start()
        {
            mainCam = Camera.main;
            screwQueue = new Queue<Screw.Screw>();
        }

        private IEnumerator WaitForInput()
        {
            yield return new WaitUntil(() => IngameController.Instance != null);
            while (true)
            {
                if (!IngameController.Instance.isPause)
                {
#if UNITY_EDITOR || UNITY_STANDALONE    
                    if (Input.GetMouseButtonDown(0))
                    {
                        HandleInput(Input.mousePosition);
                    }
#elif UNITY_ANDROID || UNITY_IOS
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    HandleInput(Input.GetTouch(0).position);
                }
#endif
                }


                yield return null;
            }
        }
        private void HandleInput(Vector3 screenPosition)
        {
            // Cache the main camera
            if (mainCam == null) return;
            bool isHadlingItem = ItemController.ins.IsHandlingItem;
            // Convert screen position to world position
            Vector2 worldPosition = mainCam.ScreenToWorldPoint(screenPosition);
            string tag = isHadlingItem ? "Part" : "Player";
            // Perform a 2D raycast to detect objects at the click position
            var hits = Physics2D.RaycastAll(worldPosition, Vector2.zero, Mathf.Infinity);

            GameObject clickedObject = null;
            Screw.Screw foundScrew = null;
            float highestZ = float.MinValue;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                var obj = hit.collider.gameObject;


                // Only consider objects with the "Player" tag
                if (obj.CompareTag(tag))
                {
                    float z = obj.transform.position.z;

                    // Prioritize the object with the highest Z position
                    if (z > highestZ)
                    {
                        clickedObject = obj;
                        highestZ = z;
                    }
                }
            }

            // If a valid object was found, process it
            if (clickedObject != null)
            {

                if (isHadlingItem)
                {
                    var obj = clickedObject.GetComponent<BasePart>();
                    if (obj)
                    {
                        ItemController.ins.IsHandlingItem = false;
                        LevelManager.Instance.RemovePartItem(obj);
                    }
                    return;
                }
                else
                {
                    foundScrew = clickedObject.GetComponent<Screw.Screw>();
                    if (foundScrew != null)
                    {
                        _screw.Add(foundScrew);
                        onScrewClicked?.Invoke(foundScrew);
                    }
                }

            }
        }

        private void ScrewClicked(Screw.Screw screw)
        {
            // Debug.LogWarning("Screw Clicked");
            screwQueue.Enqueue(screw);
            _screw.Clear();
            // Start processing the queue if it's not already processing
            if (processCoroutine == null)
            {
                var layermanager = LevelManager.Instance.layerManager;
                layermanager.RemoveScrewOnDict(screw, screw.layerMask);
                ArrayScrew.Instance.AddScrew(screw);
            }
        }

        private void PartClicked(BasePart part)
        {
            if (part == null) return;

        }
    }
}
