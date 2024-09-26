using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class Player : MonoBehaviour
    {
        public static Player instance;
        public UnityEvent<Screw.Screw> onScrewClicked = new();
        [SerializeField] private float lastClickTime = 0.5f;
        [SerializeField] private float clickCooldown;
        [SerializeField] private bool canClick;
        [SerializeField] private Screw.Screw CurrentScrew;
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
                    CurrentScrew = hit.collider.GetComponent<Screw.Screw>();
                    if (CurrentScrew != null && !CurrentScrew.IsMoving())
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

            var screwLayerMask = LayerMask.GetMask("Screw");

            // Perform a raycast and return only the first hit object
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, screwLayerMask);

            if (hit.collider != null)
            {
                // Check if the hit object has a Screw component
                var screwComponent = hit.collider.GetComponent<Screw.Screw>();

                if (screwComponent != null)
                {
                    // Return the first hit screw object
                    return hit;
                }
            }

            return new RaycastHit2D(); // Return an empty hit if no screw is found
        }

        // RaycastHit2D GetScrewByRaycastHit2D(Camera cam)
        // {
        //     var ray = cam.ScreenPointToRay(Input.mousePosition);
        //     Debug.Log("Ray Origin: " + ray.origin + " Ray Direction: " + ray.direction);
        //
        //     var screwLayerMask = LayerMask.GetMask("Screw");
        //
        //     // Get all the objects hit by the raycast
        //     var hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, screwLayerMask);
        //
        //     RaycastHit2D selectedHit = new RaycastHit2D();
        //     int highestLayerValue = int.MinValue; // Initialize to minimum value to find the highest
        //
        //     foreach (var hit in hits)
        //     {
        //         // Assume the Screw component contains the enum 'ScrewLayer'
        //         var screwComponent = hit.collider.GetComponent<Screw.Screw>(); // Assuming 'Screw' is the script that contains the enum layer
        //
        //         if (screwComponent != null)
        //         {
        //             int screwLayerValue = (int)screwComponent.LayerMask; // Convert enum to int for comparison
        //
        //             // Compare and keep the one with the highest enum layer value
        //             if (screwLayerValue > highestLayerValue)
        //             {
        //                 highestLayerValue = screwLayerValue;
        //                 selectedHit = hit; // Keep the RaycastHit2D of the highest layer screw
        //             }
        //         }
        //     }
        //
        //     return selectedHit;
        // }

    }
}