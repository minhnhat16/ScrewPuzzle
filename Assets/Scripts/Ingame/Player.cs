using Ingame.Board;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Ingame
{
    public class Player : BaseInputHandler
    {
        public static Player instance;

        [SerializeField] private List<Screw.Screw> _screw;
        [SerializeField] private Queue<Screw.Screw> screwQueue;

        public UnityEvent<Screw.Screw> onScrewClicked = new();

        protected override void Awake()
        {
            base.Awake();
            instance =this;
            screwQueue = new Queue<Screw.Screw>();
            _screw = new List<Screw.Screw>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            onScrewClicked.AddListener(ScrewClicked);
        }

        protected override void OnDisable()
        {
            onScrewClicked.RemoveListener(ScrewClicked);
            base.OnDisable();
        }
        bool IsPointerOverBlockingUI(Vector2 screenPos)
        {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = screenPos;

            List<RaycastResult> results = new();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var r in results)
            {
                // chỉ block nếu KHÔNG phải spotlight
                if (r.gameObject.GetComponent<SpotlightRaycastBlocker>() == null)
                    return true;
            }

            return false;
        }
        protected override void HandleInput(Vector3 screenPos)
        {
            bool poiterOver = EventSystem.current != null && IsPointerOverBlockingUI(screenPos);
            bool isHandlingItem = ItemController.ins.IsHandlingHammer;
            //Debug.Log("is pointer over UI: " + poiterOver + ", handling item " + isHandlingItem);
            if (poiterOver)
                return;


            if (isHandlingItem)
            {
                var part = PickAtScreenPos<BasePart>(screenPos, "Part");
                if (part != null)
                {
                    ItemController.ins.IsHandlingHammer = false;
                    var pos = mainCam.ScreenToWorldPoint(screenPos) + Vector3.right * 1.4f;
                    ItemController.ins.RemovePartState.Peform(part, pos);
                }
                return;
            }

            var screw = PickAtScreenPosScrew<Screw.Screw>(screenPos, "Player");

            Debug.Log("Clicked screw " + screw);
            if (screw != null)
            {
                _screw.Add(screw);
                onScrewClicked?.Invoke(screw);
                return;
            }

            var box = PickAtScreenPos<BoxThreeHold>(screenPos, "Player");

           // Debug.Log("is box null" + box);
            if (box != null && box.IsLocked)
            {
                IngameController.ins.ShowAddBox();
            }
        }

        private void ScrewClicked(Screw.Screw screw)
        {
            screwQueue.Enqueue(screw);
            _screw.Clear();

            var layermanager = LevelManager.ins.layerManager;
            layermanager.RemoveScrewOnDict(screw, screw.sortingOrder);
            ArrayScrew.Instance.AddScrew(screw);
        }
    }
}
