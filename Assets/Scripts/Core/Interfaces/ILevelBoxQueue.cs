using Core.Match;
using ConfigFile;

public interface ILevelBoxQueue : IContainerQueue
{
    // ─── Level lifecycle ───────────────────────────────────────

    /// <summary>
    /// True nếu Setup() đã được gọi — factory, sequence, layout đều sẵn sàng.
    /// Guard trong InitBoxQueueStep để phát hiện sớm thứ tự khởi tạo sai.
    /// </summary>
    bool IsReady { get; }

    void LoadBoxConfigRecord(BoxConfig boxConfig);
    int ActiveBoxCount { get; }
    void ClearConfigRecords();
    void ClearCurrentBoxes();
    void OnReset();
}