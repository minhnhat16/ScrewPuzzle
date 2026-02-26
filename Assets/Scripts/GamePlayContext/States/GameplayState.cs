namespace Gameplay.StateMachine
{
    /// <summary>
    /// Tất cả trạng thái có thể có trong một phiên gameplay.
    /// Mỗi state là duy nhất - không bao giờ có 2 state active cùng lúc.
    /// </summary>
    public enum GameplayState
    {
        /// <summary>Chưa khởi tạo, chờ load level</summary>
        Idle,

        /// <summary>Đang load level data, spawn object</summary>
        Loading,

        /// <summary>Player đang chơi bình thường</summary>
        Playing,

        /// <summary>Game bị tạm dừng (pause menu)</summary>
        Paused,

        /// <summary>Player thắng, đang hiện win dialog</summary>
        Win,

        /// <summary>Player thua, đang hiện lose dialog</summary>
        Lose,
            
        /// <summary>Đang chờ player quyết định revive hay không</summary>
        RevivePrompt,

        /// <summary>Player dùng item (Breaker, Magnet...) — input mode đặc biệt</summary>
        ItemUsing,
    }
}
