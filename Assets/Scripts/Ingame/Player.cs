using Ingame.Screw;
using System;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Experimental.GraphView.GraphView;

namespace Ingame
{
    public class Player : BaseInputHandler, IPlayer
    {
        [SerializeField] private MonoBehaviour screwServiceBehaviour;
        private IScrewInteractionService _screwService;

        public UnityEvent<ScrewController> OnScrewClicked = new();

        public event Action<ScrewController> OnScrewSelected;

        protected override void Awake()
        {
            base.Awake();
            _screwService = (IScrewInteractionService)screwServiceBehaviour;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (InputRouter.Instance != null)
                InputRouter.Instance.OnTappableTapped += HandleTappableTapped;
        }

        protected override void OnDisable()
        {
            if (InputRouter.Instance != null)
                InputRouter.Instance.OnTappableTapped -= HandleTappableTapped;

            base.OnDisable();
        }

        private void HandleTappableTapped(ITappable tappable, Vector2 screenPos)
        {
            if (tappable is not ScrewController screw)
                return;

            _screwService.HandleScrewSelected(screw);
            OnScrewClicked?.Invoke(screw);
        }

        public void LockInput()
        {
            IsInputLocked = true;
        }

        public void UnlockInput()
        {
            IsInputLocked = false;
        }

        protected override void HandleInput(Vector3 screenPos)
        {
            if (IsClickOverUI()) return;

            var screw = PickAtScreenPos<ScrewController>(screenPos, "Player");
            if (screw != null)
                OnScrewSelected?.Invoke(screw);
        }
    }
}