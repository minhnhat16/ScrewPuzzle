using Enums;
using Ingame.Screw;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Ingame
{
    /// <summary>
    /// Contract cho ArrayScrew — khu vực hold screw tạm thời của player.
    ///
    /// Nguyên tắc:
    /// - Chỉ expose những gì system bên ngoài thực sự cần
    /// - Không expose internal (HoldScrew, alignment...)
    /// - Event thay cho callback trực tiếp vào IngameController
    /// </summary>
    public interface IArrayScrew
    {
        // ─────────────────────────────────────────
        // State
        // ─────────────────────────────────────────

        /// <summary>Số hold đang active</summary>
        int ActiveHoldCount { get; }

        /// <summary>Còn ít nhất 1 screw trong array</summary>
        bool HasAny();

        /// <summary>Tất cả hold đang active đều có screw</summary>
        bool IsFull { get; }

        // ─────────────────────────────────────────
        // Events (thay thế callback vào IngameController)
        // ─────────────────────────────────────────

        /// <summary>
        /// Fire khi tất cả hold đầy và box queue không có box đang moving.
        /// IngameController lắng nghe → TriggerArrayScrewFull()
        /// </summary>
        event Action OnArrayFull;

        // ─────────────────────────────────────────
        // Screw operations
        // ─────────────────────────────────────────

        /// <summary>Thêm 1 screw vào hold trống đầu tiên</summary>
        void AddScrew(ScrewController screw);

        /// <summary>Xoá screw khỏi hold (dùng khi screw di chuyển vào box)</summary>
        void RemoveScrew(ScrewController screw);

        /// <summary>Xoá danh sách screw khỏi hold</summary>
        void RemoveScrews(IEnumerable<ScrewController> screws);

        /// <summary>Xoá tất cả screw, return về pool</summary>
        void Clear();

        /// <summary>
        /// Ẩn screw hiện tại và chuyển về hidden queue trong ScrewManager.
        /// Dùng khi Magnet item được activate.
        /// </summary>
        IEnumerator ClearToHidden();

        // ─────────────────────────────────────────
        // Hold operations
        // ─────────────────────────────────────────

        /// <summary>Thêm 1 hold mới (Drill item)</summary>
        void AddOneHold();

        /// <summary>
        /// Khởi tạo hiển thị với số hold active cụ thể.
        /// Dùng khi bắt đầu level.
        /// </summary>
        void ShowArrayActive(int activeCount);

        // ─────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────

        /// <summary>Màu xuất hiện nhiều nhất trong array hiện tại</summary>
        ColorEnum GetDominantColor();

        /// <summary>Position của hold cuối cùng (dùng cho item effect UI)</summary>
        UnityEngine.Vector3 GetLastHoldPosition();
    }
}