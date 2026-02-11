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
        [Tooltip("Vị trí đại diện cho Special Box trên scene (optional, chỉ để move screw cho đẹp).")]
        [SerializeField] private Transform specialBoxAnchor;

        [Tooltip("Có tự ẩn screw object sau khi move vào special box không.")]
        [SerializeField] private bool hideScrewObject = true;

        [Header("Runtime")]
        [SerializeField] private Dictionary<ColorEnum, int> screwCounts = new Dictionary<ColorEnum, int>();

        public Transform SpecialBoxAnchor { get => specialBoxAnchor; set => specialBoxAnchor = value; }



        public void Init()
        {
            specialBoxAnchor.gameObject.SetActive(true);
        }
        /// <summary>
        /// Lấy tổng số screw của 1 màu đã gom vào special box.
        /// </summary>
        public int GetCount(ColorEnum color)
        {
            if (screwCounts == null || !screwCounts.ContainsKey(color))
                return 0;

            return screwCounts[color];
        }

        /// <summary>
        /// Lấy tổng toàn bộ screw đã gom.
        /// </summary>
        public int GetTotalCount()
        {
            if (screwCounts == null) return 0;

            int total = 0;
            foreach (var kvp in screwCounts)
            {
                total += kvp.Value;
            }
            return total;
        }

        /// <summary>
        /// Thêm 1 screw vào special box.
        /// </summary>
        public void AddSingle(Screw screw)
        {
            if (screw == null) return;
            AddScrews(new List<Screw> { screw });
        }

        /// <summary>
        /// Thêm nhiều screw vào special box, tự group theo màu để update UI.
        /// </summary>
        public void AddScrews(List<Screw> screws)
        {
            if (screws == null || screws.Count == 0)
                return;

            var screwMng = LevelManager.ins != null ? LevelManager.ins.ScrewManager : null;

            foreach (var screw in screws)
            {
                if (screw == null)
                    continue;

                var color = screw.Color;
                // Xoá khỏi ScrewManager nếu có
                if (screwMng != null)
                {

                    screwMng.RemoveScrew(screw);
                }

                // Tăng đếm nội bộ
                if (!screwCounts.ContainsKey(color))
                    screwCounts[color] = 0;

                screwCounts[color]++;

                // Di chuyển screw tới vị trí Special Box (nếu có đặt anchor)
                if (specialBoxAnchor != null)
                {

                    var spark = SparkVFXPool.Instance.Spawn();
                    spark.transform.position = screw.transform.position;
                    screw.FreeHinge();
                    // Tạo sequence riêng cho mỗi screw
                    Sequence seq = DOTween.Sequence();

                    // 1) Move → bay đến anchor
                    seq.Append(
                        screw.transform.DOMove(specialBoxAnchor.position, 0.45f)
                        .SetEase(Ease.InCubic)

                    );
                    seq.Join(
                        spark.transform.DOMove(specialBoxAnchor.position, 0.45f).SetEase(Ease.InCubic));
                    // 2) Pop scale (nảy nhẹ)
                    seq.Append(
                        screw.transform.DOPunchScale(Vector3.one * 1.5f, 0.12f).SetEase(Ease.OutBack)
                    );
                    seq.OnComplete(() =>
                    {
                        IngameController.ins.StarChanging(1);
                        SideMissionManager.ins.UpdateMission(1);
                        ViewManager.Instance.UpdateSpecialBoxCount(color, screwCounts[color]);
                        MissionManager.ins.ProcessCollectScrew(color, 1);

                    });


                    // 4) Ẩn object sau khi pop xong (nếu chế độ hide bật)
                    if (hideScrewObject)
                    {
                        seq.OnComplete(() =>
                        {
                            if (screw != null)
                            {
                                screw.gameObject.SetActive(false);
                                ViewManager.Instance.UpdateSpecialBoxCount(color, screwCounts[color]);
                                MissionManager.ins.ProcessCollectScrew(color, 1);
                            }
                        });
                    }
                }

            }

            Debug.Log($"[SpecialBoxManager] Added {screws.Count} screws to special box.");
        }

        public void OnReset()
        {
            screwCounts?.Clear();
        }
    }
}
