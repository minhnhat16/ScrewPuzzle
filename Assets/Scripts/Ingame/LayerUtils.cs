using DG.Tweening;
using Ingame.Board;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static void ActiveObjectInLayer(bool isOn, int level, LayerManager lm)
        {
            Debug.Log("Screws in layer " + level);
            if (level < 0 || level >= lm.Layers.Count) return;
                List <BasePart> parts = lm.Layers[level].parts;
            List<string> idParts = parts.Where(p => p != null)
                                        .Select(p => p.uniqueID)
                                        .ToList();
            var idLayerTostring = LayerMask.LayerToName(level + 10);
            List<Screw.Screw> screwsActives= lm.screwDict.GetValueOrDefault(level);
            Debug.Log("Screws in layer " + level + ": " + (screwsActives != null ? screwsActives.Count.ToString() : "null"));
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
        public static void FadeSprite(SpriteRenderer old,Sprite newSprite, float time = 0.5f)
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
