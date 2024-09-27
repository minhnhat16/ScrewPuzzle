using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        [SerializeField] private float clickCooldown = 0.5f; // Cooldown time between clicks
        [SerializeField] private bool canClick = true;        // Flag to check if the player can click
        [SerializeField] private Screw.Screw CurrentScrew;
        [SerializeField] private Camera mainCam;
        [HideInInspector] public UnityEvent onPlayerClick = new();  // Custom event for player clicks
        [HideInInspector] public UnityEvent<Screw.Screw> onScrewClicked;
        private Coroutine inputCoroutine;
        private void OnEnable()
        {
            // Only add the listener if it hasn't been added before
            if (onScrewClicked != null)
            {
                onScrewClicked.RemoveListener(ScrewClicked); // Ensuring no duplicate listeners
                onScrewClicked.AddListener(ScrewClicked);
            }

            if (inputCoroutine == null)
            {
                inputCoroutine = StartCoroutine(WaitForInput());
            }

        }

        private void OnDisable()
        {
            // Remove the listener when the object is disabled to avoid multiple calls
            if (onScrewClicked != null)
            {
                onScrewClicked.RemoveListener(ScrewClicked);
            }
            if (inputCoroutine != null)
            {
                StopCoroutine(inputCoroutine);
                inputCoroutine = null;
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
        }
        private IEnumerator WaitForInput()
        {
            while (true)
            {
                // Handle only one type of input based on the platform
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

                yield return null; // Wait for the next frame to check input again
            }
        }


        private void HandleInput(Vector3 screenPosition)
        {
            // Convert screen position to world position
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // Cast a 2D ray (actually a point in 2D space) from the world position
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero,Mathf.NegativeInfinity);

            if (hit.collider != null)
            {
                var clickedObject = hit.collider.gameObject;

                // Example: Handle interaction with a Screw
                if (clickedObject.CompareTag("Player"))
                {
                    onScrewClicked?.Invoke(clickedObject.GetComponent<Screw.Screw>());
                }

                Debug.Log("2D Game object clicked: " + hit.collider.name);
            }
            else
            {
                Debug.LogWarning("No 2D object was hit by the raycast.");
            }
        }

        private void ScrewClicked(Screw.Screw screw)
        {
            Debug.LogWarning("Screw Clicked");
            screw.OnScrewClicked();
        }
        
    }
}
