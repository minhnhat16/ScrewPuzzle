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

        [Header("Runtime")]
        [SerializeField] private Dictionary<ColorEnum, int> screwCounts = new();

        /// <summary> Event khi tổng số screw của một màu thay đổi. </summary>
        public event System.Action<ColorEnum, int> OnBoxColorCountChanged;

        public Transform SpecialBoxAnchor { get => specialBoxAnchor; set => specialBoxAnchor = value; }

        //======================================================================//
        // INITIALIZATION
        //======================================================================//

        public void Init()
        {
            if (specialBoxAnchor != null)
                specialBoxAnchor.gameObject.SetActive(true);
            screwCounts.Clear();
        }

        //======================================================================//
        // PUBLIC API
        //======================================================================//

        public int GetCount(ColorEnum color) => screwCounts.TryGetValue(color, out var count) ? count : 0;

        public int GetTotalCount()
        {
            int total = 0;
            foreach (var kv in screwCounts) total += kv.Value;
            return total;
        }

        public void AddSingle(ScrewController screw)
        {
            if (screw != null)
                AddScrews(new List<ScrewController> { screw });
        }

        /// <summary>Thêm nhiều screw vào SpecialBox và tự xử lý animation, logic và đếm.</summary>
        public void AddScrews(List<ScrewController> screws)
        {
            if (screws == null || screws.Count == 0) return;

            var screwMng = LevelManager.ins?.ScrewManager;

            foreach (var screw in screws)
            {
                if (screw == null) continue;

                var color = screw.GetColor();
                screwMng?.RemoveScrew(screw);

                // Cập nhật đếm theo màu
                if (!screwCounts.ContainsKey(color))
                    screwCounts[color] = 0;

                screwCounts[color]++;
                OnBoxColorCountChanged?.Invoke(color, screwCounts[color]);

                // Animation collector
                if (specialBoxAnchor != null)
                    AnimateToBox(screw, color);
            }

            Debug.Log($"[SpecialBoxManager] Added {screws.Count} screws to special box.");
        }

        //======================================================================//
        // PRIVATE ANIMATION HANDLER
        //======================================================================//

        private void AnimateToBox(ScrewController screw, ColorEnum color)
        {
            if (screw == null) return;

            var spark = SparkVFXPool.Instance.Spawn();
            spark.transform.position = screw.transform.position;

            screw.FreeHinge();

            // Sequence animation to SpecialBox
            Sequence seq = DOTween.Sequence();

            seq.Append(screw.transform.DOMove(specialBoxAnchor.position, 0.45f).SetEase(Ease.InCubic));
            seq.Join(spark.transform.DOMove(specialBoxAnchor.position, 0.45f).SetEase(Ease.InCubic));

            seq.Append(screw.transform.DOPunchScale(Vector3.one * 1.5f, 0.12f).SetEase(Ease.OutBack));

            seq.OnComplete(() =>
            {
                // Update UI & mission
                IngameController.ins.StarChanging(1);
                SideMissionManager.ins.UpdateMission(1);
                ViewManager.Instance.UpdateSpecialBoxCount(color, screwCounts[color]);
                MissionManager.ins.ProcessCollectScrew(color, 1);

                if (hideScrewObject && screw != null)
                    screw.gameObject.SetActive(false);
            });

            seq.SetLink(screw.gameObject); // Nếu Screw bị destroy → DOTween tự dừng
        }

        //======================================================================//
        // RESET
        //======================================================================//

        public void OnReset()
        {
            DOTween.Kill(this); // Dừng mọi tween liên quan
            screwCounts.Clear();
        }
    }
}
