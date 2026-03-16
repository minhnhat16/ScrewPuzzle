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
        public bool IsHidden { get; private set; }



        // Option 2: Call CheckAllPartActive via event (example event subscription)
        // Uncomment and use if your BasePart class exposes an event when IsFalling changes.

        private void OnEnable()
        {
            IsLayerClear =false;
        }
        private void OnDisable()
        {
            foreach (var part in parts)
            {
                part.OnStateChanged.RemoveListener(OnPartFallingChanged);
            }
        }

        public void  RegisterPartListener()
        {
            foreach (var part in parts)
            {
                part.OnStateChanged.RemoveAllListeners();
                part.OnStateChanged.AddListener(OnPartFallingChanged);
            }
        }
        private void OnPartFallingChanged(bool isFaling, BasePart part)
        {
            Debug.Log("Part falling changed: " + isFaling);
            CheckAllPartActive(isFaling,part);
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
            {
                layerManager = GetComponentInParent<LayerManager>();
            }
            dropPart = 0;
        }

        private void CheckAllPartActive(bool isFall, BasePart part)
        {
            Debug.Log("Checck all part Active");

            if (isFall)
            {
                parts.Remove(part);

            }
            IsHidden = parts.Count == 0;

            if (IsHidden)
            {

                Debug.Log("Clear Layer " + gameObject.name);
                ClearLayer();
            }
        }

        public void ClearLayer()
        {
            IsLayerClear = true;
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
            parts.Clear();
            IsLayerClear = false;
            dropPart = 0;
        }
    }
}
