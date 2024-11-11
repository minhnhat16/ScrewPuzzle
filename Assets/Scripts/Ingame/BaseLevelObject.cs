using Ingame.Screw;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public class BaseLevelObject : MonoBehaviour
    {
        public bool canMove = true; // Flag to allow/disallow movement
        public float moveSpeed = 10f; // Speed at which the object follows the mouse

        [SerializeField] private bool isDragging = false; // Track if the object is being dragged
        [SerializeField] private bool isEditColor  =false;
        private Camera mainCamera;
        [SerializeField] private ScrewManager screwManager;
        [SerializeField] private Dictionary<string,TwoHingeScrew> screwByPart;

        public ScrewManager ScrewManager
        {
            get => screwManager;
            set => screwManager = value;
        }
        public bool IsEditColor { get => isEditColor; set => isEditColor = value; }

        private void Start()
        {
            mainCamera = Camera.main; // Get the main camera to convert screen coordinates to world coordinates
        }

        // Update is called once per frame
        void Update()
        {
            HandleMouseMovement();
        }

        // Detect click to start dragging
        private void OnMouseDown()
        {
            if (canMove && LevelMaker.instance.isEditPartPosition)
            {
                isDragging = true; // Start dragging when the mouse button is pressed
                Debug.Log("Object clicked and started dragging");
            }
            else if (LevelMaker.instance.isEditPartColor)
            {
                IsEditColor = true;
                var part = GetComponent<BasePart>();
                ApplyColor.instance.ApplyColorToSprite(part);// Start dragging when the mouse button is pressed
                Debug.Log("Object clicked and started dragging");
            }
            else
            {
                Debug.Log("Object clicked but cannot move due to the flag being disabled");
            }
        }

        // Detect mouse release to stop dragging
        private void OnMouseUp()
        {
            isDragging = false; // Stop dragging when the mouse button is released
            Debug.Log("Object stopped dragging");
        }

        // Method to move the object with the mouse
        private void HandleMouseMovement()
        {
            if (isDragging)
            {
                // Get the mouse position in world space
                Vector3 mousePosition = Input.mousePosition;
                mousePosition.z = 10f; // Set a distance from the camera if it's 2D (adjust as needed)

                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

                // Move the object smoothly to follow the mouse position
                transform.position = Vector3.Lerp(transform.position, worldPosition, moveSpeed * Time.deltaTime);
            }

        }

        public void SaveIvoke()
        {
            Debug.Log("Save invoked for object: " + gameObject.name);
            // Implement save logic here if necessary
        }
    }
}
