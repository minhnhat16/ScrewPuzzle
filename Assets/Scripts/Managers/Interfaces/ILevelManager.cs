using Level;
using System;
using System.Collections;

public interface ILevelManager
{
    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    int CurrentLevelId { get; }
    bool IsInitDone { get; }

    // ─────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────

    void Init(Action callback);

    // ─────────────────────────────────────────
    // Load
    // ─────────────────────────────────────────

    /// <summary>Load level từ menu, khởi động scene transition</summary>
    void LoadLevel(int levelId, Action callback = null);

    /// <summary>Reset scene về trạng thái ban đầu</summary>

    // ─────────────────────────────────────────
    // Query
    // ─────────────────────────────────────────

    Level.Level GetLevelData(int levelId);
}