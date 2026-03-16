using Ingame.Screw;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame
{
    public class Player : BaseInputHandler, IPlayer
    {
        private IScrewInteractionService _screwService;

        public UnityEvent<ScrewController> OnScrewClicked = new();
        public event Action<ScrewController> OnScrewSelected;

        public void Inject(IScrewInteractionService screwService)
        {
            _screwService = screwService;
            Debug.Log("[Player] IScrewInteractionService injected.");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (InputRouter.Instance != null)
                InputRouter.Instance.OnTappableTapped += HandleTappableTapped;
            else
                Debug.LogWarning("[Player] InputRouter.Instance is null in OnEnable.");
        }

        protected override void OnDisable()
        {
            if (InputRouter.Instance != null)
                InputRouter.Instance.OnTappableTapped -= HandleTappableTapped;
            base.OnDisable();
        }

        private void HandleTappableTapped(ITappable tappable, Vector2 screenPos)
        {
            if (tappable == null) return;
            if (_screwService == null)
            {
                Debug.LogError("[Player] _screwService is null. Call Inject() from ScrewGameBootstrapper.");
                return;
            }

            ScrewController screw = tappable as ScrewController;
            if (screw == null && tappable is MonoBehaviour mb)
                screw = mb.GetComponent<ScrewController>()
                     ?? mb.GetComponentInParent<ScrewController>()
                     ?? mb.GetComponentInChildren<ScrewController>();

            if (screw == null)
            {
                Debug.LogWarning($"[Player] Could not resolve ScrewController from tappable: {tappable}");
                return;
            }

            // Guard: check IsInteractable trước khi forward xuống service
            // Đây là điểm chặn chính — bao gồm cả tutorial block
            if (!screw.IsInteractable)
            {
                Debug.Log($"[Player] Screw '{screw.name}' không interactable — bỏ qua.");
                return;
            }

            _screwService.HandleScrewSelected(screw);
            OnScrewClicked?.Invoke(screw);
        }

        public void LockInput() => IsInputLocked = true;
        public void UnlockInput() => IsInputLocked = false;

        protected override void HandleInput(Vector3 screenPos)
        {
            if (IsClickOverUI()) return;
            var screw = PickAtScreenPos<ScrewController>(screenPos, "Player");
            if (screw != null) OnScrewSelected?.Invoke(screw);
        }
    }
}