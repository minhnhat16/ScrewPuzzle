using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Ingame.Board
{
    public class LayerManager : MonoBehaviour
    {
        [SerializeField] List<BaseLayer> layers = new List<BaseLayer>();
        [SerializeField]private Queue<BaseLayer> layerQueue = new Queue<BaseLayer>();

        public ApplyParentLayer applyParentLayer;
        [SerializeField] private const int MaxVisibleLayers = 3;

        private void Awake()
        {
            applyParentLayer = GetComponent<ApplyParentLayer>();
        }

        private void Start()
        {
            GetChildrenInRange();
            // Thêm tất cả layer vào hàng đợi
            foreach (var layer in layers)
            {
                layerQueue.Enqueue(layer);
            }
            //ApplyLayerVisibility(); // Thiết lập hiển thị layer ban đầu
        }

        private void Reset()
        {
        }

        private void GetChildrenInRange()
        {
            layers.Clear();
            int childCount = gameObject.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = gameObject.transform.GetChild(i);
                var baseLayer = child.GetComponent<BaseLayer>();
                if (baseLayer != null)
                {
                    layers.Add(baseLayer);
                     applyParentLayer.ApplyLayerToChildren(child.gameObject);
                   
                }
            }
        }

        // Gọi khi một layer thực hiện hành động clear
        public void OnLayerCleared(BaseLayer clearedLayer)
        {
            // Đảm bảo layer đã clear không còn trong danh sách
            if (layers.Contains(clearedLayer))
            {
                layers.Remove(clearedLayer);
            }
            // Kiểm tra và cập nhật hiển thị các layer còn lại
            //ApplyLayerVisibility();
        }

       void ApplyLayerVisibility()
        {
            // Đảm bảo hàng đợi không bị trống
            if (layerQueue.Count == 0) return;

            int activeLayerCount = 0;

            // Kiểm tra các layer hiện tại trong hàng đợi
            while (layerQueue.Count > 0)
            {
                BaseLayer currentLayer = layerQueue.Peek(); // Lấy layer đầu tiên từ queue nhưng không xoá

                if (activeLayerCount <= 2)
                {
                    // Hiển thị các layer từ 0-2 với màu sắc gốc
                    if (!currentLayer.GameObject.activeSelf)
                    {
                        currentLayer.GameObject.SetActive(true);
                    }

                    // Đảm bảo layer không bị fade nếu nằm trong khoảng từ 0-2
                    var partsInLayer = currentLayer.parts;
                    foreach (var part in partsInLayer)
                    {
                        if (part.Renderer != null)
                        {
                            StopAllCoroutines(); // Dừng tất cả coroutine fade nếu có
                            part.Renderer.color = Color.white; // Khôi phục màu sắc gốc nếu có
                        }
                        if (part.OutLine != null)
                        {
                            StopAllCoroutines(); // Dừng tất cả coroutine fade nếu có
                            part.OutLine.color = Color.white; // Khôi phục màu sắc gốc nếu có
                        }
                    }

                    activeLayerCount++;
                }
                else if (activeLayerCount <= 5)
                {
                    // Fade dần các layer từ 3-5 sang màu xám
                    if (currentLayer.GameObject.activeSelf)
                    {
                        var partsInLayer = currentLayer.parts;
                        foreach (var part in partsInLayer)
                        {
                            if (part.Renderer != null)
                            {
                                StartCoroutine(FadeToGray(part.Renderer, 0.5f));
                            }
                            if (part.OutLine != null)
                            {
                                StartCoroutine(FadeToGray(part.OutLine, 0.5f));
                            }
                        }
                    }

                    activeLayerCount++;
                }
                else
                {
                    // Tắt các layer còn lại
                    if (currentLayer.GameObject.activeSelf)
                    {
                        currentLayer.GameObject.SetActive(false);
                    }
                }

                layerQueue.Dequeue(); // Xoá layer đã xử lý

                // Nếu layer đã bị tắt, tiếp tục xử lý layer tiếp theo trong hàng đợi
                if (layerQueue.Count > 0)
                {
                    ApplyLayerVisibility(); // Gọi lại để xử lý các layer còn lại
                    break; // Dừng vòng lặp để tránh việc lặp vô tận
                }
            }
        }
        // Coroutine để làm mờ màu layer khi ẩn đi
        IEnumerator FadeToGray(SpriteRenderer spriteRenderer, float duration)
        {
            // Bắt đầu với màu xám nhưng có alpha bằng 0 (trong suốt)
            Color startColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0f);
            Color endColor = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.5f);; // Màu xám với alpha = 1 (đầy đủ)

            float elapsedTime = 0f;

            // Lặp cho đến khi đạt thời gian fade
            while (elapsedTime < duration)
            {
                // Thay đổi alpha và màu sắc dần dần từ trong suốt sang màu xám
                spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Đảm bảo spriteRenderer có màu xám đầy đủ sau khi hoàn tất
            spriteRenderer.color = endColor;
        }
    }
}
