using Ingame;
using Ingame.Board;
using Ingame.Screw;
using Level;
using UnityEngine;

namespace LevelSystem.Core
{
    /// <summary>
    /// Context object truyền dữ liệu qua toàn bộ pipeline load level.
    /// Thay vì LevelManager giữ state scattered, tất cả tập trung tại đây.
    /// </summary>
    public class LevelContext
    {
        public int LevelId { get; set; }
        public Level.Level LevelData { get; set; }
        public BaseLevelObject LevelObject { get; set; }
        public LayerManager LayerManager { get; set; }
        public ScrewManager ScrewManager { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string ErrorMessage { get; set; }
    }
}