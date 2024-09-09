using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        public UnityEvent<Screw> onScrewClicked = new();
        [SerializeField] private float lastClickTime = 0.5f;
        [SerializeField] private float clickCooldown;
        [SerializeField] private bool canClick;
        [SerializeField] private Screw CurrentScrew;
        [SerializeField] Camera mainCam;
        
        public bool CanClick {get => canClick; set => canClick = value;}
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

        private void Update()
        {
            canClick = true;
            if (Input.GetMouseButtonDown(0) && canClick)
            {
                Debug.LogWarning("On mouse down button");

                // Đặt flag về false để ngăn chặn việc click liên tiếp
                canClick = false;

                // Raycast to find the screw
                var hit = GetScrewByRaycastHit2D(mainCam);
                if (hit.collider != null)
                {
                    CurrentScrew = hit.collider.GetComponent<Screw>();
                    if (CurrentScrew != null)
                    {
                        Debug.LogWarning("Screw found!");
                       CurrentScrew.OnScrewClicked();
                    }
                    else
                    {
                        Debug.LogWarning("No screw found at the clicked location.");
                    }
                }
                else
                {
                    Debug.LogWarning("Raycast did not hit any object.");
                }
            }

        }

        RaycastHit2D GetScrewByRaycastHit2D(Camera cam)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.Log("Ray Origin: " + ray.origin + " Ray Direction: " + ray.direction);

            var screwLayerMask = ScrewManager.instance.LayerMask; // Ensure LayerMask is properly assigned
            Debug.Log("Using LayerMask: " + screwLayerMask.value);

            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, screwLayerMask);
            return hit;
        }

    }
}