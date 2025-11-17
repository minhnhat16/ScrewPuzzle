using Ingame.Board;
using Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            instance = this;
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
            bool isHandlingItem = ItemController.ins.IsHandlingItem;

            if (isHandlingItem)
            {
                var part = PickAtScreenPos<BasePart>(screenPos, "Part");
                if (part != null)
                {
                    ItemController.ins.IsHandlingItem = false;
                    LevelManager.Instance.RemovePartItem(part);
                }
                return;
            }

            var screw = PickAtScreenPos<Screw.Screw>(screenPos, "Player");
            if (screw != null)
            {
                _screw.Add(screw);
                onScrewClicked?.Invoke(screw);
            }
        }

        private void ScrewClicked(Screw.Screw screw)
        {
            screwQueue.Enqueue(screw);
            _screw.Clear();

            var layermanager = LevelManager.Instance.layerManager;
            layermanager.RemoveScrewOnDict(screw, screw.layerMask);
            ArrayScrew.Instance.AddScrew(screw);
        }
    }
}
