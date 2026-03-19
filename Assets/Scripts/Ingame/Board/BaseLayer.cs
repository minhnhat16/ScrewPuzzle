using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ingame.Board
{
    public class BaseLayer : MonoBehaviour
    {
        public LayerManager layerManager;
        [SerializeField] private Transform _transform;
        [SerializeField] private GameObject _gameObject;
        public List<BasePart> parts = new List<BasePart>();
        [SerializeField] private int dropPart = 0;
        public Rigidbody2D ridBody;
        public UnityEvent<bool, BasePart> onPartClear = new();

        [SerializeField]
        private bool isLayerClear;

        // Track đúng những part đã được đăng ký listener
        // để UnregisterPartListeners() luôn cleanup đủ, kể cả khi parts list bị mutate
        private readonly List<BasePart> _registeredParts = new List<BasePart>();

        // Flag chống re-register trong cùng lifecycle
        private bool _listenersRegistered = false;

        public Transform Transform
        {
            get => _transform;
            set => _transform = value;
        }
        public GameObject GameObject
        {
            get => _gameObject;
            set => _gameObject = value;
        }
        public List<BasePart> Parts
        {
            get => parts;
            set => parts = value;
        }
        public int ActivePartCount
        {
            get => dropPart;
            set => dropPart = value;
        }
        public bool IsLayerClear { get => isLayerClear; set => isLayerClear = value; }
        public bool IsHidden { get;  set; }

        // ──────────────────────────────────────────────────────────────────
        // UNITY LIFECYCLE
        // ──────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            IsLayerClear = false;
        }

        private void OnDisable()
        {
            Debug.Log("Layer OnDisable — Unregistering part listeners to prevent leaks.");  
            UnregisterPartListeners();
        }

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _gameObject = _transform.gameObject;
            GetAllPartsInLayer();
        }

        private void Start()
        {
            if (layerManager == null)
                layerManager = GetComponentInParent<LayerManager>();

            dropPart = 0;
        }

        // ──────────────────────────────────────────────────────────────────
        // LISTENER MANAGEMENT
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Đăng ký listener cho tất cả part trong layer.
        /// An toàn khi gọi nhiều lần — tự unregister lần trước trước khi register lại.
        /// KHÔNG dùng RemoveAllListeners() vì sẽ xóa cả listener của object khác.
        /// </summary>
        public void RegisterPartListener()
        {
            // Luôn unregister trước để tránh duplicate
            UnregisterPartListeners();

            _registeredParts.Clear();

            foreach (var part in parts)
            {
                if (part == null) continue;
                part.OnStateChanged.AddListener(OnPartFallingChanged);
                _registeredParts.Add(part);
            }

            _listenersRegistered = true;
            Debug.Log($"[BaseLayer] {gameObject.name} registered {_registeredParts.Count} part listeners.");
        }

        /// <summary>
        /// Hủy đăng ký listener chỉ của layer này — không ảnh hưởng listener từ object khác.
        /// Dùng _registeredParts snapshot thay vì parts list (vì parts có thể bị mutate).
        /// </summary>
        private void UnregisterPartListeners()
        {
            if (!_listenersRegistered) return;

            foreach (var part in _registeredParts)
            {
                if (part == null) continue;
                part.OnStateChanged.RemoveListener(OnPartFallingChanged);
            }

            _registeredParts.Clear();
            _listenersRegistered = false;
        }

        // ──────────────────────────────────────────────────────────────────
        // PART STATE CALLBACKS
        // ──────────────────────────────────────────────────────────────────

        private void OnPartFallingChanged(bool isFalling, BasePart part)
        {
            Debug.Log($"[BaseLayer] Part falling changed: {isFalling} | part: {part?.name}");
            CheckAllPartActive(isFalling, part);
        }

        private void CheckAllPartActive(bool isFall, BasePart part)
        {
            Debug.Log("[BaseLayer] CheckAllPartActive");

            if (isFall)
            {
                parts.Remove(part);
                // Xóa khỏi snapshot để OnDisable không cố cleanup part đã gone
                _registeredParts.Remove(part);
            }

            IsHidden = parts.Count() == 0;

            if (IsHidden)
            {
                ClearLayer();
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // LAYER LIFECYCLE
        // ──────────────────────────────────────────────────────────────────

        public void ClearLayer()
        {
            IsLayerClear = true;
            UnregisterPartListeners(); // cleanup trước khi notify manager
            layerManager.OnLayerCleared(this);
        }

        private void GetAllPartsInLayer()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = _transform.GetChild(i).GetComponent<BasePart>();
                if (child != null)
                    parts.Add(child);
            }
        }

        public void Reset()
        {
            UnregisterPartListeners();
            parts.Clear();
            IsLayerClear = false;
            IsHidden = false;
            dropPart = 0;
        }

        public void OnValidate()
        {
            Debug.Log($"[BaseLayer] OnValidate called for {gameObject.name} — refreshing parts list. {isLayerClear}");
        }
    }
}
