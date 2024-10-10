using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ingame.Board
{
    public  class BaseLayer : MonoBehaviour
    {
        public LayerManager layerManager;
        [SerializeField] private Transform _transform;
        [SerializeField] private GameObject _gameObject;
        public List<BasePart> parts;
        [SerializeField] private int activePartCount = 0;
        [SerializeField] private string layer;
        public Rigidbody2D ridBody;
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
            get => activePartCount;
            set => activePartCount = value;
        }
       private void Awake()
       {
           _transform = GetComponent<Transform>();
           _gameObject= _transform.gameObject;
           GetAllPartsInLayer();
       }

       private void Start()
        {
            if (layerManager == null)
            {
                layerManager = GetComponentInParent<LayerManager>();
            }
            activePartCount = 0;
            foreach (var part in parts)
            {
                part.OnStateChanged += CheckAllPartActive;
                layer =  LayerMask.LayerToName(gameObject.layer);
                part.SetSortingLayer(layer);
            }
        }

        private void CheckAllPartActive()
        {
            activePartCount = 0;
            foreach (var part in parts)
            {
                if (part.IsFalling)
                {
                    activePartCount++;
                }
            }

            if (activePartCount >= parts.Count)
            {
                ClearLayer();
            }
        }

        public void ClearLayer()
        {
            // Ẩn hoặc xóa layer này
            gameObject.SetActive(false);

            // Thông báo cho LayerManager rằng layer này đã bị clear
            if (layerManager != null)
            {
                layerManager.OnLayerCleared(this);
            }
        }

        private void GetAllPartsInLayer()
        {
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = _transform.GetChild(i).GetComponent<BasePart>();
                parts.Add(child);
            }
        }
    }
}
