using System;
using System.Collections;
using System.Collections.Generic;

namespace Core.Match
{
    /// <summary>
    /// Contract cho hàng đợi tạm — nơi item chờ trước khi vào container.
    ///
    /// ArrayScrew (screw game) → implement interface này.
    /// CatQueue   (cat game)   → implement interface này.
    /// Hand       (card game)  → implement interface này.
    ///
    /// Queue không biết item là loại cụ thể gì, không biết container là gì.
    /// Nó chỉ biết: nhận item → thử route → nếu không được thì giữ lại.
    /// </summary>
    public interface ITempQueue
    {
        // ─────────────────────────────────────────
        // State
        // ─────────────────────────────────────────

        int ActiveSlotCount { get; }
        bool IsFull { get; }
        bool HasAny { get; }

        // ─────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────

        /// <summary>
        /// Fire khi tất cả slot đầy và không có container nào đang di chuyển.
        /// IngameController lắng nghe để trigger GameOver/RevivePrompt.
        /// </summary>
        event Action OnQueueFull;

        // ─────────────────────────────────────────
        // Item operations
        // ─────────────────────────────────────────

        /// <summary>Nhận item từ world và xử lý routing</summary>
        void Enqueue(IMatchItem item);

        /// <summary>Xoá item khỏi slot (khi item đã vào container)</summary>
        void Dequeue(IMatchItem item);

        /// <summary>Xoá tất cả, return về pool</summary>
        void Clear();

        /// <summary>Ẩn và chuyển về hidden storage (Magnet item)</summary>
        IEnumerator ClearToHidden();

        // ─────────────────────────────────────────
        // Slot operations
        // ─────────────────────────────────────────

        /// <summary>Thêm 1 slot mới (Drill item)</summary>
        void AddSlot();

        /// <summary>Khởi tạo với số slot cụ thể khi start level</summary>
        void SetupSlots(int count);
    }
}