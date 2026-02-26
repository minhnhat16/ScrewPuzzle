using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Match
{
    /// <summary>
    /// Contract cho nơi nhận và chứa IMatchItem.
    /// Box (screw game), Cage (cat game), Pile (card game)...
    ///
    /// Container không biết item là gì — chỉ biết Tag và số lượng.
    /// </summary>
    public interface IMatchContainer
    {
        // ─────────────────────────────────────────
        // Identity
        // ─────────────────────────────────────────

        /// <summary>
        /// Tag mà container này chấp nhận.
        /// Box màu đỏ → "red"
        /// Cage cho cat cam → "cat_orange"
        /// </summary>
        string AcceptedTag { get; }

        // ─────────────────────────────────────────
        // State
        // ─────────────────────────────────────────

        int Count { get; }
        int Capacity { get; }
        int RemainingCapacity => Capacity - Count;

        bool IsFull { get; }
        bool IsLocked { get; }
        bool IsMoving { get; }  // đang trong animation, chưa nhận được

        // ─────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────

        /// <summary>Fire khi container nhận đủ item và hoàn thành</summary>
        event Action<IMatchContainer> OnCompleted;

        // ─────────────────────────────────────────
        // Operations
        // ─────────────────────────────────────────

        /// <summary>
        /// Thêm 1 item vào container.
        /// Trả về true nếu thành công.
        /// </summary>
        bool TryAdd(IMatchItem item);

        /// <summary>
        /// Thêm nhiều item cùng lúc.
        /// Trả về số item đã add được.
        /// </summary>
        int TryAddRange(IEnumerable<IMatchItem> items);

        // ─────────────────────────────────────────
        // Queries
        // ─────────────────────────────────────────

        /// <summary>Vị trí world để item animate tới</summary>
        Vector3 Position { get; }
    }
}