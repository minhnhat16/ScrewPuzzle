using System;
using System.Collections.Generic;
using Enums;
using Ingame.Screw;

namespace Core.Match
{
    /// <summary>
    /// Quản lý danh sách container active (Box, Cage...).
    /// Spawn mới khi container cũ hoàn thành, route item vào đúng container.
    /// </summary>
    public interface IContainerQueue
    {

        // ─────────────────────────────────────────
        // State
        // ─────────────────────────────────────────

        int ActiveCount { get; }

        // ─────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────

        event Action<IMatchContainer> OnContainerCompleted;
        event Action<IMatchContainer> OnContainerSpawned;
        event Action<IMatchContainer> OnContainerRemoved;

        // ─────────────────────────────────────────
        // Setup
        // ─────────────────────────────────────────
        public void Setup(IBoxFactory factory, IBoxSequenceService sequence, IBoxSlotLayoutService layout);
        // ─────────────────────────────────────────
        // Lifecycle
        // ─────────────────────────────────────────

        void Initialize(bool isTutorial);
        void Reset();

        // ─────────────────────────────────────────
        // Routing
        // ─────────────────────────────────────────

        /// <summary>
        /// Tìm container phù hợp với tag.
        /// Trả về null nếu không có container nào available.
        /// </summary>
        IMatchContainer FindSuitable(string tag);

        /// <summary>
        /// Add item thẳng vào container đã biết.
        /// Trả về true nếu thành công.
        /// </summary>
        bool AddItemToContainer(IMatchItem item, IMatchContainer container);

        // ─────────────────────────────────────────
        // Container management
        // ─────────────────────────────────────────

        void NotifyCompleted(IMatchContainer container);
        void UnlockNext();
        bool HasLocked();
        bool HasMovingBox();
    }
}
