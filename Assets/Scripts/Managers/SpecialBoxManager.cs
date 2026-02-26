using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Ingame.Screw;
using UnityEngine;

namespace Managers
{
    public class SpecialBoxManager : SingletonMono<SpecialBoxManager>, IResetable
    {
        [Header("Special Box Settings")]
        [Tooltip("Vị trí đại diện cho Special Box trên scene.")]
        [SerializeField] private Transform specialBoxAnchor;

        [Tooltip("Tự ẩn Screw sau khi vào Special Box.")]
        [SerializeField] private bool hideScrewObject = true;

        // Runtime data
        private Dictionary<ColorEnum, int> screwCounts = new();

        /// <summary>
        /// Event khi số lượng screw của một màu thay đổi.
        /// </summary>
        public event System.Action<ColorEnum, int> OnBoxColorCountChanged;

        /// <summary>
        /// Event khi 1 screw được collect vào box.
        /// </summary>
        public event System.Action<ColorEnum> OnScrewCollected;

        public Transform SpecialBoxAnchor => specialBoxAnchor;

        //======================================================================
        // INITIALIZATION
        //======================================================================

        public void Init()
        {
            if (specialBoxAnchor != null)
                specialBoxAnchor.gameObject.SetActive(true);

            screwCounts.Clear();
        }

        //======================================================================
        // PUBLIC API
        //======================================================================

        public int GetCount(ColorEnum color)
        {
            return screwCounts.TryGetValue(color, out var count) ? count : 0;
        }

        public int GetTotalCount()
        {
            int total = 0;
            foreach (var kv in screwCounts)
                total += kv.Value;

            return total;
        }

        public void AddSingle(ScrewController screw)
        {
            if (screw == null) return;
            AddScrews(new List<ScrewController> { screw });
        }

        /// <summary>
        /// Thêm nhiều screw vào SpecialBox và tự xử lý animation + đếm.
        /// </summary>
        public void AddScrews(List<ScrewController> screws)
        {
            if (screws == null || screws.Count == 0)
                return;

            var screwMng = LevelManager.ins?.ScrewManager;

            foreach (var screw in screws)
            {
                if (screw == null) continue;

                var color = screw.GetColor();

                // Remove khỏi level
                screwMng?.RemoveScrew(screw);

                // Update count
                if (!screwCounts.ContainsKey(color))
                    screwCounts[color] = 0;

                screwCounts[color]++;
                OnBoxColorCountChanged?.Invoke(color, screwCounts[color]);

                // Fire gameplay event
                OnScrewCollected?.Invoke(color);

                // Animation
                if (specialBoxAnchor != null)
                    AnimateToBox(screw);
            }

            Debug.Log($"[SpecialBoxManager] Added {screws.Count} screws to special box.");
        }

        //======================================================================
        // PRIVATE ANIMATION
        //======================================================================

        private void AnimateToBox(ScrewController screw)
        {
            if (screw == null) return;

            var spark = SparkVFXPool.Instance.Spawn();
            spark.transform.position = screw.transform.position;

            screw.FreeHinge();

            Sequence seq = DOTween.Sequence();

            seq.Append(
                screw.transform
                    .DOMove(specialBoxAnchor.position, 0.45f)
                    .SetEase(Ease.InCubic));

            seq.Join(
                spark.transform
                    .DOMove(specialBoxAnchor.position, 0.45f)
                    .SetEase(Ease.InCubic));

            seq.Append(
                screw.transform
                    .DOPunchScale(Vector3.one * 1.5f, 0.12f)
                    .SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                if (hideScrewObject && screw != null)
                    screw.gameObject.SetActive(false);
            });

            // Nếu screw bị destroy thì tween tự kill
            seq.SetLink(screw.gameObject);
        }

        //======================================================================
        // RESET
        //======================================================================

        public void OnReset()
        {
            screwCounts.Clear();
        }
    }
}