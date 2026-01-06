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

        protected override void HandleInput(Vector3 screenPos)
        {
            bool poiterOver = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            bool isHandlingItem = ItemController.ins.IsHandlingHammer;
            Debug.Log("is pointer over UI: " + poiterOver + ", handling item " + isHandlingItem);
            if (poiterOver)
                return;


            if (isHandlingItem)
            {
                var part = PickAtScreenPos<BasePart>(screenPos, "Part");
                if (part != null)
                {
                    ItemController.ins.IsHandlingHammer = false;
                    ItemController.ins.RemovePartState.Peform(part);
                }
                return;
            }

            var screw = PickAtScreenPos<Screw.Screw>(screenPos, "Player");
            if (screw != null)
            {
                _screw.Add(screw);
                onScrewClicked?.Invoke(screw);
                return;
            }

            var box = PickAtScreenPos<BoxThreeHold>(screenPos, "Player");

            Debug.Log("is box null" + box);
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
