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

        public static void ActiveObjectInLayer(bool isOn, int layer, LayerManager lm)
        {
            if (lm == null) return;
            if (layer < 0 || layer >= lm.Layers.Count) return;

            if (!lm.screwDict.TryGetValue(layer, out var screws) || screws == null || screws.Count == 0)
                return;

            foreach (var screw in screws)
            {
                if (screw != null)
                    screw.gameObject.SetActive(isOn);
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
