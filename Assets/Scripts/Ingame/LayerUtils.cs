using DG.Tweening;
using Ingame.Board;
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
        /// Activate/deactivate screws using a BaseLayer reference (keeps index mapping stable).
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

            ActiveObjectInLayer(isOn, index, lm);
        }

        public static void ActiveObjectInLayer(bool isOn, int layer, LayerManager lm)
        {
            if (lm == null) return;
            if (lm.Layers == null) return;
            if (layer < 0 || layer >= lm.Layers.Count) return;

            if (!lm.screwDict.TryGetValue(layer, out var screws) || screws == null || screws.Count == 0)
                return;

            foreach (var screw in screws)
            {
                if (screw == null) continue;

                // If we're turning objects ON, do not activate screws whose connected part is inactive.
                if (isOn)
                {
                    var hinge = screw.HingeController;
                    if (hinge != null)
                    {
                        var body = hinge.BodyConnect;
                        if (body != null && !body.gameObject.activeSelf)
                        {
                            Debug.Log($"[LayerUtils] Skipping screw activation because connected part is inactive: {body.name}");
                            screw.gameObject.SetActive(false);
                            // Part is inactive → skip enabling this screw
                            continue;
                        }

                        // Part is active (or no body) → enable screw
                        screw.gameObject.SetActive(true);
                        continue;
                    }

                    // No hinge controller → enable screw
                    screw.gameObject.SetActive(true);
                }
                else
                {
                    // Always allow disabling screws regardless of part state
                    // Guard against null HingeController when logging
                    var bodyName = screw.HingeController?.BodyConnect?.gameObject?.name ?? "unknown";
                    Debug.Log($"[LayerUtils] Deactivating screw (connected body: {bodyName})");
                    screw.gameObject.SetActive(false);
                }
            }
        }


        private static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            // Bỏ qua nếu là Screw hoặc có component Screw
            if (obj.CompareTag("Screw") || obj.GetComponent<Screw.Screw>() != null)
                return;

            // Nếu layer đã đúng thì bỏ qua để tránh gọi thừa
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

            r.DOKill(); // tránh overlap tween

            r.DOFade(0f, time)
             .OnComplete(() =>
             {
                 r.sprite = newSprite;
                 r.DOFade(1f, time);
             });
        }


        internal static void RemoveScrew(Screw.Screw screw, LayerManager lm)
        {
            if (screw == null || lm == null) return;
            screw.FreeHinge();
            lm.RemoveScrewOnDict(screw, screw.sortingOrder);
        }

        public static void RemoveScrews(List<Screw.Screw> screws, LayerManager lm)
        {
            if (screws == null || lm == null) return;

            foreach (var screw in screws)
                RemoveScrew(screw, lm);
        }

    }
}