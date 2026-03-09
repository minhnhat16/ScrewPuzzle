using ConfigFile;
using Core.Match;
using Ingame.Screw;
using System.Collections.Generic;

/// <summary>
/// ┌──────────────────────────────────────────────────────────┐
/// │  ILevelBoxQueue                                          │
/// │                                                          │
/// │  Mở rộng IContainerQueue với các method mà              │
/// │  LevelManager cần nhưng IContainerQueue (Core) không có. │
/// │                                                          │
/// │  Tại sao không đưa vào IContainerQueue?                  │
/// │   - IContainerQueue thuộc Core layer — không biết        │
/// │     BoxConfig, ScrewController, hay game-specific logic  │
/// │   - ISP: chỉ expose những gì caller thực sự cần         │
/// │                                                          │
/// │  BoxQueue implement cả IContainerQueue + ILevelBoxQueue  │
/// └──────────────────────────────────────────────────────────┘
/// </summary>
public interface ILevelBoxQueue : IContainerQueue
{
    // ─── Level lifecycle ───────────────────────────────────────

    /// <summary>
    /// Load BoxConfig của level → build box sequence.
    /// Gọi trong InitBoxQueueStep trước khi Init().
    /// </summary>
    void LoadBoxConfigRecord(BoxConfig boxConfig);

    /// <summary>
    /// Reset toàn bộ state: clear boxes, clear config, clear hidden.
    /// Gọi trong LevelManager.OnReset().
    /// </summary>
    void OnReset();

    /// <summary>
    /// Clear config records (box sequence) — không xóa active boxes.
    /// </summary>
    void ClearConfigRecords();

    /// <summary>
    /// Clear các box đang active.
    /// </summary>
    void ClearCurrentBoxes();

    // ─── Screw routing ─────────────────────────────────────────

    /// <summary>
    /// Nhận danh sách screw từ board, group theo màu, route vào box phù hợp.
    /// Nếu không có box nào match → screw ở lại ArrayScrew (queue).
    /// </summary>
    void TryMoveScrewsGroupedByColor(List<ScrewController> screws, bool fromBoard);

    // ─── Win condition queries ─────────────────────────────────

    /// <summary>Còn box nào trong sequence chưa spawn không.</summary>
    bool HasNext();

    /// <summary>Số box đang active trên slot.</summary>
    int ActiveBoxCount { get; }
}