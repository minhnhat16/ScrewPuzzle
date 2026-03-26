using DG.Tweening;
using Ingame.Board;
using Ingame.Screw;
using System.Collections.Generic;
using UnityEngine;

namespace Ingame
{
    public static class LayerUtils
    {
        /// <summary>
        /// Áp dụng layer của cha cho tất cả con (bỏ qua object có Screw)
        /// </summary>
        public static void ApplyParentLayer(GameObject parent)
        {
            if (parent == null) return;

            int parentLayer = parent.layer;

            foreach (Transform child in parent.transform)
            {
                SetLayerRecursively(child.gameObject, parentLayer);
            }
        }

        /// <summary>
        /// Activate/deactivate screws using a BaseLayer reference.
        /// Khi deactivate: freeze Rigidbody của BasePart trước để tránh part rớt
        /// khi layer GameObject bị SetActive(false).
        /// </summary>
        public static void ActiveObjectInLayer(bool isOn, BaseLayer layer, LayerManager lm)
        {
            if (lm == null || layer == null) return;
            if (lm.Layers == null) return;

            int index = lm.Layers.IndexOf(layer);
            if (index < 0)
            {
                Debug.LogWarning($"[LayerUtils] ActiveObjectInLayer: layer not found in LayerManager.Layers: {layer.name}");
                return;
            }

            // Deactivate: freeze parts TRƯỚC khi tắt screw
            // Activate: KHÔNG unfreeze ở đây — HingeController.OnEnable → InitHingeJoints
            //           sẽ unfreeze từng part SAU KHI hinge reconnect thành công
            if (!isOn)
                FreezeLayerParts(layer);

            ActiveObjectInLayer(isOn, index, lm);
        }

        public static void ActiveObjectInLayer(bool isOn, int layer, LayerManager lm)
        {
            if (lm == null || lm.Layers == null) return;
            if (layer < 0 || layer >= lm.Layers.Count) return;

            if (!lm.screwDict.TryGetValue(layer, out var screws) || screws == null || screws.Count == 0)
                return;

            // Resolve sorting layer name theo index
            // Layer 0 (top) = "Part0", layer 1 = "Part1", v.v.
            int trueLayer = layer + 1; // Layer index bắt đầu từ 0, nhưng sorting layer bắt đầu từ 1
            string sortingLayerName = $"Layer {trueLayer}";

            foreach (var screw in screws)
            {
                if (screw == null) continue;
                if (screw.IsDetachedFromBoard) continue;

                var hinge = screw.hingeController;
                var body = hinge.BodyConnect;

                if (isOn)
                {
                    if (body != null && !body.gameObject.activeSelf)
                    {
                        Debug.Log($"[LayerUtils] Skip Activate Screw ({screw.name}) — connected part inactive: {body.name}");
                        screw.SetActive(false);
                        continue;
                    }

                    screw.SetActive(true);

                    // Set sorting layer cho part sau khi activate
                    if (body != null && body.TryGetComponent<BasePart>(out var part))
                    {
                        part.SetSortingLayer(sortingLayerName);
                        part.SetSpriteAlpha(0.8f);
                    }
                }
                else
                {
                    string bodyName = body != null ? body.name : "NULL";
                    Debug.Log($"[LayerUtils] Deactivate Screw ({screw.name}) — connected body: {bodyName}");
                    screw.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Freeze tất cả BasePart trong layer trước khi ẩn —
        /// ngăn Rigidbody simulate khi GameObject wake up lại.
        /// Gọi trước SetActive(false) trên layer GameObject.
        /// </summary>
        public static void FreezeLayerParts(BaseLayer layer)
        {
            if (layer == null || layer.parts == null) return;

            foreach (var part in layer.parts)
            {
                if (part == null || part.Body == null) continue;
                part.Body.bodyType = RigidbodyType2D.Kinematic;
                part.Body.linearVelocity = Vector2.zero;
                part.Body.angularVelocity = 0f;
            }

            Debug.Log($"[LayerUtils] FreezeLayerParts — frozen {layer.parts.Count} parts in layer '{layer.name}'");
        }

        /// <summary>
        /// Unfreeze BasePart trong layer sau khi SetActive(true) và hinge đã kết nối.
        /// </summary>
        public static void UnfreezeLayerParts(BaseLayer layer)
        {
            if (layer == null || layer.parts == null) return;

            foreach (var part in layer.parts)
            {
                if (part == null || part.Body == null) continue;
                part.Body.bodyType = RigidbodyType2D.Dynamic;
                part.Body.gravityScale = 1f;
            }

            Debug.Log($"[LayerUtils] UnfreezeLayerParts — unfrozen {layer.parts.Count} parts in layer '{layer.name}'");
        }

        /// <summary>
        /// Set tất cả parts về Kinematic — dùng trước khi SetActive(true)
        /// để tránh physics simulate ngay khi wake up.
        /// </summary>
        public static void SetAllKinematic(List<BasePart> parts)
        {
            if (parts == null) return;
            foreach (var part in parts)
            {
                if (part == null || part.Body == null) continue;
                part.Body.bodyType = RigidbodyType2D.Kinematic;
                part.Body.linearVelocity = Vector2.zero;
                part.Body.angularVelocity = 0f;
            }
        }

        /// <summary>
        /// Set tất cả parts về Dynamic sau khi hinge đã connect và transform đã settle.
        /// gravityScale vẫn giữ 0 — hinge chịu trách nhiệm giữ part.
        /// </summary>
        public static void SetAllDynamic(List<BasePart> parts)
        {
            if (parts == null) return;
            foreach (var part in parts)
            {
                if (part == null || part.Body == null) continue;
                part.Body.bodyType = RigidbodyType2D.Dynamic;
                part.Body.gravityScale = 1f;
            }
        }

        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj.CompareTag("Screw") || obj.GetComponent<ScrewController>() != null)
                return;

            if (obj.layer != newLayer)
                obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static void FadeSprite(SpriteRenderer r, Sprite newSprite, float time = 0.5f)
        {
            if (r == null) return;

            r.DOKill();

            r.DOFade(0f, time)
             .OnComplete(() =>
             {
                 r.sprite = newSprite;
                 r.DOFade(1f, time);
             });
        }

        internal static void RemoveScrew(ScrewController screw, LayerManager lm)
        {
            if (screw == null || lm == null) return;
            screw.FreeHinge();
            lm.RemoveScrewOnDict(screw, screw.GetSortingOrder());
        }

        public static void RemoveScrews(List<ScrewController> screws, LayerManager lm)
        {
            if (screws == null || lm == null) return;

            foreach (var screw in screws)
                RemoveScrew(screw, lm);
        }
    }
}
