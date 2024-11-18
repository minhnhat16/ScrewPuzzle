using Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            var hits = Physics2D.RaycastAll(worldPosition, Vector2.zero, Mathf.Infinity);

            // Sort hits by Z position (assuming all objects have a Transform)
            var sortedHits = hits.OrderByDescending(hit => hit.transform.position.z).ToArray();

            foreach (var hit in sortedHits)
            {
                if (hit.collider != null)
                {
                    var clickedObject = hit.collider.gameObject;

                    if (clickedObject.CompareTag("Player"))
                    {
                        var screw = clickedObject.GetComponent<Screw.Screw>();
                        _screw.Add(screw);
                        Debug.Log("Player clicked: " + clickedObject.name);
                        // return; // Exit after the first valid click, if needed
                    }
                }
            }
            if (_screw.Count == 0) return;
            var firstScrew = _screw.Last();
            if (firstScrew == null) return;
            onScrewClicked?.Invoke(firstScrew);

            Debug.LogWarning("No valid 2D object was clicked.");
        }



        private void ScrewClicked(Screw.Screw screw)
        {
            // Debug.LogWarning("Screw Clicked");
            screwQueue.Enqueue(screw);
            _screw.Clear();
            // Start processing the queue if it's not already processing
            if (processCoroutine == null)
            {
                ArrayScrew.Instance.AddScrew(screw);
            }
        }


    }
}
