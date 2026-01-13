using DG.Tweening;
using Ingame.Board;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public static void ActiveObjectInLayer(bool isOn, int layer, LayerManager lm, bool isInactive = false)
        {

            if (layer < 0 || layer >= lm.Layers.Count) return;
            List<BasePart> parts = lm.Layers[layer].parts;
            List<string> idParts = parts.Where(p => p != null)
                                        .Select(p => p.uniqueID)
                                        .ToList();
            var idLayerTostring = LayerMask.LayerToName(layer + 10);
            var listScrews = lm.screwDict.GetValueOrDefault(layer);

            if (listScrews == null) return;
            List<Screw.Screw> screwsActives = listScrews.Where(s => s != null).ToList();

            if (screwsActives == null) return;
            foreach (var s in screwsActives)
            {
                s.gameObject.SetActive(isOn);
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
        public static void FadeSprite(SpriteRenderer old, Sprite newSprite, float time = 0.5f)
        {
            // Fade out
            old.DOFade(0f, time)
                .OnComplete(() =>
                {
                    old.sprite = newSprite; // đổi sprite sau khi ẩn
                    old.DOFade(1f, time); // fade in lại
                });
        }
    }
}
